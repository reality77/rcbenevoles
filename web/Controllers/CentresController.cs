using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using web.Models;

namespace web.Controllers
{
    [Authorize]
    public class CentresController : RCBenevoleController
    {
        public CentresController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Centres
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Centres.ToListAsync());
        }

        [Authorize(Roles = "BasicAdmin")]
        public async Task<IActionResult> My()
        {
            var user = GetCurrentUser();

            return await Details(user.Centre.ID);
        }

        // GET: Centres/Details/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres
                .SingleOrDefaultAsync(m => m.ID == id);
            if (centre == null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // GET: Centres/Create
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Centres/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([Bind("ID,Nom,Adresse,SiegeID")] Centre centre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(centre);
                await _context.SaveChangesAsync();
                LogInfo("Centre #{CentreID} ({Centre}) créé", centre.ID, centre.Nom);
                SetGlobalMessage("Le centre a été créé avec succès", EGlobalMessageType.Success);

                return RedirectToAction(nameof(Index));
            }
            return View(centre);
        }

        // GET: Centres/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres.SingleOrDefaultAsync(m => m.ID == id);
            if (centre == null)
            {
                return NotFound();
            }
            return View(centre);
        }

        // POST: Centres/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nom,Adresse,SiegeID")] Centre centre)
        {
            if (id != centre.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centre);
                    await _context.SaveChangesAsync();
                    LogInfo("Centre #{CentreID} ({Centre}) modifié", centre.ID, centre.Nom);
                    SetGlobalMessage("Le centre a été modifié avec succès", EGlobalMessageType.Success);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentreExists(centre.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(centre);
        }

        // GET: Centres/Delete/5
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centre = await _context.Centres
                .SingleOrDefaultAsync(m => m.ID == id);

            if (centre == null)
                return NotFound();

            return View(centre);
        }

        // POST: Centres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var centre = await _context.Centres.SingleOrDefaultAsync(m => m.ID == id);
            _context.Centres.Remove(centre);
            await _context.SaveChangesAsync();

            LogInfo("Centre #{CentreID} ({Centre}) supprimé", centre.ID, centre.Nom);
            SetGlobalMessage("Le centre a été supprimé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Centres/Details/{id}/ImpressionPresencesIndex")]
        public async Task<IActionResult> PrintPresenceHoursIndex(int id)
        {
            var userCentreId = GetCurrentUser().CentreID;
            if (userCentreId != null && id != userCentreId)
                return Forbid();

            var centre = await _context.Centres.SingleOrDefaultAsync(c => c.ID == id);

            var query = _context.Pointages
                .Include(p => p.Adresse).ThenInclude(a => a.Centre)
                .Where(p => p.Adresse.CentreID == id);

            var model = new PrintHoursPresenceIndexModel()
            {
                Centre = centre,
            };

            if(!query.Any())
            {
                model.Centre = centre;
                model.Periods = null;
                return View(model);
            }

            var start = query.Min(p => p.Date);
            var end = query.Max(p => p.Date);
            
            int startPeriodId;
            DateTime periodStart;
            DateTime periodEnd;

            if(start == DateTime.MinValue)
                return View(model);
            else
            {
                if(start.Month >= 5)
                {
                    start = new DateTime(start.Year, 5, 1);
                    startPeriodId = 2;
                }
                else
                {
                    start = new DateTime(start.Year, 1, 1);
                    startPeriodId = 1;
                }
            }

            for(int year = start.Year; year <= end.AddDays(-1).Year; year++)
            {
                for(int periodId = startPeriodId; periodId <= 2; periodId++)
                {
                    switch(periodId)
                    {
                        case 1:
                            {
                                periodStart = new DateTime(year, 1, 1);
                                periodEnd = new DateTime(year, 5, 1);
                            }
                            break;
                        case 2:
                            {
                                periodStart = new DateTime(year, 5, 1);
                                periodEnd = new DateTime(year + 1, 1, 1);
                            }
                            break;
                        default:
                            return BadRequest("Période invalide");
                    }

                    if(periodStart >= end)
                        break;

                    var period = new PrintIndexPeriod
                    {
                        PeriodId = periodId,
                        Start = periodStart,
                        End = periodEnd,
                    };

                    model.Periods.Add(period);
                }

                startPeriodId = 1;
            }

            return View(model);
        }

        [HttpGet("Centres/Details/{id}/ImpressionPresences")]
        public async Task<IActionResult> PrintPresenceHours(int id, int period, int year)
        {
            var userCentreId = GetCurrentUser().CentreID;
            if (userCentreId != null && id != userCentreId)
                return Forbid();

            var centre = await _context.Centres.SingleOrDefaultAsync(c => c.ID == id);

            DateTime periodStart, periodEnd;

            try
            {
                (periodStart, periodEnd) = GetPeriodDates(period, year);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var presences = _context.Pointages
                .Include(p => p.Benevole)
                .Include(p => p.Adresse)
                .Where(p => p.Adresse.CentreID == id)
                .Where(p => p.Date >= periodStart && p.Date < periodEnd)
                .ToList()   // Fix exception
                .GroupBy(p => p.Benevole);

            int coefHours = 4;

            var model = new PrintCentrePresenceModel();
            model.Presences = new List<(Benevole benevole, int nballers, int heures)>();
            model.Centre = centre;
            model.PeriodStart = periodStart;
            model.PeriodEnd = periodEnd;

            foreach(var pres in presences)
                model.Presences.Add((pres.Key, pres.Sum(p => p.NbDemiJournees), pres.Sum(p => p.NbDemiJournees) * coefHours));

            return View(model);
        }

        private bool CentreExists(int id)
        {
            return _context.Centres.Any(e => e.ID == id);
        }
    }
}

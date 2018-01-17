﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dal;
using dal.models;
using web.Models;
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    [Authorize]
    public class PointagesController : RCBenevoleController
    {
        public PointagesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Pointages/Details/5
        [HttpGet("Pointages/Benevole/{id}")]
        public async Task<IActionResult> Benevole(int id, int? year, int? month, bool partial = false)
        {
            var now = DateTime.Now;

            if (year == null)
                year = now.Year;

            if (month == null)
                month = now.Month;

            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if(benevole == null)
                return NotFound();

            var centreGere = GetCurrentUser().Centre;

            if (centreGere != null && !benevole.Adresses.Any(a => a.CentreID == centreGere.ID))
                return Forbid();

            var model = new PointagesBenevoleModel
            {
                Centre = centreGere,
                Benevole = benevole,
                MonthDate = new DateTime(year.Value, month.Value, 1)
            };

            var dateFrom = new DateTime(year.Value, month.Value, 1);

            int dayOfWeekStartMonday = (int)dateFrom.DayOfWeek - 1;
            if (dayOfWeekStartMonday == -1)
                dayOfWeekStartMonday = 6; // dimanche

            dateFrom = dateFrom.AddDays(0 - dayOfWeekStartMonday);

            var dateTo = dateFrom.AddDays(PointagesBenevoleModel.CALENDAR_ROW_COUNT * PointagesBenevoleModel.CALENDAR_DAY_COUNT);

            var pointages = _context.Pointages
                .Where(p => p.BenevoleID == id)
                .Where(p => p.Date >= dateFrom && p.Date < dateTo)
                .OrderBy(p => p.Date)
                .ToDictionary(p => p.Date);

            var currentDate = dateFrom;

            var adresses = benevole.Adresses
                .Where(a => a.DateChangement < dateTo)
                .OrderBy(a => a.DateChangement)
                .ToList();

            int addressIndex = 0;

            DateTime? nextChangeAddressDate = null;

            if(addressIndex + 1 < adresses.Count)
                nextChangeAddressDate = adresses[addressIndex + 1].DateChangement;

            for (int r = 0; r < PointagesBenevoleModel.CALENDAR_ROW_COUNT; r++)
            {
                var row = new CalendarRow();

                for (int i = 0; i < PointagesBenevoleModel.CALENDAR_DAY_COUNT; i++)
                {
                    while (nextChangeAddressDate != null && currentDate.Date >= nextChangeAddressDate)
                    {
                        addressIndex++;

                        if (addressIndex + 1 < adresses.Count)
                            nextChangeAddressDate = adresses[addressIndex + 1].DateChangement;
                        else
                            nextChangeAddressDate = null;
                    }

                    var item = new CalendarItem
                    {
                        Date = currentDate,
                        Pointage = pointages.GetValueOrDefault(currentDate),
                        IsCurrentMonth = (currentDate.Month == month && currentDate.Year == year),
                        RowIndex = r,
                        ColumnIndex = i,
                        Distance = adresses[addressIndex].DistanceCentre,
                    };

                    if(centreGere != null && adresses[addressIndex].CentreID != centreGere.ID)
                        item.DisabledByCenter = true;

                    row.Items.Add(item);

                    currentDate = currentDate.AddDays(1);
                }

                model.CalendarRows.Add(row);
            }

            string viewName = "Benevole";

            if (partial)
                viewName = "_BenevoleCalendarContent";

            return View(viewName, model);
        }

        [HttpGet("Pointages/Benevole/{id}/editcreate")]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, DateTime date)
        {
            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound();

            var centre = benevole.GetAdresseFromDate(date).Centre;
            var userCentreId = GetCurrentUser().CentreID;

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(p => p.BenevoleID == id && p.Date == date);

            ViewBag.DisabledForCenter = (userCentreId != null && centre.ID != userCentreId);

            if (pointage == null)
            {
                pointage = new dal.models.Pointage
                {
                    BenevoleID = id,
                    CentreID = centre.ID,
                    Date = date,
                    NbDemiJournees = 1,
                };

                pointage.Benevole = benevole;
                pointage.Centre = centre;
            }

            return PartialView(pointage);
        }

        [HttpPost("Pointages/Benevole/{id}/editcreate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BenevoleEditOrCreate(int id, [FromBody] [Bind("BenevoleID,Date,NbDemiJournees")] Pointage pointage)
        {
            if (id != pointage.BenevoleID)
                return BadRequest("id does not match BenevoleId");

            var existing = _context.Pointages
                .Where(p => p.BenevoleID == pointage.BenevoleID && p.Date == pointage.Date.Date)
                .SingleOrDefault();

            if (existing != null)
                pointage.ID = existing.ID;

            var benevole = _context.Benevoles.Include(b => b.Adresses).SingleOrDefault(b => b.ID == pointage.BenevoleID);
            var centreId = benevole.GetAdresseFromDate(pointage.Date).CentreID;

            pointage.CentreID = centreId;

            if (existing != null && existing.CentreID != pointage.CentreID)
                return BadRequest("centre id does not match with existing pointage");

            var userCentreId = GetCurrentUser().CentreID;
            if (userCentreId != null && centreId != userCentreId)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    if (existing != null)
                    {
                        existing.NbDemiJournees = pointage.NbDemiJournees;
                        _context.Update(existing);
                    }
                    else
                        _context.Pointages.Add(pointage);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PointageExists(pointage.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Json(new { id = pointage.ID });
            }

            return View(pointage);
        }

        [HttpGet("Pointages/Benevole/{id}/printindex")]
        public async Task<IActionResult> PrintIndex(int id)
        {
            var benevole = await _context.Benevoles
                .Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .Include(b => b.Pointages)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound("Bénévole non trouvé");

            var model = new PrintIndexModel
            {
                Benevole = benevole,
            };

            var userCentreId = GetCurrentUser().CentreID;

            // début : début de période à partir du premier pointage du bénévole
            int startPeriodId;

            IQueryable<Pointage> ptgs = benevole.Pointages.AsQueryable();

            if (userCentreId != null)
                ptgs = ptgs.Where(p => p.CentreID == userCentreId);

            var dates = ptgs
                .OrderBy(p => p.Date)
                .Select(p => p.Date);

            var start = dates.FirstOrDefault(); 

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

            // fin : fin de période à partir du dernier pointage du bénévole
            var end = dates.LastOrDefault();

            if (end.Month >= 5)
                end = new DateTime(end.Year + 1, 1, 1);
            else
                end = new DateTime(end.Year, 5, 1);

            // calcul des périodes
            DateTime periodStart;
            DateTime periodEnd;

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

                    var adressesWithDates = benevole.GetAdressesInPeriod(periodStart, periodEnd);

                    var period = new PrintIndexPeriod
                    {
                        PeriodId = periodId,
                        Start = periodStart,
                        End = periodEnd,
                    };

                    foreach (var adrDate in adressesWithDates.Keys.OrderBy(k => k))
                    {
                        var adr = adressesWithDates[adrDate];

                        if(GetCurrentUser().CentreID == null || adr.CentreID == GetCurrentUser().CentreID)
                            period.Adresses.Add(adr);

                    }

                    if(period.Adresses.Count() > 0)
                        model.Periods.Add(period);
                }

                startPeriodId = 1;
            }

            return View(model);
        }

        [HttpGet("Pointages/Benevole/{id}/print")]
        public async Task<IActionResult> Print(int id, int addressId, int period, int year)
        {
            var benevole = await _context.Benevoles
                .Include(b => b.Adresses)
                .ThenInclude(a => a.Centre)
                .ThenInclude(c => c.Siege)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound("Bénévole non trouvé");

            var adresse = benevole.Adresses
                .SingleOrDefault(a => a.ID == addressId);

            if(adresse == null)
                return NotFound("Adresse non trouvée");

            if (!IsCentreAllowed(adresse.Centre))
                return Forbid();

            DateTime periodStart;
            DateTime periodEnd;

            switch(period)
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

            var frais = _context.Frais.SingleOrDefault(f => f.Annee == year);

            if(frais == null)
                return NotFound("Frais non trouvé");

            // ***** Recherches des dates des pointages à calculer
            var start = periodStart;
            var end = periodEnd;

            if(adresse.DateChangement > start)
                start = adresse.DateChangement;

            var adressesByPeriod = benevole.GetAdressesInPeriod(start, end)
                .OrderBy(x => x.Key);

            var allAdresses = adressesByPeriod.Select(x => x.Value).ToList();

            int index = allAdresses.IndexOf(adresse);

            if (index < adressesByPeriod.Count() - 1)
            {
                var nextAdr = allAdresses[index + 1];
                end = adressesByPeriod.Where(a => a.Value == nextAdr).Single().Key;
            }

            // ***** Calcul du nombre de demi-journées pointées sur l'adresse
            var totalDemiJournees = _context.Pointages
                .Where(p => p.BenevoleID == adresse.BenevoleID && p.CentreID == adresse.CentreID)
                .Where(p => p.Date >= start && p.Date < end)
                .Sum(p => p.NbDemiJournees);

            int monthCount;

            if(periodStart.Year == periodEnd.Year)
                monthCount = periodEnd.Month - periodStart.Month;
            else
                monthCount = periodEnd.Month + 12 - periodStart.Month;

            PrintModel model = new PrintModel
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd.AddDays(-1),
                Benevole = benevole,
                Adresse = adresse,
                FraisKm = frais.TauxKilometrique,
                MonthCount = monthCount,
                TotalDemiJournees = totalDemiJournees,
            };

            return View(model);
        }

        private bool PointageExists(int id)
        {
            return _context.Pointages.Any(e => e.ID == id);
        }

        // POST: Pointages/Delete/5 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pointage = await _context.Pointages.SingleOrDefaultAsync(m => m.ID == id);
            _context.Pointages.Remove(pointage);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}

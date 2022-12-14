using System;
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
        // Le warning MVC1004 ne devrait pas apparaitre (bug analyzer) : ne pas modifier (https://github.com/aspnet/AspNetCore/issues/6945)
        public async Task<IActionResult> BenevoleEditOrCreate(int id, DateTime date)
        {
            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound();

            var adresse = benevole.GetAdresseFromDate(date);
            var centre = adresse.Centre;
            var userCentreId = GetCurrentUser().CentreID;

            var pointage = await _context.Pointages
                .SingleOrDefaultAsync(p => p.BenevoleID == id && p.Date == date);

            ViewBag.DisabledForCenter = (userCentreId != null && centre.ID != userCentreId);

            if (pointage == null)
            {
                pointage = new dal.models.Pointage
                {
                    BenevoleID = id,
                    AdresseID = adresse.ID,
                    Date = date,
                    NbDemiJournees = 1,
                };

                pointage.Benevole = benevole;
                pointage.Adresse = adresse;
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
            var adresse = benevole.GetAdresseFromDate(pointage.Date);
            var centreId = adresse.CentreID;

            pointage.AdresseID = adresse.ID;

            if (existing != null && existing.AdresseID != pointage.AdresseID)
                return BadRequest("AdresseID does not match with existing pointage");

            var userCentreId = GetCurrentUser().CentreID;
            if (userCentreId != null && centreId != userCentreId)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    bool created = false;

                    if (existing != null)
                    {
                        existing.NbDemiJournees = pointage.NbDemiJournees;
                        _context.Update(existing);
                    }
                    else
                    {
                        created = true;
                        _context.Pointages.Add(pointage);
                    }

                    await _context.SaveChangesAsync();

                    if(created)
                        LogInfo("Pointage créé au {DatePointage:dd/MM/yyyy} pour le benevole #{BenevoleID} sur l'adresse {AdresseID}", pointage.Date, pointage.BenevoleID, pointage.AdresseID);
                    else
                        LogInfo("Pointage modifié au {DatePointage:dd/MM/yyyy} pour le benevole #{BenevoleID} sur l'adresse {AdresseID}", pointage.Date, pointage.BenevoleID, pointage.AdresseID);
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
                ptgs = ptgs.Where(p => p.Adresse.CentreID == userCentreId);

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

                    var adressesWithDates = benevole.GetAdressesInPeriod(periodStart, periodEnd, excludeEnd:true);

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

            DateTime periodStart, periodEnd;

            try
            {
                (periodStart, periodEnd) = GetPeriodDates(period, year);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var frais = _context.Frais.SingleOrDefault(f => f.Annee == year);

            if(frais == null)
                return NotFound("Frais non trouvé");

            // ***** Calcul du nombre de demi-journées pointées sur l'adresse
            var demiJournees = _context.Pointages
                .Where(p => p.AdresseID == addressId)
                .Where(p => p.Date >= periodStart && p.Date < periodEnd);

            var totalDemiJournees = demiJournees.Sum(p => p.NbDemiJournees);

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
                DetailDemiJournees = demiJournees.ToDictionary(p => Tuple.Create(p.Date.Month, p.Date.Day)),
            };

            return View(model);
        }

        [HttpGet("Pointages/Benevole/{id}/print-frais-km")]
        public async Task<IActionResult> PrintFraisKm(int id, int year)
        {
            var benevole = await _context.Benevoles
                .Include(b => b.Adresses)
                .ThenInclude(a => a.Centre)
                .ThenInclude(c => c.Siege)
                .SingleOrDefaultAsync(b => b.ID == id);

            if (benevole == null)
                return NotFound("Bénévole non trouvé");

            if (benevole.NbChevauxFiscauxVoiture == null)
                return BadRequest("Le nombre de chevaux fiscaux doit être renseigné pour le bénévole");

            DateTime periodStart = new DateTime(year, 1, 1);
            DateTime periodEnd = periodStart.AddYears(1);

            var adresses = benevole.GetAdressesInPeriod(periodStart, periodEnd, excludeEnd: true)
                .OrderBy(ap => ap.Key)
                .Select(ap => ap.Value)
                .Distinct();
            
            var centres = adresses.Select(a => a.Centre).Distinct();

            if (GetCurrentUser().Centre != null && !centres.Contains(GetCurrentUser().Centre))
                return Forbid();

            PrintFraisKmModel model = new PrintFraisKmModel
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd.AddDays(-1),
                Benevole = benevole,
                MonthCount = 12,
            };

            foreach(var adresse in adresses)
            {
                // ***** Calcul du nombre de demi-journées pointées sur l'adresse
                var demiJournees = _context.Pointages
                    .Where(p => p.Adresse == adresse)
                    .Where(p => p.Date >= periodStart && p.Date < periodEnd);

                var totalDemiJournees = demiJournees.Sum(p => p.NbDemiJournees);

                if(totalDemiJournees > 0)
                {
                    var addressData = new PrintFraisKmAddressModel
                    {
                        Adresse = adresse,
                        FirstDate = demiJournees.Select(p => p.Date).Min(),
                        LastDate = demiJournees.Select(p => p.Date).Max(),
                        TotalDemiJournees = totalDemiJournees,
                        Distance = totalDemiJournees * adresse.DistanceCentre,
                        DetailDemiJournees = demiJournees.ToDictionary(p => Tuple.Create(p.Date.Month, p.Date.Day)),
                    };

                    model.FraisParAdresse.Add(addressData);
                    model.DistanceTotale += addressData.Distance;
                }
            }

            // Barème impots revenus 2022
            var baremesAnnee = _context.BaremeFiscalLignes
                .Where(bf => bf.Annee == year)
                .AsEnumerable()
                .GroupBy(bf => bf.NbChevaux)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            if(baremesAnnee.Count() == 0)
                return NotFound("Barème de l'année demandée non trouvée");

            if(!baremesAnnee.TryGetValue(benevole.NbChevauxFiscauxVoiture.Value, out var baremesChevaux))
            {
                var minChevaux = baremesAnnee.Select(ba => ba.Key).Min();
                var maxChevaux = baremesAnnee.Select(ba => ba.Key).Max();
                
                if(benevole.NbChevauxFiscauxVoiture <= minChevaux)
                    baremesChevaux = baremesAnnee[minChevaux];
                else if(benevole.NbChevauxFiscauxVoiture >= maxChevaux)
                    baremesChevaux = baremesAnnee[maxChevaux];
            }

            decimal distanceAppliquee = model.DistanceTotale;
            BaremeFiscalLigne bareme = null;

            foreach(var baremeKm in baremesChevaux.OrderBy(bc => bc.LimiteKm))
            {
                if(model.DistanceTotale <= baremeKm.LimiteKm)
                {
                    bareme = baremeKm;
                    break;
                }
            }

            if(bareme == null)
            {
                // on applique la bareme limité au nombre de kilometre maximum
                bareme = baremesChevaux.Single(b => b.LimiteKm == baremesChevaux.Select(bc => bc.LimiteKm).Max());
                distanceAppliquee = bareme.LimiteKm;
            }

            model.FormuleBareme = $"({distanceAppliquee} * {bareme.Coef}) + {bareme.Ajout}";

            LogDebug($"Bareme appliqué : {bareme.NbChevaux} ch, {bareme.LimiteKm} km => {model.FormuleBareme}");

            model.FraisTotaux = distanceAppliquee * bareme.Coef + bareme.Ajout;

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

            LogInfo("Pointage supprimé au {DatePointage:dd/MM/yyyy} pour le benevole #{BenevoleID} sur l'adresse {AdresseID}", pointage.Date, pointage.BenevoleID, pointage.AdresseID);
            
            return Ok();
        }
    }
}

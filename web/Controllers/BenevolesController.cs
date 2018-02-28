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
using web.Filters;
using Microsoft.AspNetCore.Authorization;

namespace web.Controllers
{
    [AtLeastOneCenterExists]
    [Authorize]
    public class BenevolesController : RCBenevoleController
    {
        public BenevolesController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Benevoles
        public IActionResult Index()
        {
            var model = new BenevoleFilterModel
            {
                Term = string.Empty
            };

            if (User.IsInRole("SuperAdmin"))
                model.Centres = _context.Centres;
            else
                model.CentreID = GetCurrentUser().CentreID;

            return View(model);
        }

        // AJAX : Benevoles/Filter
        public IActionResult Filter(BenevoleFilterModel filter)
        {
            if (filter.Term == null)
                filter.Term = string.Empty;

            bool bCurrentCentreOnly = true;

            if (!User.IsInRole("SuperAdmin"))
            {
                filter.CentreID = GetCurrentUser().CentreID;
                bCurrentCentreOnly = false;
            }

            var query = _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre).AsQueryable();

            if (filter.CentreID > 0)
            {
                if(!bCurrentCentreOnly)
                    query = query.Where(b => b.Adresses.Any(a => a.CentreID == filter.CentreID));
                else
                    query = query.Where(b => b.Adresses.SingleOrDefault(a => a.IsCurrent).CentreID == filter.CentreID);
            }

            if (!string.IsNullOrEmpty(filter.Term))
                query = query.Where(b => b.Nom.ToLower().StartsWith(filter.Term.Trim().ToLower()));

            query = query.OrderBy(b => b.Adresses.SingleOrDefault(a => a.IsCurrent).Centre.Nom)
                .ThenBy(b => b.Nom)
                .ThenBy(b => b.Prenom);

            var model = query.Select(b => new BenevolePointageListItemModel
            {
                BenevoleID = b.ID,
                BenevoleNom = b.Nom,
                BenevolePrenom = b.Prenom,
                BenevoleCentre = b.Adresses.SingleOrDefault(a => a.IsCurrent).Centre.Nom,
                ShowAddressWarning = b.Adresses.Any(a => a.CentreID == filter.CentreID && a.IsCurrent)
            }).AsEnumerable();

            return View(model);
        }

        [HttpPost]
        public IActionResult List(string term)
        {
            var query = _context.ListAllowedBenevoles(GetCurrentUser());

            if (!string.IsNullOrEmpty(term))
                query = query.Where(b => b.Nom.ToLower().StartsWith(term));
            
            var list = query.Select(b => new
            {
                ID = b.ID,
                Nom = b.Nom,
                Prenom = b.Prenom,
            });

            return Json(list);
        }

        // GET: Benevoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (benevole == null)
                return NotFound();

            if (GetCurrentUser().CentreID != null && GetCurrentUser().CentreID != benevole.CurrentAdresse.CentreID)
                return Forbid();

            return View(benevole);
        }

        // GET: Benevoles/Create
        public IActionResult Create()
        {
            SetViewBagCentres();

            var benevole = new BenevoleWithAdresse
            {
                Benevole = new Benevole(),
                Adresse = new Adresse(),
            };

            return View(benevole);
        }

        // POST: Benevoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenevoleWithAdresse benevoleWithAddress)
        {
            if (!_context.ContainsCentre(benevoleWithAddress.Adresse.CentreID))
                ModelState.AddModelError("CentreID", "Le centre n'existe pas");

            SetViewBagCentres();

            if (!ModelState.IsValid)
                return View(benevoleWithAddress);

            benevoleWithAddress.Adresse.IsCurrent = true;
            benevoleWithAddress.Adresse.Benevole = benevoleWithAddress.Benevole;

            _context.Add(benevoleWithAddress.Adresse);
            _context.Add(benevoleWithAddress.Benevole);
            await _context.SaveChangesAsync();

            LogInfo("Benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom}) créé", benevoleWithAddress.Benevole.ID, benevoleWithAddress.Benevole.Prenom, benevoleWithAddress.Benevole.Nom);
            SetGlobalMessage("Le bénévole a été créé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        // GET: Benevoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var benevole = await _context.Benevoles
                .Include(b => b.Adresses)
                .ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (benevole == null)
                return NotFound();

            if(GetCurrentUser().CentreID != null && GetCurrentUser().CentreID != benevole.CurrentAdresse.CentreID)
                return Forbid();

            SetViewBagCentres();

            return View(benevole);
        }

        // POST: Benevoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Nom,Prenom,Telephone,CentreID")] Benevole benevole)
        {
            if (id != benevole.ID)
                return NotFound("Les identifiants ne correspondent pas");

            var existing = _context.Benevoles.Include(b => b.Adresses)
                .SingleOrDefault(b => b.ID == id);

            if(existing == null)
                return NotFound("Le bénévole n'existe pas");

            var adresse = existing.Adresses.SingleOrDefault(a => a.IsCurrent);

            if(adresse == null)
                return NotFound("Aucune adresse actuelle pour le bénévole");

            if (!_context.ContainsCentre(adresse.CentreID))
                ModelState.AddModelError("CentreID", "Le centre n'existe pas");

            var user = GetCurrentUser();

            if (user.Centre != null)
            {
                if (adresse.CentreID != user.Centre.ID)
                    ModelState.AddModelError("CentreID", "Vous ne pouvez pas créer de bénévole sur un autre centre que celui qui vous est affecté");
            }

            if (!ModelState.IsValid)
                return View(benevole);

            try
            {
                existing.Nom = benevole.Nom;
                existing.Prenom = benevole.Prenom;
                existing.Telephone = benevole.Telephone;
                
                _context.Update(existing);
                            
                await _context.SaveChangesAsync();

                LogInfo("Benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom}) modifié", benevole.ID, benevole.Prenom, benevole.Nom);
                SetGlobalMessage("Le bénévole a été modifié avec succès", EGlobalMessageType.Success);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BenevoleExists(benevole.ID))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Benevoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benevole = await _context.Benevoles
                .SingleOrDefaultAsync(m => m.ID == id);
            if (benevole == null)
            {
                return NotFound();
            }

            return View(benevole);
        }

        // POST: Benevoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benevole = await _context.Benevoles.SingleOrDefaultAsync(m => m.ID == id);
            _context.Benevoles.Remove(benevole);
            await _context.SaveChangesAsync();

            LogInfo("Benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom}) supprimé", benevole.ID, benevole.Prenom, benevole.Nom);
            SetGlobalMessage("Le bénévole a été supprimé avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Index));
        }

        // GET: Benevoles/ChangeAddress/5
        public async Task<IActionResult> ChangeAddress(int id, bool? force = false)
        {
            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (benevole == null)
                return NotFound();

            if (GetCurrentUser().CentreID != null && GetCurrentUser().CentreID != benevole.CurrentAdresse.CentreID)
                return Forbid();

            SetViewBagCentres();

            var benevoleWithAddress = new BenevoleWithAdresse
            {
                Benevole = benevole,
                Adresse = new Adresse
                {
                    BenevoleID = id,
                    CentreID = benevole.CurrentAdresse.CentreID,
                    Centre = benevole.CurrentAdresse.Centre,
                    DateChangement = DateTime.Today,
                }
            };

            ViewBag.Force = force;

            return View(benevoleWithAddress);
        }

        // POST: Benevoles/ChangeAddress/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAddress(int id, BenevoleWithAdresse benevoleWithAddress, bool force = false)
        {
            ViewBag.Force = force;
            if (!_context.ContainsCentre(benevoleWithAddress.Adresse.CentreID))
                ModelState.AddModelError("Adresse.CentreID", "Le centre n'existe pas");

            foreach (var ms in new List<string>(ModelState.Keys))
            {
                if(ms.StartsWith("Benevole."))
                    ModelState.Remove(ms);
            }

            SetViewBagCentres();

            benevoleWithAddress.Benevole = _context.Benevoles.SingleOrDefault(b => b.ID == benevoleWithAddress.Adresse.BenevoleID);

            if (!ModelState.IsValid)
                return View(benevoleWithAddress);

            var benevole = await _context.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre)
                .SingleOrDefaultAsync(b => b.ID == id);

            // Recherche d'adresses déjà présentes à une date ultérieure
            var anyAddress = benevole.Adresses.Any(a => a.DateChangement >= benevoleWithAddress.Adresse.DateChangement);

            if (anyAddress)
            {
                ModelState.AddModelError("Adresse.DateChangement", "Une adresse existe déjà à une date ultérieure. Veuillez supprimer l'adresse postérieure d'abord");
                return View(benevoleWithAddress);
            }

            // Recherche de pointages sur une adresse différente à une date ultérieure
            var pointagesFromDate = _context.Pointages
                .Where(p => p.BenevoleID == id)
                .Where(p => p.AdresseID != benevoleWithAddress.Adresse.ID)
                .Where(p => p.Date >= benevoleWithAddress.Adresse.DateChangement);

            if (pointagesFromDate.Count() > 0)
            {
                if (!force)
                {
                    ViewBag.Force = true;
                    ViewBag.ImpactedCount = pointagesFromDate.Count();
                    return View(benevoleWithAddress);
                }
                else
                {
                    // suppression des pointages précédents
                    _context.Pointages.RemoveRange(pointagesFromDate);

                }
            }

            benevole.CurrentAdresse.IsCurrent = false;

            benevoleWithAddress.Adresse.BenevoleID = benevole.ID;
            benevole.Adresses.Add(benevoleWithAddress.Adresse);

            benevole.Adresses.OrderByDescending(a => a.DateChangement).First().IsCurrent = true;

            _context.Add(benevoleWithAddress.Adresse);

            await _context.SaveChangesAsync();

            if (pointagesFromDate.Count() > 0)
                LogInfo("Suppression de {NombrePointages} pointages du benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom}) après le {DateChangement:dd/MM/yyyy}", pointagesFromDate.Count(), benevole.ID, benevole.Prenom, benevole.Nom, benevoleWithAddress.Adresse.DateChangement);

            LogInfo("Adresse {AdresseID} créée pour le benevole #{BenevoleID} ({BenevolePrenom} {BenevoleNom})", benevoleWithAddress.Adresse.ID, benevole.ID, benevole.Prenom, benevole.Nom);
            SetGlobalMessage("L'adresse a été créée avec succès", EGlobalMessageType.Success);

            return RedirectToAction(nameof(Details), new { id = id });
        }

        private bool BenevoleExists(int id)
        {
            return _context.Benevoles.Any(e => e.ID == id);
        }

        private void SetViewBagCentres()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                ViewBag.Centres = _context.Centres
                    .OrderBy(c => c.Nom)
                    .AsEnumerable();
            }
            else
            {
                ViewBag.Centres = _context.Centres
                    .Where(c => c.ID == GetCurrentUser().CentreID)
                    .AsEnumerable();
            }
        }
    }
}

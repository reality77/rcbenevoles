using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dal;
using Microsoft.AspNetCore.Authorization;
using web.Models;

namespace web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ParametresController : RCBenevoleController
    {
        public ParametresController(RCBenevoleContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(ListFraisWithNewYear());
        }

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult PostIndex()
        {
            var list = new List<string>(Request.Form.Keys);

            var formdata = Request.Form.Where(f => f.Key.StartsWith("frais_"));
            bool haserror = false;

            foreach (var form in formdata)
            {
                var yearstring = form.Key.Replace("frais_", "");

                if (!decimal.TryParse(form.Value, out decimal value) || value < 0)
                {
                    haserror = true;
                }
                else
                {
                    var year = Convert.ToInt32(yearstring);

                    var frais = _context.Frais.SingleOrDefault(f => f.Annee == year);

                    if (frais == null)
                    {
                        if (value > 0)
                        {
                            _context.Frais.Add(new dal.models.Frais
                            {
                                Annee = year,
                                TauxKilometrique = value,
                            });
                        }
                    }
                    else
                    {
                        if (value == 0)
                            haserror = true;
                        else
                            frais.TauxKilometrique = value;
                    }
                }
            }

            if (haserror)
                ViewBag.ErrorMessage = "Un des taux n'est pas correct. Veuillez le corriger pour sauvegarder l'ensemble des données";
            else
            {
                LogInfo("Taux kilométriques modifié");
                SetGlobalMessage("Les taux ont été sauvegardés avec succès", EGlobalMessageType.Success);
                _context.SaveChanges();
            }

            return View(ListFraisWithNewYear());
        }

        private IOrderedEnumerable<dal.models.Frais> ListFraisWithNewYear()
        {
            var list = new List<dal.models.Frais>(_context.Frais);

            var maxAnnee = _context.Frais.Max(f => f.Annee);

            list.Add(new dal.models.Frais { Annee = maxAnnee + 1 });

            return list.OrderBy(f => f.Annee);
        }
    }
}
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace dal.models
{
    public class Benevole
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le nom de famille du bénévole")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner le prénom du bénévole")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Veuillez renseigner un numéro de téléphone du bénévole")]
        [Display(Name = "N° de téléphone")]
        public string Telephone { get; set; }

        public List<Adresse> Adresses { get; set; }

        public List<Pointage> Pointages { get; set; }

        [NotMapped]
        public Adresse CurrentAdresse => Adresses?.SingleOrDefault(a => a.IsCurrent);

        public Adresse GetAdresseFromDate(DateTime date)
        {
            var addresses = this.Adresses
                .Where(a => a.DateChangement <= date)
                .OrderByDescending(a => a.DateChangement);

            return addresses.FirstOrDefault();
        }


        public IDictionary<DateTime, Adresse> GetAdressesInPeriod(DateTime periodStart, DateTime periodEnd)
        {
            var result = this.Adresses
                .Where(a => a.DateChangement < periodEnd)
                .ToDictionary(a => a.DateChangement);
            
            // on ajoute un element pour le debut de periode sauf si une adresse a été placée exactement sur la date de debut de periode
            result.TryAdd(periodStart, null);

            bool periodStartSet = false;
            Adresse currentAddress = null;

            foreach(var date in result.Keys.OrderBy(d => d))
            {
                var addr = result[date];

                if(addr == null)     // period start
                {
                    periodStartSet = true;
                    addr = currentAddress;
                    result[date] = currentAddress;
                }

                currentAddress = addr;

                if(!periodStartSet)
                    result.Remove(date);
            }

            return result;
        }
    }
}
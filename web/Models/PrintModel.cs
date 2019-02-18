using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintModel
    {
        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public int MonthCount { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public dal.models.Adresse Adresse { get; set; }

        public decimal FraisKm { get; set; }

        public decimal TotalDemiJournees { get; set; }

        public IDictionary<Tuple<int, int>, dal.models.Pointage> DetailDemiJournees { get; set; }

        public string FormatPhoneNumber()
        {
            const string REGEX_PHONE_10DIGITS = @"^(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})$";
            string phone = this.Benevole?.Telephone;

            if(!string.IsNullOrEmpty(phone))
            {
                var match = Regex.Match(phone, REGEX_PHONE_10DIGITS);

                if(match.Success)
                    return match.Result("$1 $2 $3 $4 $5");
            }

            return this.Benevole?.Telephone;
        }
    }
}

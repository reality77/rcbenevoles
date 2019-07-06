using System;
using System.Collections.Generic;

namespace web.Models
{
    public class PrintCentrePresenceModel
    {
        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public List<(dal.models.Benevole benevole, int nbar, int heures)> Presences { get; set; }

        public dal.models.Centre Centre { get; set; }

    }
}

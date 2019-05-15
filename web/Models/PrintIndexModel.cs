using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintIndexModel
    {
        public List<PrintIndexPeriod> Periods { get; set; }

        public dal.models.Benevole Benevole { get; set; }

        public PrintIndexModel()
        {
            this.Periods = new List<PrintIndexPeriod>();
        }
    }

    public class PrintIndexPeriod
    {
        public int PeriodId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public List<dal.models.Adresse> Adresses { get; set; }

        public PrintIndexPeriod()
        {
            this.Adresses = new List<dal.models.Adresse>();
        }
    }
}

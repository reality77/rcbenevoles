using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class PrintHoursPresenceIndexModel
    {
        public List<PrintIndexPeriod> Periods { get; set; }

        public dal.models.Centre Centre { get; set; }

        public PrintHoursPresenceIndexModel()
        {
            this.Periods = new List<PrintIndexPeriod>();
        }
    }
}

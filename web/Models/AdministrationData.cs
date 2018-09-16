using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using dal.models;

namespace web.Models
{
    public class AdministrationData
    {
        [Display(Name = "Nombre de bénévoles enregistrés")]
        public int BenevolesCount { get; set; }

        [Display(Name = "Nombre de pointages créés")]
        public int PointagesCount { get; set; }
        public Pointage LastCreatedPointage { get; set; }

        public IEnumerable<AdministrationFileData> BackupFiles { get; set; }
        public IEnumerable<AdministrationFileData> LogFiles { get; set; }
    }

    public class AdministrationFileData
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}

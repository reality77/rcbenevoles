using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using dal.models;
using System.Globalization;

namespace web.Models
{
    public class DiagData
    {
        [Display(Name = "Culture actuelle")]
        public CultureInfo Culture { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace dal.models
{
    public class BaremeFiscalLigne
    {
        [Display(Name = "Année")]
        [Required]
        public int Annee { get; set; }

        [Display(Name = "Nombre de chevaux")]
        [Required]
        public int NbChevaux { get; set; }
        
        [Display(Name = "Limite en km")]
        [Required]
        public int LimiteKm { get; set; }
        
        [Display(Name = "Coefficient multiplicateur")]
        [Required]
        public decimal Coef { get; set; }
        
        [Display(Name = "Ajout de valeur")]
        [Required]
        public decimal Ajout { get; set; }
    }
}

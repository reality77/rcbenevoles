using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace web.Models
{
    public class LoginPasswordModel
    {
        [Required]
        [Display(Name ="Identifiant")]
        public string Login { get; set; }

        [Required]
        [Display(Name ="Mot de passe")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}

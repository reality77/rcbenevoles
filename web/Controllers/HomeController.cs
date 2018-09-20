using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using dal;

namespace web.Controllers
{
    public class HomeController : RCBenevoleController
    {
        public HomeController(RCBenevoleContext context)
        {
            _context = context;
        }

        public IActionResult Index(string ReturnUrl = null)
        {
            const string HOME_MESSAGE_FILE = "external/home_message";
            if(System.IO.File.Exists(HOME_MESSAGE_FILE))
                ViewData["InformationMessage"] = System.IO.File.ReadAllText(HOME_MESSAGE_FILE);

            if(!string.IsNullOrEmpty(ReturnUrl))
                ViewData["ReturnUrl"] = ReturnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginPasswordModel model)
        {
            LogInfo("[LOGIN-TRY:{UserLogin}] Tentative de connexion de {UserLogin}", model.Login);

            if (!ModelState.IsValid)
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : ModelState invalide ({@ModelState})", model.Login, ModelState);
                return View();
            }

            var dbuser = _context.Utilisateurs.Include(u => u.Centre).Where(u => u.Login == model.Login).SingleOrDefault();

            if (dbuser == null)
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : Utilisateur inconnu", model.Login);
                ModelState.AddModelError("", "Echec de la connexion. Vérifier votre login et votre mot de passe");
                return View();
            }

            if(!dbuser.TestPassword(model.Password))
            {
                LogWarning("[LOGIN-FAIL:{UserLogin}] Echec de connexion de {UserLogin} : Mot de passe invalide", model.Login);
                ModelState.AddModelError("", "Echec de la connexion. Vérifier votre login et votre mot de passe");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Login),
                new Claim(ClaimTypes.Authentication, "true"),
            };

            if (dbuser.Centre != null)
                claims.Add(new Claim(ClaimTypes.Role, "BasicAdmin", ClaimValueTypes.String));
            else
                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin", ClaimValueTypes.String));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            LogInfo("[LOGIN-SUCCESS:{UserLogin}] Succès de la connexion de {UserLogin}", model.Login);

            string returnUrl = null;

            if(!string.IsNullOrEmpty(model.ReturnUrl))
            {
                if(Uri.TryCreate(model.ReturnUrl, UriKind.Relative, out Uri uri) && !uri.IsAbsoluteUri)
                    returnUrl = model.ReturnUrl;
            }

            if(!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(Index));
        }

        public IActionResult Legal()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

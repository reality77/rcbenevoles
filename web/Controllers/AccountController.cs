using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dal;
using Microsoft.AspNetCore.Authorization;
using web.Models;
using Microsoft.AspNetCore.Authentication;

namespace web.Controllers
{
    [Authorize]
    public class AccountController : RCBenevoleController
    {
        public AccountController(RCBenevoleContext context)
        {
            _context = context;
        }

        // GET: Account
        public IActionResult Index()
        {
            var model = GetCurrentUser();
            return View(model);
        }

        // GET: Account/ChangePassword
        public ActionResult ChangePassword()
        {
            var model = new ChangeMyPasswordModel { Login = GetCurrentUser().Login };
                
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangeMyPasswordModel model)
        {
            var user = GetCurrentUser();

            if (user.Login != model.Login)
                return Forbid();

            model.Login = user.Login;

            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword.Trim().Length == 0)
                return BadRequest();

            try
            {
                if (model.NewPassword != model.NewPasswordConfirm)
                {
                    ModelState.AddModelError("NewPasswordConfirm", "Les mots de passe ne correspondent pas");
                    return View(model);
                }

                if (!user.TestPassword(model.OldPassword))
                {
                    ModelState.AddModelError("OldPassword", "Le mot de passe est erroné");
                    return View(model);
                }

                user.SetPassword(model.NewPassword);

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult AccessDenied(string ReturnUrl)
        {
            if(Uri.TryCreate(ReturnUrl, UriKind.Relative, out Uri uri) && !uri.IsAbsoluteUri)
            {
                ViewBag.ReturnUrl = ReturnUrl;
            }

            return View();
        }
    }
}
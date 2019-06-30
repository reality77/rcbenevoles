using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dal;
using Microsoft.AspNetCore.Authorization;
using web.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class DiagnosticsController : RCBenevoleController
    {
        public DiagnosticsController(RCBenevoleContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DiagData
            {
                Culture = CultureInfo.CurrentCulture
            };

            return View(model);
        }
    }
}
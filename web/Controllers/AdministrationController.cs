using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dal;
using Microsoft.AspNetCore.Authorization;
using web.Models;
using System.IO;

namespace web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdministrationController : RCBenevoleController
    {
        public AdministrationController(RCBenevoleContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Log files
            var fileLogPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH"));
            var logFiles = Directory.EnumerateFiles(fileLogPath).Select(f => new AdministrationFileData {
                Name = Path.GetFileName(f),
            });

            // DB backup files
            var backupLogPath = Environment.GetEnvironmentVariable("APP_DB_BACKUP_PATH");
            var backupFiles = Directory.EnumerateFiles(backupLogPath).Select(f => new AdministrationFileData {
                Name = Path.GetFileName(f),
            });

            // Model
            var model = new AdministrationData
            {
                BenevolesCount = _context.Benevoles.Count(),
                PointagesCount = _context.Pointages.Count(),
                LastCreatedPointage = _context.Pointages.OrderByDescending(p => p.ID).FirstOrDefault(),
                LogFiles = logFiles,
                BackupFiles = backupFiles,
            };

            return View(model);
        }

        public IActionResult DownloadLogFile(string name)
        {
            //TODO
            return null;
        }

        public IActionResult DownloadBackupFile(string name)
        {
            //TODO
            return null;
        }
    }
}
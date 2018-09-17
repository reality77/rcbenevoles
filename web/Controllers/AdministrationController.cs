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
            var model = new AdministrationData
            {
                BenevolesCount = _context.Benevoles.Count(),
                PointagesCount = _context.Pointages.Count(),
                LastCreatedPointage = _context.Pointages.OrderByDescending(p => p.ID).FirstOrDefault(),
            };

            // Log files
            var pathLogs = Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH");

            if(!string.IsNullOrWhiteSpace(pathLogs))
            {
                var dirLogs = new DirectoryInfo(Path.GetDirectoryName(pathLogs));

                var logFiles = dirLogs.GetFiles()
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new AdministrationFileData {
                        Name = f.Name,
                        Date = f.LastWriteTime,
                    });

                model.LogFiles = logFiles;
            }
            else
            {
                model.LogFilesError = "Aucun chemin de logs configuré";
            }

            // DB backup files
            var pathBackups = Environment.GetEnvironmentVariable("APP_DB_BACKUP_PATH");

            if(!string.IsNullOrWhiteSpace(pathBackups))
            {
                var dirBackups = new DirectoryInfo(pathBackups);

                var backupFiles = dirBackups.GetFiles()
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new AdministrationFileData {
                        Name = f.Name,
                        Date = f.LastWriteTime,
                    });

                model.BackupFiles = backupFiles;
            }
            else
            {
                model.BackupFilesError = "Aucun chemin de backup configuré";
            }

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
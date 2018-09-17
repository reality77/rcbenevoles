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

            if (!string.IsNullOrWhiteSpace(pathLogs))
            {
                var dirLogs = new DirectoryInfo(Path.GetDirectoryName(pathLogs));

                if (dirLogs.Exists)
                {
                    var logFiles = dirLogs.GetFiles()
                        .OrderByDescending(f => f.LastWriteTime)
                        .Select(f => new AdministrationFileData
                        {
                            Name = f.Name,
                            Date = f.LastWriteTime,
                        });

                    model.LogFiles = logFiles;
                }
                else
                    model.LogFilesError = "Chemin des logs non trouvé";
            }
            else
            {
                model.LogFilesError = "Aucun chemin de logs configuré";
            }

            // DB backup files
            var pathBackups = Environment.GetEnvironmentVariable("APP_DB_BACKUP_PATH");

            if (!string.IsNullOrWhiteSpace(pathBackups))
            {
                var dirBackups = new DirectoryInfo(pathBackups);

                if (dirBackups.Exists)
                {
                    var backupFiles = dirBackups.GetFiles()
                        .OrderByDescending(f => f.LastWriteTime)
                        .Select(f => new AdministrationFileData
                        {
                            Name = f.Name,
                            Date = f.LastWriteTime,
                        });

                    model.BackupFiles = backupFiles;
                }
                else
                    model.BackupFilesError = "Chemin des backups non trouvé";
            }
            else
            {
                model.BackupFilesError = "Aucun chemin de backup configuré";
            }

            return View(model);
        }

        public IActionResult DownloadLogFile(string name)
        {
            var basePath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH"));

            return DownloadInternal(basePath, name);
        }

        public IActionResult DownloadBackupFile(string name)
        {
            var basePath = Environment.GetEnvironmentVariable("APP_DB_BACKUP_PATH");

            return DownloadInternal(basePath, name);
        }

        private IActionResult DownloadInternal(string basePath, string name)
        {
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                var path = Path.Combine(basePath, name);
                if (!System.IO.File.Exists(path))
                    return NotFound();

                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var mimeType = "application/octet-stream";

                switch (Path.GetExtension(name).ToLower())
                {
                    case "sql":
                    case "txt":
                    case "log":
                        mimeType = "text/plain";
                        break;
                    case "zip":
                        mimeType = "application/zip";
                        break;
                    default:
                        mimeType = "application/octet-stream";
                        break;
                }

                return new FileStreamResult(fileStream, mimeType)
                {
                    FileDownloadName = name,
                };
            }
            else
            {
                return NotFound();
            }
        }
    }
}
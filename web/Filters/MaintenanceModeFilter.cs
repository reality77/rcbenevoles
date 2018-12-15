using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using web.Utils;

namespace web.Filters
{
    public class MaintenanceModeFilter : IResultFilter
    {
        const string MAINTENANCE_ON_FILE = "external/maintenance_on";

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (System.IO.File.Exists(MAINTENANCE_ON_FILE))
            {
                context.Result = new ViewResult()
                {
                    StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                    ViewName = "Maintenance",
                };
            }
        }
    }
}
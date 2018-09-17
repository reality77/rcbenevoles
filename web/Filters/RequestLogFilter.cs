using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using web.Utils;

namespace web.Filters
{
    public class RequestLogFilter : IResultFilter
    {
        Stopwatch _timer;

        public void OnResultExecuting(ResultExecutingContext context)
        {
            _timer = new Stopwatch();
            _timer.Start();

            var controller = context.Controller as Controller;
            LogExtensionUtils.LogInfo(controller, "{RequestDirection} {RequestUriPath} {RequestUriQueryData}", ">>", controller.Request.Path.Value, controller.Request.Query);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            _timer.Stop();

            var controller = context.Controller as Controller;
            LogExtensionUtils.LogInfo(controller, "{RequestDirection} {RequestUriPath} {RequestUriQueryData} returned {ResponseStatus} ({ResponseElapsedTime} ms)", "<<", controller.Request.Path.Value, controller.Request.Query, context.HttpContext.Response.StatusCode, _timer.ElapsedMilliseconds);
        }
    }
}
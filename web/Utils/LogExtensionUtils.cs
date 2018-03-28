using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using web.Filters;

namespace web.Utils
{
    public static class LogExtensionUtils
    {
        private const string LOG_PREFIX = "({X_UserLogin}@{X_ClientIp}) [{X_Method} {X_ControllerName}.{X_ActionName}] ";

        public static void LogDebug(Controller controller, string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Debug(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(controller, propertyValues));
        }

        public static void LogInfo(Controller controller, string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Information(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(controller, propertyValues));
        }

        public static void LogWarning(Controller controller, string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Warning(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(controller, propertyValues));
        }

        public static void LogError(Controller controller, string templateMessage, params object[] propertyValues)
        {
            Serilog.Log.Error(LOG_PREFIX + templateMessage, GenerateFullPropertyValues(controller, propertyValues));
        }

        private static object[] GenerateFullPropertyValues(Controller controller, params object[] propertyValues)
        {
            const int NB_PROPERTIES_ADDED = 5;

            var realProps = new object[propertyValues.Length + NB_PROPERTIES_ADDED];

            int propindex = 0;

            // 1 - Utilisateur
            if(!controller.User.Identity.IsAuthenticated)
                realProps[propindex++] = "<anonymous>";
            else
                realProps[propindex++] = controller.User.Identity.Name;

            realProps[propindex++] = GetClientIpAddress(controller.Request.HttpContext);

            // 2 - Method/Controlleur/Action
            realProps[propindex++] = controller.Request.Method;

            var actionDesc = controller.ControllerContext?.ActionDescriptor;
            if(actionDesc == null)
            {
                realProps[propindex++] = "";
                realProps[propindex++] = "";
            }
            else
            {
                realProps[propindex++] = actionDesc.ControllerName;
                realProps[propindex++] = actionDesc.ActionName;
            }

            propertyValues.CopyTo(realProps, NB_PROPERTIES_ADDED);

            return realProps;
        }

        public static string GetClientIpAddress(HttpContext context)
        {
            if (context.Request?.Headers.TryGetValue("X-Real-IP", out Microsoft.Extensions.Primitives.StringValues ips) == true)
                return ips[0];

            return context.Connection.RemoteIpAddress.ToString();
        }
    }
}
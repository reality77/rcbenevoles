using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using web.Filters;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Primitives;
using System.Linq;

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
			string ip = null;

            // FROM https://stackoverflow.com/questions/28664686/how-do-i-get-client-ip-address-in-asp-net-core

			// todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

			// X-Forwarded-For (csv list):  Using the First entry in the list seems to work
			// for 99% of cases however it has been suggested that a better (although tedious)
			// approach might be to read each IP from right to left and use the first public IP.
			// http://stackoverflow.com/a/43554000/538763
			ip = GetHeaderValueAs<string>(context, "X-Forwarded-For").SplitCsv().FirstOrDefault();

			// RemoteIpAddress is always null in DNX RC1 Update1 (bug).
			if (string.IsNullOrWhiteSpace(ip) && context.Connection?.RemoteIpAddress != null)
				ip = context.Connection.RemoteIpAddress.ToString();

			if (string.IsNullOrWhiteSpace(ip))
				ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");

			// _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

			if (string.IsNullOrWhiteSpace(ip))
				throw new Exception("Unable to determine caller's IP.");

			return ip;
		}

		private static T GetHeaderValueAs<T>(HttpContext context, string headerName)
		{
			StringValues values;

			if (context.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
			{
				string rawValues = values.ToString();   // writes out as Csv when there are multiple.

				if (!string.IsNullOrEmpty(rawValues))
				    return (T)Convert.ChangeType(values.ToString(), typeof(T));
			}
			return default(T);
		}

		public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
		{
			if (string.IsNullOrWhiteSpace(csvList))
				return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

			return csvList
				.TrimEnd(',')
				.Split(',')
				.AsEnumerable<string>()
				.Select(s => s.Trim())
				.ToList();
		}
    }
}

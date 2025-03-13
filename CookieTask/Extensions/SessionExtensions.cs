using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace CompanyTask.Extensions
{
    public static class CookieExtensions
    {
        public static void SetObjectAsJson(this IResponseCookies cookies, string key, object value, int days = 1)
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(days) };
            cookies.Append(key, JsonConvert.SerializeObject(value), options);
        }

        public static T? GetObjectFromJson<T>(this IRequestCookieCollection cookies, string key)
        {
            if (cookies.TryGetValue(key, out var value))
                return JsonConvert.DeserializeObject<T>(value);
            return default;
        }
    }
}

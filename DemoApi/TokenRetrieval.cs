using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DemoApi
{
    public static class TokenRetrieval
    {
        public static Func<HttpRequest, string> FromAuthHeaderWithFallback()
        {
            return (request) =>
            {
                string authorization = request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authorization))
                {
                    return request.Query["access_token"].FirstOrDefault();
                }

                if (authorization.StartsWith("Bearer" + " ", StringComparison.OrdinalIgnoreCase))
                {
                    return authorization.Substring(7).Trim();
                }

                return null;
            };
        }
    }
}
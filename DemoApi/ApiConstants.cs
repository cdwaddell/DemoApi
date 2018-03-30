using System.Collections.Generic;

namespace DemoApi
{
    public static class ApiConstants
    {
        public static readonly string[] Versions = {V10, V11Beta};
        public static readonly Dictionary<string, string> Scopes = new Dictionary<string, string>
        {
            { AdminScope, "Grants the ability to create content." },
            { PublicScope, "Grants the ability to view and comment on content." }
        };

        public const string V11Beta = "1.1b";
        public const string V10 = "1.0";

        public const string AdminScope = "Blog.Admin";
        public const string PublicScope = "Blog.Public";
    }
}
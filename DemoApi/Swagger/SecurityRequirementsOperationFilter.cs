using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DemoApi.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        private readonly string _appName;
        private readonly IAuthorizationPolicyProvider _provider;

        private static readonly object ValidationErrorObject = new
        {
            Property1 = new[] {"First error for Property1", "Second error for Property1"},
            Property2 = new[] {"First error for Property1"}
        };
        
        public SecurityRequirementsOperationFilter(IConfiguration configure, IAuthorizationPolicyProvider provider)
        {
            _appName = configure["ApiName"];
            _provider = provider;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            //everything requires the public scope
            var requiredRootPolicies = new[] { _appName, ApiConstants.PublicScope };

            var apiDescription = context.ApiDescription;

            var requiredScopes = apiDescription
                .ActionAttributes()
                .Union(apiDescription.ControllerAttributes())
                .OfType<AuthorizeAttribute>()//Get all auth attributes from the action and controller
                .Where(a => a.Policy != null)//There has to be a policy set
                .Select(a => _provider.GetPolicyAsync(a.Policy).Result)//Lookup the policy
                .SelectMany(p => 
                    p.Requirements //flatten the requirements
                )
                .Union(
                    //combine that with the global requirements
                    apiDescription.ActionDescriptor.FilterDescriptors
                        .Select(f => f.Filter)
                        .OfType<AuthorizeFilter>()
                        .Where(f => f.Policy?.Requirements != null)
                        .SelectMany(f => 
                            f.Policy?.Requirements
                        )
                ).OfType<ClaimsAuthorizationRequirement>() // we only care about scope claims
                .Where(c => c.ClaimType == "Scope")
                .SelectMany(c => c.AllowedValues)
                .Union(requiredRootPolicies)
                .Distinct()
                .ToList();
            
            operation.Responses.Add("400", new Response { Description = "Bad Request", Examples = ValidationErrorObject });

            if (!requiredScopes.Any())
                return;

            operation.Responses.Add("401", new Response { Description = "Unauthorized" });
            operation.Responses.Add("403", new Response { Description = "Forbidden" });

            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    {"oidc", requiredScopes}
                }
            };
        }
    }
}
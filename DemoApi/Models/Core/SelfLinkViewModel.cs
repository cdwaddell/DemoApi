using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace DemoApi.Models.Core
{
    public abstract class SelfLinkViewModel
    {
        public static IActionContextAccessor ActionContextAccessor;

        protected abstract string ActionName { get; }
        protected abstract string ControllerName { get; }
        
        public virtual object RouteValues { private get; set; }

        public string SelfLink => new UrlHelper(ActionContextAccessor.ActionContext).Action(ActionName, ControllerName, RouteValues, ActionContextAccessor.ActionContext.HttpContext.Request.Scheme);
    }
}
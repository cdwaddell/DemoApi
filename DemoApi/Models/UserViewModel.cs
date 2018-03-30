using DemoApi.Controllers;
using DemoApi.Models.Core;

namespace DemoApi.Models
{
    public class UserViewModel: SelfLinkViewModel
    {
        public string Name { get; set; }

        protected override string ActionName => nameof(UserController.GetUser);
        protected override string ControllerName => "User";
    }
}
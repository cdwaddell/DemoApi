namespace DemoApi.Controllers
{
    public class UserViewModel: SelfLinkViewModel
    {
        public string Name { get; set; }

        protected override string ActionName => nameof(UserController.GetUser);
        protected override string ControllerName => "User";
    }
}
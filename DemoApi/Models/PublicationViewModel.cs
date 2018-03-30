namespace DemoApi.Controllers
{
    public class PublicationViewModel : SelfLinkViewModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public UserViewModel Author { get; set; }
        public CommentSummaryViewModel Comments {get; set; }

        protected override string ActionName => nameof(BlogController.GetPublication);
        protected override string ControllerName => "Blog";
    }
}
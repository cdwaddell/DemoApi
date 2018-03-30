namespace DemoApi.Controllers
{
    public class PostsSummaryViewModel : SelfLinkViewModel
    {
        public int Count { get; set; }
        protected override string ActionName => nameof(BlogController.GetPosts);
        protected override string ControllerName => "Blog";
    }
}
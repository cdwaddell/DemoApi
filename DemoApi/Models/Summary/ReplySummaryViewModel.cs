namespace DemoApi.Controllers
{
    public class ReplySummaryViewModel : SelfLinkViewModel
    {
        public int Count { get; set; }
        protected override string ActionName => nameof(BlogController.GetPublicationCommentReplies);
        protected override string ControllerName => "Blog";
    }
}
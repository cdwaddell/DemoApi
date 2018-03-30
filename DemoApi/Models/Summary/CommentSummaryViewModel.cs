namespace DemoApi.Controllers
{
    public class CommentSummaryViewModel : SelfLinkViewModel
    {
        public int Count { get; set; }
        protected override string ActionName { get; } = nameof(BlogController.GetPublicationComments);
        protected override string ControllerName { get; } = "Blog";
    }
}
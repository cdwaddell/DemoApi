namespace DemoApi.Controllers
{
    public class CommentViewModel : SelfLinkViewModel
    {
        public int? ReplyToCommentId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public UserViewModel Sender { get; set; }
        public ReplySummaryViewModel Replies { get; set; }

        protected override string ActionName => nameof(BlogController.GetPublicationComment);
        protected override string ControllerName => "Blog";
    }
}
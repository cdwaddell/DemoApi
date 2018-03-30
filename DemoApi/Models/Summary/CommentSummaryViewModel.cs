using DemoApi.Controllers;
using DemoApi.Models.Core;

namespace DemoApi.Models.Summary
{
    public class CommentSummaryViewModel : SelfLinkViewModel
    {
        public int Count { get; set; }
        protected override string ActionName { get; } = nameof(BlogController.GetPublicationComments);
        protected override string ControllerName { get; } = "Blog";
    }
}
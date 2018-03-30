using DemoApi.Controllers;
using DemoApi.Models.Core;
using DemoApi.Models.Summary;

namespace DemoApi.Models
{
    public class BlogViewModel: SelfLinkViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public UserViewModel Owner { get; set; }
        public PostsSummaryViewModel Posts { get; set; }
        public PagesSummaryViewModel Pages { get; set; }

        protected override string ActionName => nameof(BlogController.GetBlog);
        protected override string ControllerName => "Blog";
    }
}
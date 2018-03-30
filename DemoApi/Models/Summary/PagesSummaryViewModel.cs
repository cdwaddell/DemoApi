namespace DemoApi.Controllers
{
    public class PagesSummaryViewModel : SelfLinkViewModel
    {
        public int Count { get;set; }
        protected override string ActionName => nameof(BlogController.GetPages);
        protected override string ControllerName => "Blog";
    }
}
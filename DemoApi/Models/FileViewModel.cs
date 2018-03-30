namespace DemoApi.Controllers
{
    public class FileViewModel: SelfLinkViewModel
    {
        public FileContentViewModel Content { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        protected override string ActionName { get; } = nameof(FileController.GetFile);
        protected override string ControllerName { get; } = "File";
        public UserViewModel Owner { get; set; }
    }
}
namespace DemoApi.Controllers
{
    public class FileContentViewModel: SelfLinkViewModel
    {
        protected override string ActionName { get; } = nameof(FileController.GetFileContent);
        protected override string ControllerName { get; } = "File";
    }
}
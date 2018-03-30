using DemoApi.Controllers;
using DemoApi.Models.Core;

namespace DemoApi.Models
{
    public class FileContentViewModel: SelfLinkViewModel
    {
        protected override string ActionName { get; } = nameof(FileController.GetFileContent);
        protected override string ControllerName { get; } = "File";
    }
}
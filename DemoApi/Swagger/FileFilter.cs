using System.IO;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DemoApi.Swagger
{
    public class FileFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.SupportedResponseTypes.Any(x => x.Type == typeof(Stream))
                && operation.Responses.Any(x => x.Value.Schema?.Type == "file"))
            {
                operation.Produces = new[] { "application/octet-stream" };
            }
        }
    }
}
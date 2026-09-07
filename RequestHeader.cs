using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace laptop_service
{
    public class RequestHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "OID",
                In = ParameterLocation.Header,
                Required = true
            });

            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = "EID",
                In = ParameterLocation.Header,
                Required = true
            });
        }
    }
}

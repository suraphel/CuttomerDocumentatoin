using Swashbuckle.Application;
using System.Web.Http;

namespace Local24API
{
    public static class SwaggerExtensions
    {
        public static HttpConfiguration EnableSwagger(this HttpConfiguration httpConfiguration)
        {
            httpConfiguration
                .EnableSwagger(c => c.SingleApiVersion("v1", "24LOCAL API"))
                .EnableSwaggerUi();
            return httpConfiguration;
        }
    }
}
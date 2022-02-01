using MySql.Data.MySqlClient;
using Local24API.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;


namespace Local24API.Controllers
{
    [RoutePrefix("api/CustomerPortal")]
    public class CustomerPortalController : ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;

        //[Authorize]
        [HttpGet]
        [SwaggerOperation("CustomerDocuments")]
        [Route("CustomerDocuments")]
        public async Task<HttpResponseMessage> CustomerDocuments()
        {
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }            

    }

}
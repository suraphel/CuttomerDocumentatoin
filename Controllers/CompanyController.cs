using Microsoft.AspNet.Identity;
using MySql.Data.MySqlClient;
using Local24API.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Local24API.Controllers
{
    [RoutePrefix("api/Company")]
    public class CompanyController : ApiController
    {
        private string LOCAL24ConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Connection"].ConnectionString;
           
        public CompanyController()
        {
           
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("Companies")]
        [Route("Companies")]
        public async Task<CompanyModel> Companies(int companyID)
        {
            var principal = ClaimsPrincipal.Current;
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub");
            CompanyModel company = new CompanyModel();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select c.cityID, c.cityTitle, c.type, p.paramData as companyInfo, p2.paramData as companyRates " +
                        "from rokea_booking.sh_cities c " +
                        "join rokea_intranet.jf_fixel_params p on c.cityid = p.cityid " +
                        "join rokea_intranet.jf_fixel_params p2 on c.cityid = p2.cityid " +
                        "where c.cityid = " +@companyID +" and(p.paramtype = 'company' AND p2.paramtype = 'prices')", connection))
                    {
                        cmd.Parameters.Add(new MySqlParameter("companyID", companyID));

                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            company.companyID= reader.GetInt32(0);
                            company.companyTitle = reader.GetString(1);
                            company.companyType = reader.GetInt32(2);
                            company.companyInfo = JsonConvert.DeserializeObject<CompanyInfo>(reader.GetString(3));
                            company.companyRates = JsonConvert.DeserializeObject<CompanyRates>(reader.GetString(4));
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return company;
        }

      
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_repo.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using MySql.Data.MySqlClient;
using Local24API.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Local24API.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private string LOCAL24ConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Connection"].ConnectionString;
        
        //private AuthRepository _repo = null;
        //private UserManager<IdentityUser> _userManager;
        //private AuthContext _ctx;

        public UserController()
        {
            //_repo = new AuthRepository();
            //_ctx = new AuthContext();
            //_userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }

        [AllowAnonymous]
        [HttpPost]
        [SwaggerOperation("Register")]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //IdentityResult result = await _repo.RegisterUser(userModel);

            //IHttpActionResult errorResult = GetErrorResult(result);

            //if (errorResult != null)
            //{
            //    return errorResult;
            //}

            //IdentityUser user = _userManager.FindByName(userModel.UserName);

            ////send confirmation email
            //MailMessage mailMsg = new MailMessage();
            //mailMsg.To.Add(new MailAddress(userModel.UserName));
            //mailMsg.From = new MailAddress("noreply@folkestyret.no", "Folkestyret");
            //mailMsg.Subject = "Velkommen som representant i Folkestyret!";
            //string text = "Klikk lenken for å bekrefte din epostadresse:";
            ////string html = @"<p>Velkommen som representant i Folkestyret</p><p>Klikk på lenken under for å bekfrefte din epostadresse.</p> <a href='http://localhost:3000/auth/ConfirmEmail?id=" + user.Id + "'>Bekreft adressenved å klikke på denne lenken</a>";
            //string html = @"<p>Velkommen som representant i Folkestyret</p><p>Klikk på lenken under for å bekfrefte din epostadresse.</p> <a href='https://www.folkestyret.no/auth/ConfirmEmail?id=" + user.Id +"'>Bekreft adressen ved å klikke på denne lenken</a>";
            //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain)); 
            //mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            //// Init SmtpClient and send
            //SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
            //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("azure_5a61c33468c4c7d8b6345d84b270ef3f@azure.com", "Barchetta313");
            //smtpClient.Credentials = credentials;

            //smtpClient.Send(mailMsg);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [SwaggerOperation("ValidateEmail")]
        [Route("ValidateEmail")]
        public async Task<IHttpActionResult> ValidateEmail(string email)
        {
            //IdentityResult result = await _repo.ValidateEmail(email);

            //IHttpActionResult errorResult = GetErrorResult(result);

            //if (errorResult != null)
            //{
            //    return errorResult;
            //}

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("ConfirmEmail")]
        [Route("ConfirmEmail")]
        public async Task<IHttpActionResult> ConfirmEmail(string id)
        {
            string connString = "Server=tcp:urtehagensql.database.windows.net,1433;Initial Catalog=DemokratiDB;Persist Security Info=False;User ID=mattis;Password=Barchetta313;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";
            
            //string insertCommandText = "UPDATE AspNetUsers set EmailConfirmed = 1 where Id = '"+id +"'";
            //try
            //{
            //    using (var connection = new SqlConnection(connString))
            //    {
            //        connection.Open();
            //        SqlCommand cmd = new SqlCommand(insertCommandText);
            //        cmd.Connection = connection;
            //        cmd.ExecuteNonQuery();
            //    }
            //}
            //catch
            //{
            //    throw;
            //}

            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("Profile")]
        [SwaggerOperation("Profile")]
        public UserProfileModel Profile()
        {
            var principal = ClaimsPrincipal.Current;
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub");
            UserProfileModel accountProfile = new UserProfileModel();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select e.employeeID, e.employeeName, e.cityID" +
                        "from rokea_booking.sh_employee e "+
                        "where employeeName = '" + idClaim.Value +"'", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            accountProfile.userID = reader.GetInt32(0);
                            accountProfile.employeeName = reader.GetString(1);
                            accountProfile.email = "mattis@24local.no";
                            accountProfile.companyID = reader.GetInt32(2);
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

            return accountProfile;
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

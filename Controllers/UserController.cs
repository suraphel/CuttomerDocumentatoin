using Microsoft.AspNet.Identity;
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
using System.Text;

namespace Local24API.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        private string LOCAL24WriteConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Write_Connection"].ConnectionString;

        //private AuthRepository _repo = null;
        //private UserManager<IdentityUser> _userManager;
        //private AuthContext _ctx;

        public UserController()
        {
            //_repo = new AuthRepository();
            //_ctx = new AuthContext();
            //_userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        }

        [Authorize]
        [HttpGet]
        [SwaggerOperation("Users")]
        [Route("Users")]
        public async Task<List<UserProfileModel>> Users(UserModel userModel)
        {
            var principal = ClaimsPrincipal.Current;
            var companyIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");
            List<UserProfileModel> userProfiles = new List<UserProfileModel>();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select i.bookingID, i.userID as intranetUserId, i.firstname, i.surname, i.adresse, " +
                        "i.postnr, i.poststed, i.cityID, i.mobilephone, u.email, b.employeeIsActive, b.employeeName, b.employeeUsername " +
                        "from rokea_intranet.jf_fixel_employees i " +
                        "join rokea_booking.sh_employee b on i.bookingID = b.employeeID " +
                        "join rokea_intranet.jf_users u on b.employeeName = u.username " +
                        "where i.cityID = "+ companyIdClaim.Value +" order by b.employeeName asc", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            UserProfileModel user = new UserProfileModel();
                            user.userID = reader.GetInt32(0);
                            user.id = reader.GetInt32(0).ToString(); //Brynthum Scheduler Resources needs id as string
                            user.intranetUserID = reader.GetInt32(1);
                            user.firstName = reader.GetString(2);
                            user.lastName = reader.GetString(3);
                            user.address = reader.GetString(4);
                            user.postalCode = reader.GetString(5);
                            user.postalCity = reader.GetString(6);
                            user.companyID = reader.GetInt32(7);
                            user.phone = reader.GetString(8);
                            user.email = reader.GetString(9);
                            user.isActive = reader.GetBoolean(10);
                            user.employeeName = reader.GetString(11);
                            user.employeeUserName = reader.GetString(12);
                            
                            userProfiles.Add(user);
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

            return userProfiles;
        }


        [Authorize]
        [Route("Create")]
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> Create(UserModel userModel)
        {
            var principal = ClaimsPrincipal.Current;
            var cityIDClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");
            
            HttpResponseMessage response;

            string MD5password = CreateMD5(userModel.password);
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            Object rokea_intranet_jf_users_lastID;
            Object rokea_booking_sh_employee_lastID = "";

            if (!ModelState.IsValid)
            {
                response = this.Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }

            
            try
            {
                using (MySqlConnection conn = new MySqlConnection(LOCAL24WriteConnString))
                {
                    //1. create user in table rokea_intranet.jf_users. Id of created user is used in next query
                    using (MySqlCommand cmd = new MySqlCommand("insert into rokea_intranet.jf_users(name, username, email, password, usertype, gid, registerdate, params) " +
                        "values('" + userModel.firstName + " " + userModel.lastName + "', '" + userModel.userName + "', '" + userModel.email + "', '" + MD5password + "', 'Registered', 18, '" + today + "', '');" +
                        "SELECT LAST_INSERT_ID();", conn))
                    {
                        conn.Open();
                        rokea_intranet_jf_users_lastID = cmd.ExecuteScalar();
                    }

                    //2. create user in table rokea_booking.sh_employee. Id of created user is used in next query
                    using (MySqlCommand cmd = new MySqlCommand("insert into rokea_booking.sh_employee (employeename, employeeusername, employeepassword, employeedescription, employeeimei, " +
                        "mobileno, pincodes, pukcodes, statoilcardnumber, branchno, vendorlist, loggingactivitycar, earnedcommission, creditlimitreached, certificatesandrates, supportnameandphone, " +
                        "telheadquarters, activityregistration, startdate, enddate, employeehaslonn, cityid, hvisvihartid) " +
                        "values ('"+userModel.userName +"', '" +userModel.firstName +" " +userModel.lastName +"', '"+ MD5password +"', '', '', '"+userModel.phone +"', '', '', '', '', '', '', '', '', '', '', '', '', date '"+today +"', date '3001-01-01', 1, "+ cityIDClaim.Value+", 0);" +
                        "SELECT LAST_INSERT_ID();", conn))
                    {
                        rokea_booking_sh_employee_lastID = cmd.ExecuteScalar();
                    }

                    //3. create user in table rokea_intranet.jf_fixel_employees.
                    using (MySqlCommand cmd = new MySqlCommand("insert into rokea_intranet.jf_fixel_employees(userid, name, adresse, postnr, poststed, personnr, avdeling, avdelingid, mamutid," +
                        " ansvarstilleggid, specialansvarstillegg, bookingid, cityid, empstatus, groupid, mobilephone, beregningid, ismontor, firstname, surname, salaryhour, statusmamut, startdate, issalgsingenior) " +
                        "values (" +rokea_intranet_jf_users_lastID +", '" +userModel.firstName +" " +userModel.lastName +"', '', '', '', '', '',1, 0, 0, 0, " + rokea_booking_sh_employee_lastID + ", " + cityIDClaim.Value + ", 0, 0, '"+userModel.phone +"', 0, 1, '"+userModel.firstName +"', '" +userModel.lastName +"', 0.0, 0, date '"+today +"', 1);", conn))
                    {
                        cmd.ExecuteScalar();
                    }


                    conn.Close();
                }
            }
            catch (Exception e)
            {
                response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }

            response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        [Authorize]
        [HttpPost]
        [SwaggerOperation("Update")]
        [Route("Update")]
        public async Task<HttpResponseMessage> Update(UserProfileModel userProfileModel)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(LOCAL24WriteConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE rokea_booking.sh_employee set employeeUsername = '" + userProfileModel.employeeUserName
                        + "', mobileNo = '" + userProfileModel.phone + "', employeeIsActive = " + userProfileModel.isActive + " where employeeID = '" + userProfileModel.userID + "'", conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        var response = this.Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                }
            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("Deactivate")]
        [SwaggerOperation("Deactivate")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> Deactivate(UserProfileModel userProfileModel)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(LOCAL24WriteConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE rokea_booking.sh_employee set employeeIsActive = 0 " +
                        "where employeeID = '" + userProfileModel.userID + "'", conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        var response = this.Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                }
            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
        }

        //[Authorize]
        [Route("ValidateEmail")]
        [SwaggerOperation("ValidateEmail")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> ValidateEmail(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select id from rokea_intranet.jf_users where email = '" + @email+ "'", connection))
                    {
                        cmd.Parameters.Add(new MySqlParameter("email", email));

                        connection.Open();
                        Object id = cmd.ExecuteScalar();
                        connection.Close();
                        if (id != null)
                            return Conflict();
                        else
                            return Ok();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //[Authorize]
        [Route("ValidateUserName")]
        [SwaggerOperation("ValidateUserName")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<IHttpActionResult> ValidateUserName(string userName)
        {
            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select id from rokea_intranet.jf_users where username = '" + @userName + "'", connection))
                    {
                        cmd.Parameters.Add(new MySqlParameter("userName", userName));

                        connection.Open();
                        Object id = cmd.ExecuteScalar();
                        connection.Close();
                        if (id != null)
                            return Conflict();
                        else
                            return Ok();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("Profile")]
        [SwaggerOperation("Profile")]
        public UserProfileModel Profile()
        {
            var principal = ClaimsPrincipal.Current;
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == "userName");
            UserProfileModel accountProfile = new UserProfileModel();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select e.employeeID, e.employeeName, e.employeeUsername, e.cityID, e.mobileNo, e.employeeIsActive, c.cityTitle " +
                        "from rokea_booking.sh_employee e " +
                        "join sh_cities c on e.cityID = c.cityID " +
                        "where employeeName = '" + idClaim.Value +"'", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            accountProfile.userID = reader.GetInt32(0);
                            accountProfile.employeeName = reader.GetString(1);
                            accountProfile.employeeUserName = reader.GetString(2);
                            accountProfile.companyID = reader.GetInt32(3);
                            accountProfile.phone = reader.IsDBNull(4) ? null : reader.GetString(4);
                            accountProfile.isActive = reader.GetBoolean(5);
                            accountProfile.id = reader.GetInt32(0).ToString(); //Brynthum Scheduler Resources needs id as string
                            accountProfile.companyName = reader.GetString(6);
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

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }

        }
    }
}

using MySql.Data.MySqlClient;
using Local24API.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web;
using System.Text;

namespace Local24API.Controllers
{
    [RoutePrefix("api/CustomerPortal")]
    public class CustomerPortalController : ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;

        [HttpPost]
        [Route("Login")]
        [SwaggerOperation("Login")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public dynamic LogIn() //this param causes cors-isssues when running both client and api on localhost...??
        {
            string customerId = "";
            string MD5password;

            //get username and pwd from HttpHeader. Base64 decode and validate.
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            string companyID = HttpContext.Current.Request.Headers["CompanyID"];  //2=Fixel, 3=Elmesteren, 4=Elektris, 6=Elmesteren

            //get userName and password from Base64Encoded authheader, then hash password using MD5Hash
            byte[] data = System.Convert.FromBase64String(authHeader.Replace("Basic ", ""));
            string base64Decoded;
            base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);
            string userName = base64Decoded.Substring(0, base64Decoded.IndexOf(":"));
            string pwd = base64Decoded.Substring(base64Decoded.IndexOf(":") + 1, base64Decoded.Length - 1 - base64Decoded.IndexOf(":"));
            MD5password = CreateMD5(pwd);

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    MySqlCommand command = new MySqlCommand("select companyId from sh_customer where companyId = '" + userName + "' and password = '" + MD5password + "'", connection);
                    connection.Open();
                    customerId = command.ExecuteScalar().ToString();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }

            if (customerId == "")
            {
                var response = this.Request.CreateResponse(HttpStatusCode.NotFound);
                return response;
            }
            else
            {
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
        }

        private bool Authorize(out string userName, out string pwd)
        {
            string customerId = "";
            string MD5password;
            string companyID = HttpContext.Current.Request.Headers["CompanyID"];  //2=Fixel, 3=Elmesteren, 4=Elektris, 6=Elmesteren
            string intranet_connection = "";
            MySqlCommand command;

            //get username and pwd from HttpHeader. Base64 decode and validate.
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            byte[] data = System.Convert.FromBase64String(authHeader.Replace("Basic ", ""));
            string base64Decoded;
            base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);
            userName = base64Decoded.Substring(0, base64Decoded.IndexOf(":"));
            pwd = base64Decoded.Substring(base64Decoded.IndexOf(":") + 1, base64Decoded.Length - 1 - base64Decoded.IndexOf(":"));

            //hash pwd
            MD5password = CreateMD5(pwd);
            intranet_connection = LOCAL24ReadConnString;
            command = new MySqlCommand("select companyId from sh_customer where companyId='" + userName + "' and password = '" + MD5password + "' and cityID = '" + companyID + "'");
            
            try
            {
                using (var connection = new MySqlConnection(intranet_connection))
                {
                    command.Connection = connection;
                    connection.Open();
                    customerId = command.ExecuteScalar().ToString();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            if (customerId == "")
            {
                return false;
            }
            else
            {
                return true;
            }
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
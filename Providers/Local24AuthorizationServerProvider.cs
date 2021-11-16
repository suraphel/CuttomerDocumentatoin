using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using MySql.Data.MySqlClient;
using Local24API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Local24API.Providers
{
    public class Local24AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            try
            {
                UserService userService = new UserService();
                var user = await userService.Authenticate(context.UserName, context.Password);

                if(user != null)
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                    identity.AddClaim(new Claim("sub", context.UserName));
                    identity.AddClaim(new Claim("role", "user"));

                    context.Validated(identity);
                }                
            }
            catch (Exception ex)
            {
                context.SetError("invalid_grant", ex.Message);
            }
        }
    }

    public class UserService : IDisposable
    {
        private string aa1_intranet_connection = "SERVER=db.rokea.no; PORT=3306; DATABASE=rokea_booking; UID=mattis;password=X$m#Jcqr229P5K@k;";

        public async Task<UserModel> Authenticate(string userName, string password)
        {
            string MD5password = CreateMD5(password);
            UserModel user = new UserModel();

            try
            {
                using (var connection = new MySqlConnection(aa1_intranet_connection))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select employeeName from sh_employee where employeeName= '"+userName +"' AND employeePassword = '" + MD5password + "'", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            user.UserName = reader[0].ToString();
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

            if (user.UserName == null)
                return null;

            return user;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
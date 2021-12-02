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
                UserModel user = await userService.Authenticate(context.UserName, context.Password);

                if(user != null)
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                    identity.AddClaim(new Claim("userID", user.userID));
                    identity.AddClaim(new Claim("userName", user.userName));
                    identity.AddClaim(new Claim("isAdmin", user.isAdmin));
                    identity.AddClaim(new Claim("cityID", user.cityID.ToString()));
                    identity.AddClaim(new Claim("companyName", user.companyName.ToString()));

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
        private string LOCAL24ConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        

        public async Task<UserModel> Authenticate(string userName, string password)
        {
            string MD5password = CreateMD5(password);
            UserModel user = new UserModel();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select e.employeeID, e.employeeName, e.employeeIsAdmin, e.cityID, c.cityTitle from sh_employee e " +
                        "join sh_cities c on  e.cityID = c.cityID " +
                        "where e.employeeName = '"+userName +"' AND e.employeePassword = '" + MD5password + "'", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            user.userID = reader[0].ToString();
                            user.userName = reader[1].ToString();
                            user.isAdmin = reader[2].ToString();
                            user.cityID = Convert.ToInt32(reader[3]);
                            user.companyName = reader[4].ToString();
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

            if (user.userName == null)
                return null;

            return user;
        }

        public UserModel FindByName(string userName)
        {
            UserModel user = new UserModel();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select e.employeeName, e.cityID, c.cityTitle from sh_employee e " +
                        "join sh_cities c on  e.cityID = c.cityID " +
                        "where e.employeeName = '" + userName + "'", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            user.userName = reader[0].ToString();
                            user.cityID = Convert.ToInt32(reader[1]);
                            user.companyName = reader[2].ToString();
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

            if (user.userName == null)
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
using MySql.Data.MySqlClient;
using Local24API.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections; // typeScript

using System.Web;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Local24API.Controllers
{
    [RoutePrefix("api/CustomerPortal")]
    public class CustomerPortalController : ApiController
    {
        private string rokea_booking = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        // need new connection to sql 
        private string connetionString = System.Configuration.ConfigurationManager.ConnectionStrings["RokeaSQLConnString"].ConnectionString;

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
                using (var connection = new MySqlConnection(rokea_booking))
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


        
        // connects to the Amazon server and fetch data for a jobID
        [HttpGet]
        [Route("GetJobs")]
        [SwaggerOperation("GetJobs")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public List<CustomerJobModel> GetJobs(int jobID) // sm edit, a data type CustomerJobModel init it has getter functions
        {
            // Containers
            List<CustomerJobModel> CustomerModel = new List<CustomerJobModel>(); 
            List<CustomerPortalModel> CustomersPortal = new List<CustomerPortalModel>();
            List<string> Customer = new List<string>();  // made a list                


            // Readers
            MySqlDataReader reader = null;
            SqlDataReader dataReader = null;

            try
            {  // Azure server 
                using (var connection = new MySqlConnection(rokea_booking))  // Connection to Rokea_booking           

                {                    
                    using (MySqlCommand cmd = new MySqlCommand("SELECT jobId, EmployeeId, creationTime from sh_job where jobID = " + jobID, connection))
                    {
                        //using (MySqlCommand cmd = new MySqlCommand("SELECT JobId, EmployeeId,  creationTime from sh_job where companyId = " + companyId, connection))                  {
                        connection.Open();
                        MySqlDataReader readers = cmd.ExecuteReader();
                        while (readers.Read())
                        {
                            CustomerJobModel Customers = new CustomerJobModel();
                            Customers.jobId = readers.IsDBNull(0) ? null : readers.GetString(0);
                            Customers.EmployeeId = readers.IsDBNull(1) ? null : readers.GetString(1);
                            Customers.creationTime = readers.IsDBNull(2) ? null : readers.GetString(2);
                            //Customer.SurveyCompleted = reader.GetBoolean(3);  // this is not coming from the db
                            CustomerModel.Add(Customers);
                        }
                        readers.Close();
                    }
                    connection.Close();
                }
                // connection to the sql server 


                //using (var connection = new SqlConnection(connetionString)) // est connection parameter
                //{
                //    connection.Open(); //open connection
                //    string cmdTxt = ("Select jobId from CustomerSurveyAnswers");
                //    using (var cmd = new SqlCommand(cmdTxt, connection)) // say what it shall fetch
                //    {
                //        reader = cmd.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            CustomerPortalModel CustomerPortal = new CustomerPortalModel();
                //            CustomerPortal.jobId = reader[0].ToString();
                //            CustomersPortal.Add(CustomerPortal); // add the data to a list                              
                //        }
                //    }
                //    connection.Close();
                //}
                //if (CustomerModeList.Count < 1 )
                //{
                //    Console.WriteLine(" Empty List");
                //}
                //else
                //{
                //      foreach(CustomerPortalModel CustomerPortal in Customer)
                //    {                       
                //        if (CustomerModeList.Contains(CustomerPortal.jobId))
                //        {
                //            CustomerPortal.SurveyCompleted = true;
                //        }
                //        else
                //        {
                //            CustomerPortal.SurveyCompleted = false;    
                //        }

                //    }
                }
        
            catch (Exception e)
            {
                throw e;
            }
            return CustomerModel;
        }


        // Will need an entry point : OWIN auto start!!!
        // Connects to both Db's and verifies var status on jobId from customersurveyanswers to jobId on sh_jobs. 
        
        [HttpGet]
        [Route("GetJobsId")]
        [SwaggerOperation("GetJobsId")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public dynamic GetJobsId(int jobID)
        // switched CustomerJobModel with CustomerPortalModel 
        {
            string userName, pwd;
            string intranet_connection = "";

            //string companyID = HttpContext.Current.Request.Headers["CompanyID"];
            //2=Fixel, 3=Elmesteren, 4=Elektris, 6=Elfiksern // get which company is connected.? 

            List<CustomerPortalModel> CustomerJobList = new List<CustomerPortalModel>();  
            List<string> customerSurveysJobList = new List<string>(); 

            MySqlDataReader mySqlDataReader; // MySqlDataReader is a data type/ Class and mySqlDataReader is a local variable
            SqlDataReader sqlDataReader;  // MySqlDataReader is a data type/ Class and mySqlDataReader is a local variable

            string sqlCommandText = "";

            //set MySql connectionstring and MySQLCommand based on companyID

            //intranet_connection = connetionString; // connection to the  mySql workbench
            sqlCommandText = "SELECT jobId, EmployeeId from sh_job where jobID = " + jobID; 

            //intranet_connection = rokea_booking;
            //sqlCommandText = " "; 


             List<CustomerSurveyAnswersModel> customerSurveysJobLists = new List<CustomerSurveyAnswersModel>();
            SqlDataReader reader;

            try
            {
                using (var connection = new MySqlConnection(rokea_booking))
                {
                    connection.Open();

                    // Creates a SQL command
                    using (var cmd = new MySqlCommand(sqlCommandText, connection))
                    {
                        mySqlDataReader = cmd.ExecuteReader();
                        while (mySqlDataReader.Read())
                        {
                            CustomerPortalModel  customerJob = new CustomerPortalModel();
                            customerJob.jobId = mySqlDataReader[0].ToString();
                            customerJob.EmployeeId = mySqlDataReader[1].ToString();
                          // customerJob.SurveyCompleted = ;                          
                            CustomerJobList.Add(customerJob);
                        }
                    }
                    connection.Close();
                }
                //Connecition to MYSQL workbench server!!!!!!!!!!!! 

                using (var connection = new SqlConnection(connetionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("select jobId from customersurveyanswers where jobID = " + jobID, connection))
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            // customerSurveysJobList.Add(reader[0].ToString());
                            CustomerSurveyAnswersModel customerJob = new CustomerSurveyAnswersModel();
                            customerJob.jobId = reader[0].ToString();
                           // customerJob.employeeId = reader[1].ToString();                            
                            customerSurveysJobList.Add(customerJob.ToString());
                        }
                    }

                    connection.Close();
                }

                if (CustomerJobList.Count < 1)
                {
                    var response = this.Request.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }
                else
                {
                    foreach (CustomerPortalModel customerJob in CustomerJobList)
                    {
                        if (!customerSurveysJobList.Contains(customerJob.jobId))
                        {
                            customerJob.SurveyCompleted = true;
                        }
                        else
                        {
                            customerJob.SurveyCompleted = false;
                        }
                    }
                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
                     return Json(CustomerJobList);

                }
            }
            catch (Exception e)
            {
                // is this answer beig written on the db???????
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
        }
        /*   public List<CustomerPortalModel> GetJobsIdList()
           {
               List<CustomerPortalModel> GetJobsIdList = new List<CustomerPortalModel>();
               string userName = "", pwd;
               string intranet_connection = "";

               // outputting null 
               string companyID = HttpContext.Current.Request.Headers["CompanyID"];  //2=Fixel, 3=Elmesteren, 4=Elektris, 6=Elfiksern  // this is not returning any thing~~
               companyID = "1";

               //List<CustomerPortalModel> CustomerJobList = new List<CustomerPortalModel>();
               List<string> customerSurveysJobList = new List<string>();

               MySqlDataReader mySqlDataReader; //Amazon : 
               SqlDataReader sqlDataReader; //Azure : workbench
               string sqlCommandText = "";

               //set MySql connectionstring and MySQLCommand based on companyID 
               //if (companyID == "2")
               //{
               intranet_connection = connetionString;
               sqlCommandText = "select jobId, createdDate from CustomerSurveyAnswers";

               // sql connection test by SM and this goes to Azure 
               using (SqlConnection connection = new SqlConnection(connetionString))
               {
                   SqlCommand command = new SqlCommand(sqlCommandText, connection);
                   connection.Open();

                   SqlDataReader reader = command.ExecuteReader();

                   while (reader.Read())
                   {
                       CustomerPortalModel jobids = new CustomerPortalModel();
                       jobids.jobId = reader.IsDBNull(0) ? null : Convert.ToString(reader.GetInt32(0));
                       jobids.creationTime = reader.IsDBNull(1) ? null : Convert.ToString(reader.GetDateTime(1));
                       //ReadSingleRow(IDataRecord)reader);
                       GetJobsIdList.Add(jobids);
                   }
                   reader.Close();
               }
               void ReadSingleRow(IDataRecord dataRecord)
               {
                   Console.WriteLine(String.Format("{0}, {1}", dataRecord[0], dataRecord[1]));
               }
               Console.WriteLine(sqlCommandText);
               Console.WriteLine("we have made it this far");

               using (var connection = new MySqlConnection(rokea_booking))

               using (MySqlCommand cmd = new MySqlCommand("SELECT  jobId, creationTime from sh_job", connection))
               {
                   connection.Open();
                   MySqlDataReader reader = cmd.ExecuteReader();
                   while (reader.Read())
                   {
                       CustomerJobModel Customer = new CustomerJobModel();
                       Customer.jobId = reader.IsDBNull(0) ? null : reader.GetString(0);                            
                       //Customer.creationTime = reader.IsDBNull(1) ? null : reader.GetString(1);
                       //Customer.SurveyCompleted = reader.GetBoolean(3); 
                       customerSurveysJobList.Add(Convert.ToString(Customer)); // will need to check
                   }

                   try
                   {
                       using (var connections = new MySqlConnection(connetionString))
                       {
                           connection.Open();
                           // bool 
                           // Creates a SQL command
                           using (var cmds = new MySqlCommand(sqlCommandText, connection))
                           {
                               mySqlDataReader = cmd.ExecuteReader();
                               while (mySqlDataReader.Read())
                               {
                                   CustomerPortalModel customerJobid = new CustomerPortalModel();

                                   // customerJobid.lastName = mySqlDataReader[1].ToString();
                                   //customerJob.InvoiceId = mySqlDataReader[2].ToString();
                                   //customerJob.EmployeeId = mySqlDataReader[3].ToString();

                                   GetJobsIdList.Add(customerJobid);
                               }
                           }
                           connection.Close();
                       }

                       using (var connections = new SqlConnection(rokea_booking))
                       {
                           connection.Open();

                           using (var cmds = new SqlCommand("select jobId from customersurveyAnswers", connections))
                           {
                               sqlDataReader = cmds.ExecuteReader();
                               while (sqlDataReader.Read())
                               {
                                   customerSurveysJobList.Add(sqlDataReader[0].ToString());
                               }
                           }
                           connection.Close();
                       }

                       if (GetJobsIdList.Count < 1)
                       {
                           var response = this.Request.CreateResponse(HttpStatusCode.NotFound);
                           //return response;
                       }
                       else
                       {
                           foreach (CustomerPortalModel customerJob in GetJobsIdList)
                           {
                               if (!customerSurveysJobList.Contains(customerJob.jobId))
                               {
                                   customerJob.SurveyCompleted = false;
                               }
                               else
                               {
                                   customerJob.SurveyCompleted = true;
                               }
                           }
                           var response = this.Request.CreateResponse(HttpStatusCode.OK);
                           //return Json(GetJobsIdList);
                       }
                   }

                   catch (Exception e)
                   {
                       var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                       //return response;
                   }
                   return GetJobsIdList;
               }
           }
        */

        

        // working  Connedted to the SSMS
        [HttpGet]
        [Route("CustomerSurveyQuestions")]
        [SwaggerOperation("CustomerSurveyQuestions")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public dynamic CustomerSurveyQuestions()
        {

            SqlDataReader reader;

            List<CustomerSurveyQuestionsModel> questionList = new List<CustomerSurveyQuestionsModel>();
            try
            {
                using (var connection = new SqlConnection(connetionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("select id, question from customersurveyquestions", connection))
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {

                            CustomerSurveyQuestionsModel question = new CustomerSurveyQuestionsModel();
                            question.Id = reader[0].ToString();
                            question.Question = reader[1].ToString();
                            questionList.Add(question);
                        }
                    }

                    connection.Close();
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return Json(questionList);

            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
        }


        // to be posted to the SSMS
        [HttpPost]
        [Route("CustomerSurveyAnswers")]
        [SwaggerOperation("CustomerSurveyAnswers")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public dynamic CustomerSurveyAnswers(int customerId, int jobId, int employeeId, int prisScore, int kvalitetScore, int effektivitetScore,
        int palitelighetScore, int gjenbrukScore, int anbefaleScore, string annetText, int companyID)
        {
            string userName, pwd;

            Authorize(out userName, out pwd);

            if (userName == "")
            {
                var response = this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
            }

            try
            {
                using (var connection = new SqlConnection(connetionString))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(14, " + customerId + "," + prisScore + ",'" + annetText + "'," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(15, " + customerId + "," + kvalitetScore + ",''," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(16, " + customerId + "," + effektivitetScore + ",''," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(17, " + customerId + "," + palitelighetScore + ",''," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(18, " + customerId + "," + anbefaleScore + ",''," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("insert into customersurveyAnswers (questionId, customerId, answer, comment, employeeId, jobId, companyID, createdDate) VALUES(19, " + customerId + "," + gjenbrukScore + ",''," + employeeId + "," + jobId + "," + companyID + ", getdate())", connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
        }

        //Støtteklasser
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
            intranet_connection = rokea_booking; //LOCAL24ReadConnString;
            command = new MySqlCommand("select companyId from sh_customer where companyId='" + userName + "' and password = '" + MD5password + "'");

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



        // getDocument 

    }



}























/*// it needs to connect to the Db 



          HttpResponseMessage response1 = response;
                    return response1;

            // it needs to Get data based on some parameter 
            // it needs to render that msg 
            //

            string GetJobs ="";
            string intranet_connection = "";

            if (GetJobs == "")
            {
                //var response = this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                //return response;
                Console.WriteLine("got this far"); 
            }
            //string companyID = HttpContext.Current.Request.Headers["CompanyID"];  //2=Fixel, 3=Elmesteren, 4=Elektris, 6=Elfiksern

            List<CustomerJobModel> CustomerJobList = new List<CustomerJobModel>();
            List<string> customerSurveysJobList = new List<string>();

            MySqlDataReader mySqlDataReader;
            SqlDataReader sqlDataReader;
            string sqlCommandText = "";


           //set MySql connectionstring and MySQLCommand based on companyID
            if (companyID == "8")
            {

                // rokea_bookin needs to have a connection here. 
                intranet_connection = rokea_booking;

                sqlCommandText = "select jobID, DATE_FORMAT(dato, '%d-%m-%Y') as jobbdato, fgrnr as fakturanummer, userId " +
                        "from elektris_intranet.jf_fixel_fakturagrunnlag " +
                        "join elektris_booking.sh_customer on elektris_booking.sh_customer.kundeNo = elektris_intranet.jf_fixel_fakturagrunnlag.kundeNr " +
                        "where elektris_booking.sh_customer.companyID = " + GetJobs + " order by dato desc";
            }
            else
            {
                //intranet_connection = aa1_intranet_connection;
                //sqlCommandText = "select jobID, DATE_FORMAT(dato, '%d-%m-%Y') as jobbdato, fgrnr as fakturanummer, userId " +
                //        "from aa1_intranet.jf_fixel_fakturagrunnlag " +
                //        "join aa1_booking.sh_customer on aa1_booking.sh_customer.kundeNo = aa1_intranet.jf_fixel_fakturagrunnlag.kundeNr " +
                //        "where aa1_booking.sh_customer.companyID = " + GetJobs + " order by dato desc";

                Console.WriteLine("No db found"); 
            }

            try
            {
                using (var connection = new MySqlConnection(intranet_connection))
                {
                    connection.Open();

                    // Creates a SQL command
                    using (var cmd = new MySqlCommand(sqlCommandText, connection))
                    {
                        mySqlDataReader = cmd.ExecuteReader();
                        while (mySqlDataReader.Read())
                        {
                            CustomerJobModel customerJob = new CustomerJobModel();
                            customerJob.JobId = mySqlDataReader[0].ToString();
                            customerJob.Date = mySqlDataReader[1].ToString();
                            //customerJob.InvoiceId = mySqlDataReader[2].ToString();
                            customerJob.EmployeeId = mySqlDataReader[3].ToString();

                            CustomerJobList.Add(customerJob);
                        }
                    }
                    connection.Close();
                }

                using (var connection = new SqlConnection(rokea_booking))
                {
                    connection.Open();

                    using (var cmd = new SqlCommand("select jobId from customersurveyAnswers", connection))
                    {
                        sqlDataReader = cmd.ExecuteReader();
                        while (sqlDataReader.Read())
                        {
                            customerSurveysJobList.Add(sqlDataReader[0].ToString());
                        }
                    }


                    connection.Close();
                }

                if (CustomerJobList.Count < 1)
                {
                    var response = this.Request.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }
                else
                {
                    foreach (CustomerJobModel customerJob in CustomerJobList)
                    {
                        if (!customerSurveysJobList.Contains(customerJob.JobId))
                        {
                            customerJob.SurveyCompleted = false;
                        }
                        else
                        {
                            customerJob.SurveyCompleted = true;
                        }
                    }
                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
                    return Json(CustomerJobList);
                }

            }
            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            
 */
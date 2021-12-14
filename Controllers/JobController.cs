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
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using Local24API.Providers;

namespace Local24API.Controllers
{
    [RoutePrefix("api/Job")]
    public class JobController : ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        private string LOCAL24WriteConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Write_Connection"].ConnectionString;

        public JobController()
        {

        }

        [Authorize]
        [HttpGet]
        [Route("JobEmployeeArray")]
        [SwaggerOperation("JobEmployeeArray")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public List<UserProfileModel> JobEmployeeArray()
        {
            var principal = ClaimsPrincipal.Current;
            var cityIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");

            List<UserProfileModel> employeeList = new List<UserProfileModel>();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("select employeeId, employeeName, employeeUsername, cityID, employeeIsActive from sh_employee " +
                        "where cityID = " + cityIdClaim.Value + " and (groupID = 0 or groupID=2) order by employeeName asc", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            UserProfileModel employee = new UserProfileModel();
                            employee.userID = reader.GetInt16(0);
                            employee.id = reader.GetInt16(0).ToString();
                            employee.employeeName = reader.GetString(1);
                            employee.employeeUserName = reader.GetString(2);
                            employee.companyID = reader.GetInt16(3);
                            employee.isActive = reader.GetBoolean(4);
                            employeeList.Add(employee);

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
            return employeeList;

        }


        [Authorize]
        [HttpGet]
        [Route("Jobs")]
        [SwaggerOperation("Jobs")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public List<JobModel> Jobs(string startDateTime)
        {
            var principal = ClaimsPrincipal.Current;
            var cityIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");
            

            MySqlDataReader sqlDataReader;
            List<JobModel> jobsList = new List<JobModel>();

            string jobStart = String.Format("{0:yyyy-MM-dd:HH:mm:ss}", startDateTime);

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    connection.Open();

                    using (var cmd = new MySqlCommand("SELECT j.jobID, j.employeeID, j.jobBookerID, j.jobTitle, j.jobStatus, j.JobAdress, j.jobPostal, j.jobCity," +
                            "j.jobContactName, j.jobContactMobileNumber, j.JobStart, j.JobStop, j.kundeNo, j.reclamation, j.fromJobID, t.name as jobType, e.cityID, e2.employeeUsername, gender AS genderType, " +
                            "j.jobDescription, ageId, fgrStatus, j.companyID, orderNo, fakturaVS, buildYear, sourceID, oppussingdate, oppussingjob, statussikring, infratekrapportnr, " +
                            "molerID, c.companyEmail, companyInvoiceAdress, companyInvoicePostal, companyInvoiceCity, c.companyName, c.contactName, c.contactMobileNumber, c.receiveSms " +
                            "FROM sh_job j LEFT JOIN sh_job_types t on j.jobTypeId = t.id " +
                            "LEFT JOIN sh_employee e on j.employeeID = e.employeeId " +
                            "LEFT JOIN sh_employee e2 on j.jobBookerID = e2.employeeId " +
                            "LEFT JOIN sh_customer c on c.companyID = j.companyID " +
                            "WHERE j.cityID= "+cityIdClaim.Value +" AND DATE(jobstart) = '" + jobStart + "'", connection))
                    {
                        sqlDataReader = cmd.ExecuteReader();
                        while (sqlDataReader.Read())
                        {
                            string startDate = sqlDataReader.GetDateTime(10).ToString("yyyy/MM/dd H:mm").Replace("/", "-");
                            string endDate = sqlDataReader.GetDateTime(11).ToString("yyyy/MM/dd H:mm").Replace("/", "-");

                            JobModel job = new JobModel();
                            job.jobId = sqlDataReader.GetInt32(0);
                            job.resourceId = sqlDataReader.GetInt32(1).ToString(); ;
                            job.jobBookerID = sqlDataReader.GetInt32(2);
                            job.name = sqlDataReader.GetString(3);
                            job.jobStatus = sqlDataReader.GetInt32(4);
                            job.JobAdress = sqlDataReader.GetString(5);
                            job.jobPostal = sqlDataReader.IsDBNull(6) ? null : sqlDataReader.GetString(6);
                            job.jobCity = sqlDataReader.IsDBNull(7) ? null : sqlDataReader.GetString(7);
                            job.alternativeJobContactName = sqlDataReader.IsDBNull(8) ? null : sqlDataReader.GetString(8);
                            job.alternativeJobContactMobileNumber = sqlDataReader.IsDBNull(9) ? null : sqlDataReader.GetString(9);
                            job.startDate = startDate;
                            job.endDate = endDate;
                            job.kundeNo = sqlDataReader.IsDBNull(12) ? null : sqlDataReader.GetString(12);
                            job.reclamation = sqlDataReader.IsDBNull(13) ? null : sqlDataReader.GetString(13);
                            job.fromJobID = sqlDataReader.IsDBNull(14) ? null : sqlDataReader.GetString(14);
                            job.jobType = sqlDataReader.IsDBNull(15) ? null : sqlDataReader.GetString(15);
                            job.jobBookerName = sqlDataReader.IsDBNull(17) ? null : sqlDataReader.GetString(17);
                            job.genderType = sqlDataReader.IsDBNull(18) ? null : sqlDataReader.GetString(18);
                            job.jobDescription = sqlDataReader.IsDBNull(19) ? null : sqlDataReader.GetString(19);
                            job.ageType = sqlDataReader.IsDBNull(20) ? null : sqlDataReader.GetString(20);
                            job.fgrStatus = sqlDataReader.GetInt32(21);
                            job.companyID = sqlDataReader.IsDBNull(22) ? 0 : sqlDataReader.GetInt32(22);
                            job.orderNo = sqlDataReader.IsDBNull(23) ? null : sqlDataReader.GetString(23);
                            job.fakturaVS = sqlDataReader.IsDBNull(24) ? null : sqlDataReader.GetString(24);
                            job.buildYear = sqlDataReader.IsDBNull(25) ? null : sqlDataReader.GetString(25);
                            job.marketingSource = sqlDataReader.IsDBNull(26) ? null : sqlDataReader.GetString(26);
                            job.oppussingdate = sqlDataReader.IsDBNull(27) ? null : sqlDataReader.GetString(27);
                            job.oppussingjob = sqlDataReader.IsDBNull(28) ? null : sqlDataReader.GetString(28);
                            job.statusSikring = sqlDataReader.IsDBNull(29) ? null : sqlDataReader.GetString(29);
                            job.infratekRapportNr = sqlDataReader.IsDBNull(30) ? null : sqlDataReader.GetString(30);
                            job.maalerNr = sqlDataReader.IsDBNull(31) ? null : sqlDataReader.GetString(31);
                            job.customerEmail = sqlDataReader.IsDBNull(32) ? null : sqlDataReader.GetString(32);
                            job.companyInvoiceAdress = sqlDataReader.IsDBNull(33) ? null : sqlDataReader.GetString(33);
                            job.companyInvoicePostal = sqlDataReader.IsDBNull(34) ? null : sqlDataReader.GetString(34);
                            job.companyInvoiceCity = sqlDataReader.IsDBNull(35) ? null : sqlDataReader.GetString(35);
                            job.companyName = sqlDataReader.IsDBNull(36) ? null : sqlDataReader.GetString(36);
                            job.companyInvoiceContactName = sqlDataReader.IsDBNull(37) ? null : sqlDataReader.GetString(37);
                            job.contactMobileNumber = sqlDataReader.IsDBNull(38) ? null : sqlDataReader.GetString(38);
                            job.receiveSMS = sqlDataReader.IsDBNull(38) ? 0 : sqlDataReader.GetInt16(39);  


                            job.resourceImage = "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";

                            if (job.reclamation == "1") //job is "Reklamasjon"
                            {
                                job.eventType = "Reklamasjon";
                                job.eventColor = "#EF8157"; // red
                            }
                            else if (job.jobStatus > 19) //job is "Interntid" -> table sh_statuses
                            {
                                job.eventType = "Opptatt";
                                job.eventColor = "#A0A0A0";
                            }

                            else if (job.jobType == "Befaring") //job is "Befaring"
                            {
                                job.eventType = "Befaring";
                                job.eventColor = "#51BCDA"; //blue
                            }

                            else
                            {
                                job.eventType = "Jobb";
                                job.eventColor = "#6BD098"; //green
                            }


                            if (job.jobStatus == 2) //Jobben er påbegynt
                            {
                                job.eventColor = "#d0cb6b"; //yellow
                            }
                            if (job.jobStatus == 3 || job.jobStatus == 7) //Jobben er ferdig
                            {
                                job.eventColor = "#d0986b"; //dark orange
                            }

                            jobsList.Add(job);
                        }
                    }

                    connection.Close();

                }

                return jobsList;
                //return Json(customerProfile);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("JobsForCustomer")]
        [SwaggerOperation("JobsForCustomer")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<List<JobModel>> JobsForCustomer(int customerID)
        {
            MySqlDataReader sqlDataReader;
            List<JobModel> jobList = new List<JobModel>();

            string queryString = "select j.jobID, Date(j.jobStart), j.jobTitle, e.employeeUsername, j.jobAdress, j.jobPostal, j.jobCity, j.companyID, j.jobDescription " +
                "from rokea_booking.sh_job j join sh_employee e on j.employeeID = e.employeeID where j.companyID = " +customerID +" order by Date(j.jobStart) desc";

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    connection.Open();

                    using (var cmd = new MySqlCommand(queryString, connection))
                    {
                        sqlDataReader = cmd.ExecuteReader();
                        while (sqlDataReader.Read())
                        {
                            JobModel job = new JobModel();

                            job.jobId = sqlDataReader.GetInt32(0);
                            job.startDate = sqlDataReader.GetString(1);
                            job.name = sqlDataReader.GetString(2);
                            job.employeeUserName = sqlDataReader.GetString(3);
                            job.JobAdress = sqlDataReader.GetString(4);
                            job.jobPostal = sqlDataReader.GetString(5);
                            job.jobCity = sqlDataReader.GetString(6);
                            job.companyID = sqlDataReader.IsDBNull(7) ? 0 : sqlDataReader.GetInt32(7);
                            job.jobDescription = sqlDataReader.IsDBNull(8) ? null : sqlDataReader.GetString(8);

                            jobList.Add(job);
                        }
                    }
                }

                return jobList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Authorize]
        [Route("CreateJob")]
        [SwaggerOperation("CreateJob")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> Create(string customerId, string customerName, string contactMobileNumber, string contactPhoneNumber, 
          string eventType, string jobTitle, string jobType, string jobContactName, string smsAndEmailChecked, string startDateTime, string stopDateTime, 
          string employeeID, string jobDescription, string jobAdress, string jobPostal, string jobCity, bool isNewCustomer, string customerEmail, 
          string companyInvoiceAdress, string companyInvoiceCity, string companyInvoicePostal, string alternativeJobContactName, string alternativeJobContactMobileNumber, 
          string companyType, string companyOrgNr, string companyContactName, string fromJobID)
        {
            var principal = ClaimsPrincipal.Current;
            var userNameClaim = principal.Claims.FirstOrDefault(c => c.Type == "userName");
            var cityIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");
            var userIDClaim = principal.Claims.FirstOrDefault(c => c.Type == "userID");

            var errroresponse = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
            string createdDate = String.Format("{0:yyyy-MM-dd:HH:mm:ss}", DateTime.Now.AddHours(2));
            string jobBookerID = userIDClaim.Value;
            int jobTypeId = 30; // 30 is Reklamasjon in rokea_booking.sh_job_types
            int newCustomer = 0;
            int reclamation = 0;

            if (eventType == "Reklamasjon")
            {
                reclamation = 1;
                jobTypeId = 32; // 32 is Reklamasjon in rokea_booking.sh_job_types
            }

            if (eventType == "Befaring")
            {
                jobTypeId = 26; // 26 is Befaring in rokea_booking.sh_job_types
            }


            if (isNewCustomer) // ----->> CREATE CUSTOMER IF NEW <<----- //
            {
                CustomerController customerController = new CustomerController();
                int companyID = customerController.Create(customerName, jobAdress, jobPostal, companyContactName, contactMobileNumber, jobCity, customerEmail, employeeID, createdDate, 
                    companyType, cityIdClaim.Value, companyInvoiceAdress, companyInvoicePostal, companyInvoiceCity, jobContactName, alternativeJobContactName,
                    alternativeJobContactMobileNumber, smsAndEmailChecked);
            }

            if (!isNewCustomer) // ----->> EXISTING CUSTOMER -> UPDATE //
            {
                CustomerController customerController = new CustomerController();
                customerController.Update(customerId, customerName, jobAdress, jobPostal, companyContactName, contactMobileNumber, jobCity, customerEmail, employeeID, createdDate,
                    companyType, cityIdClaim.Value, companyInvoiceAdress, companyInvoicePostal, companyInvoiceCity, jobContactName, alternativeJobContactName,
                    alternativeJobContactMobileNumber, smsAndEmailChecked);
            }


            // ----->> IS ALTERNATIVE INVOICE ADDRESS BEEING USED? <<----- //
            object jobID;
            var useCompanyInvoiceAdress = 0;
            if (alternativeJobContactName != "undefined")
            {
                jobContactName = alternativeJobContactName;
                contactMobileNumber = alternativeJobContactMobileNumber;
                useCompanyInvoiceAdress = 1;
            }

            // ----->> CREATE JOB <<----- //
            try
            {
                using (var connection = new MySqlConnection(LOCAL24WriteConnString))
                {
                   

                    connection.Open();

                   
                    // Creates job on customer (jobstatus=1 -> "Ikke påbegynt" (sh_statuses)  )
                    using (var cmd = new MySqlCommand("INSERT INTO sh_job(employeeID, jobBookerID, jobTitle, jobDescription, companyID, jobStatus, jobAdress, jobPostal, " +
                        "jobContactName, jobContactMobileNumber, jobStart, jobStop, fromPartnerID, jobCity, creationTime, useCompanyInvoiceAdress, kundeNo, saksbehandlerID, " +
                        "newKunde, campaignID, sourceID, fromJobID, reclamation, jobTypeId, gender, ageId, infratekrapportnr, buildyear, oppussingdate, oppussingjob, statussikring, " +
                        "paymentType, molerID, cityID) " +
                        "VALUES('" + employeeID + "', '" + jobBookerID + "', '" + jobTitle + "', '" + jobDescription + "', '" + customerId + "', '1', '" + jobAdress + "', '" +
                        jobPostal + "', '" + jobContactName + "', '" + contactMobileNumber + "','" + startDateTime + "', " + "'" + stopDateTime + "', '0', '" +
                        jobCity + "', '" + createdDate + "'," + useCompanyInvoiceAdress + ", '" + customerId + "', '" + employeeID + "', '" + 
                        newCustomer + "','0','0','" +fromJobID +"','" +reclamation +"','" + jobTypeId +"','0','0','0','0','0','0','0','0','0'," +cityIdClaim.Value +"); SELECT LAST_INSERT_ID(); ", connection))
                    {
                        jobID = cmd.ExecuteScalar();

                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Could not create JOB.");
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        [HttpPost]
        [Authorize]
        [Route("CreateBusy")]
        [SwaggerOperation("CreateBusy")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<HttpResponseMessage> CreateBusy(string employeeID, string startDateTime, string stopDateTime,
            string jobTitle, string jobDescription, string standardarbeidID)
        {
            var principal = ClaimsPrincipal.Current;
            var userNameClaim = principal.Claims.FirstOrDefault(c => c.Type == "userName");
            var cityIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");
            var userIDClaim = principal.Claims.FirstOrDefault(c => c.Type == "userID");

            string createdDate = String.Format("{0:yyyy-MM-dd:HH:mm:ss}", DateTime.Now.AddHours(2));

            string stda = standardarbeidID + "|";
            try
            {
                MySqlConnection connection = new MySqlConnection(LOCAL24WriteConnString);
                connection.Open();

                string query = "INSERT INTO sh_job(employeeID, jobBookerID, jobTitle, jobDescription, companyID, jobStatus, jobAdress, jobPostal, " +
                    "jobContactName, jobContactMobileNumber, jobStart, jobStop, creationTime, kundeNo, jobTypeId, stda, cityID) " +
                    "VALUES (@employeeID, @jobBookerID, @jobTitle, @jobDescription, @companyID, @jobStatus, @jobAdress, @jobPostal, @jobContactName, " +
                    "@jobContactMobileNumber, @jobStart, @jobStop, @creationTime, @kundeNo, @jobTypeId, @stda, @cityID); SELECT LAST_INSERT_ID()";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@employeeID", employeeID);
                command.Parameters.AddWithValue("@jobBookerID", userIDClaim.Value);
                command.Parameters.AddWithValue("@jobTitle", jobTitle);
                command.Parameters.AddWithValue("@jobDescription", jobDescription);
                command.Parameters.AddWithValue("@companyID", 0);
                command.Parameters.AddWithValue("@jobStatus", 30);
                command.Parameters.AddWithValue("@jobAdress", "");
                command.Parameters.AddWithValue("@jobPostal", "");
                command.Parameters.AddWithValue("@jobContactName", "");
                command.Parameters.AddWithValue("@jobContactMobileNumber", "");
                command.Parameters.AddWithValue("@jobStart", startDateTime);
                command.Parameters.AddWithValue("@jobStop", stopDateTime);
                command.Parameters.AddWithValue("@creationTime", createdDate);
                command.Parameters.AddWithValue("@kundeNo", "");
                command.Parameters.AddWithValue("@jobTypeId", 31); //31 is Opptatt in table sh_job_types
                command.Parameters.AddWithValue("@stda", stda);
                command.Parameters.AddWithValue("@cityID", cityIdClaim.Value);

                command.ExecuteScalar();

                connection.Close();
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Could not create internal time.");
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        [HttpGet]
        [Authorize]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("GoogleHeatMapData")]
        [SwaggerOperation("GoogleHeatMapData")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<dynamic> GoogleHeatMapData(int companyID, string date)
        {
            var principal = ClaimsPrincipal.Current;
            var cityIDClaim = principal.Claims.FirstOrDefault(c => c.Type == "cityID");

            MySqlDataReader mySqlDataReader;
            List<GoogleHeatMapDataModel> addressList = new List<GoogleHeatMapDataModel>();
            string connection;

            string queryString = "select j.jobAdress, j.jobPostal, j. jobCity, j.jobTitle, j.jobDescription, Time(j.jobStart), e.employeeUserName, c.companyName, contactMobileNumber " +
                "from sh_job j " +
                "join sh_employee e on j.employeeID = e.employeeID " +
                "join sh_customer c on j.companyID = c.companyID " +
                "where Date(j.jobStart) = "+date +" AND j.jobStatus NOT IN(20, 21, 22, 23, 24, 26, 27, 28, 29, 30) AND e.cityID =" + cityIDClaim.Value;
            

            try
            {
                using (var conn = new MySqlConnection(LOCAL24ReadConnString))
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand(queryString, conn))
                    {
                        mySqlDataReader = cmd.ExecuteReader();
                        while (mySqlDataReader.Read())
                        {
                            if (mySqlDataReader[0].ToString() != "")
                            {
                                GoogleHeatMapDataModel address = new GoogleHeatMapDataModel();
                                address.street = mySqlDataReader[0].ToString();
                                address.postal = mySqlDataReader[1].ToString();
                                address.city = mySqlDataReader[2].ToString();
                                address.jobTitle = mySqlDataReader.IsDBNull(3) ? null : mySqlDataReader.GetString(3);
                                address.jobDescription = mySqlDataReader.IsDBNull(4) ? null : mySqlDataReader.GetString(4);
                                address.jobStart = mySqlDataReader.IsDBNull(5) ? null : mySqlDataReader.GetString(5);
                                address.employeeUserName = mySqlDataReader.IsDBNull(6) ? null : mySqlDataReader.GetString(6);
                                address.companyName = mySqlDataReader.IsDBNull(7) ? null : mySqlDataReader.GetString(7);
                                address.contactMobileNumber = mySqlDataReader.IsDBNull(8) ? null : mySqlDataReader.GetString(8);
                                addressList.Add(address);
                            }
                        }
                    }
                    conn.Close();
                }
            }

            catch (Exception e)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }

            string googleAPIKey = "AIzaSyDt2sNgcWjSO3UkUo0I73BD6017Y2RCX9g";
            var coordinatesList = new List<CoordinatesModel>();

            foreach (GoogleHeatMapDataModel address in addressList)
            {
                var uri = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address.street + ",+" + address.postal+ ",+" + address.city + "&key=" + googleAPIKey;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        Object clientInfoObject = await response.Content.ReadAsAsync<Object>();
                        var obj = JObject.Parse(clientInfoObject.ToString());
                        string lat = obj.ToString().Substring(obj.ToString().IndexOf(@"""lat""") + 6, 9);
                        string lng = obj.ToString().Substring(obj.ToString().IndexOf(@"""lng""") + 7, 9);
                        if(!lat.Contains("result"))
                        {
                            CoordinatesModel coordinates = new CoordinatesModel();
                            coordinates.lat = lat;
                            coordinates.lng = lng;
                            coordinates.jobTitle = address.jobTitle;
                            coordinates.jobDescription = address.jobDescription;
                            coordinates.jobStart = address.jobStart;
                            coordinates.employeeUserName = address.employeeUserName;
                            coordinates.companyName = address.companyName;
                            coordinates.contactMobileNumber = address.contactMobileNumber;
                            coordinatesList.Add(coordinates);
                        }
                        
                    }
                }
            }
    

            return coordinatesList;
        }
    }
}

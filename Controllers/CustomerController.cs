using Local24API.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Local24API.Controllers
{
    [RoutePrefix("api/Customer")]
    public class CustomerController : ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        private string LOCAL24WriteConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Write_Connection"].ConnectionString;


        [HttpGet]
        [Authorize]
        [Route("Search")]
        [SwaggerOperation("Search")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public async Task<List<CustomerModel>> SearchCustomersAsync(string searchValue)
        {
            MySqlDataReader sqlDataReader;
            List<CustomerModel> customerList = new List<CustomerModel>();
            string connection;
            string contactPhoneNumber = "";
            string queryString = "";
            string rokeaCompany = "";

            //create querystrings based on param searchValue 
            if (searchValue.Length == 8 && int.TryParse(searchValue, out int n)) //querystring is 8 digit number -> search on phone numbers
                queryString = "select companyID, companyName, contactMobileNumber, contactPhoneNumber, jobAdress, jobPostal, jobCity, companyEmail, receiveSMS, cityID, companyType, companyInvoiceAdress, companyInvoicePostal, companyInvoiceCity, contactName, blacklist, orgNr from sh_customer " +
                                "where contactMobileNumber = '" + searchValue + "' || contactPhoneNumber = '" + searchValue + "' order by creationDate desc LIMIT 10";
            else //search on company names
                queryString = "select companyID, companyName, contactMobileNumber, contactPhoneNumber, jobAdress, jobPostal, jobCity, companyEmail, receiveSMS, cityID, companyType, companyInvoiceAdress, companyInvoicePostal, companyInvoiceCity, contactName, blacklist, orgNr from sh_customer " +
                            "where companyName LIKE '%" + searchValue + "%' order by creationDate desc LIMIT 10";

            try
            {
                using (var booking_connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    booking_connection.Open();

                    using (var cmd = new MySqlCommand(queryString, booking_connection))
                    {
                        sqlDataReader = cmd.ExecuteReader();
                        while (sqlDataReader.Read())
                        {
                            CustomerModel customer = new CustomerModel();

                            if (sqlDataReader.IsDBNull(3))
                                contactPhoneNumber = "";
                            else
                                contactPhoneNumber = sqlDataReader.GetString(3);

                            customer.customerId = sqlDataReader.GetInt32(0);
                            customer.companyName = sqlDataReader.GetString(1);
                            customer.contactMobileNumber = !sqlDataReader.IsDBNull(2) ? sqlDataReader.GetString(2) : "";
                            customer.contactPhoneNumber = contactPhoneNumber;
                            customer.jobAdress = !sqlDataReader.IsDBNull(4) ? sqlDataReader.GetString(4) : "";
                            customer.jobPostal = !sqlDataReader.IsDBNull(5) ? sqlDataReader.GetString(5) : "";
                            customer.jobCity = !sqlDataReader.IsDBNull(6) ? sqlDataReader.GetString(6) : "";
                            customer.companyEmail = !sqlDataReader.IsDBNull(7) ? sqlDataReader.GetString(7) : "";
                            customer.receiveSMS = sqlDataReader.GetInt16(8);
                            customer.rokeaCompany = "";
                            customer.newCustomer = 0;
                            customer.companyType = sqlDataReader.GetInt32(10);
                            customer.companyInvoiceAdress = !sqlDataReader.IsDBNull(11) ? sqlDataReader.GetString(11) : "";
                            customer.companyInvoicePostal = !sqlDataReader.IsDBNull(12) ? sqlDataReader.GetString(12) : "";
                            customer.companyInvoiceCity = !sqlDataReader.IsDBNull(13) ? sqlDataReader.GetString(13) : "";
                            customer.companyInvoiceContactName = !sqlDataReader.IsDBNull(14) ? sqlDataReader.GetString(14) : "";
                            customer.blacklist = sqlDataReader.GetInt32(15);
                            customer.companyOrgNr = !sqlDataReader.IsDBNull(16) ? sqlDataReader.GetString(16) : "";
                            customer.jobContactName = sqlDataReader.GetString(1);
                            customerList.Add(customer);
                        }
                    }
                }

                //search 1881 if searchValue is not found in db
                //search on PhoneNumber
                if (customerList.Count == 0 && searchValue.Length == 8 && int.TryParse(searchValue, out int o))
                {
                    var uri = "https://services.api1881.no/lookup/phonenumber/" + searchValue;

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://services.api1881.no/");
                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "b9d0a0a201c84ba49caf47f23dd81340");

                        // New code:
                        HttpResponseMessage response = await client.GetAsync("lookup/phonenumber/" + searchValue);
                        if (response.IsSuccessStatusCode)
                        {
                            CustomerModel customer = new CustomerModel();
                            Object clientInfoObject = await response.Content.ReadAsAsync<Object>();
                            try
                            {
                                var myJsonString = clientInfoObject.ToString();
                                var json = JObject.Parse(myJsonString);
                                var name = json["contacts"][0]["name"].ToString();
                                var mobile = json["contacts"][0]["contactPoints"][0]["value"].ToString();
                                var jobAddress = json["contacts"][0]["geography"]["address"]["street"].ToString();
                                var houseNumber = json["contacts"][0]["geography"]["address"]["houseNumber"].ToString();
                                var entrance = json["contacts"][0]["geography"]["address"]["entrance"].ToString();
                                var jobPostal = json["contacts"][0]["geography"]["address"]["postCode"].ToString();
                                var jobCity = json["contacts"][0]["geography"]["address"]["postArea"].ToString();

                                customer.customerId = 0;
                                customer.companyName = name;
                                customer.companyInvoiceContactName = "";
                                customer.contactMobileNumber = mobile;
                                customer.contactPhoneNumber = mobile;
                                customer.companyEmail = "";
                                customer.jobAdress = jobAddress + " " + houseNumber + entrance;
                                customer.jobPostal = jobPostal;
                                customer.receiveSMS = 1;
                                customer.jobCity = jobCity;
                                customer.newCustomer = 1;
                                customer.companyType = 0;
                                customer.companyOrgNr = "";
                                customer.companyInvoiceAdress = jobAddress + " " + houseNumber + entrance;
                                customer.companyInvoicePostal = jobPostal;
                                customer.companyInvoiceCity = jobCity;
                                customer.jobContactName = name;
                                customerList.Add(customer);
                            }
                            catch
                            {
                                customer.customerId = 0;
                                customer.companyName = "";
                                customer.companyInvoiceContactName = "";
                                customer.contactMobileNumber = "";
                                customer.contactPhoneNumber = "";
                                customer.companyEmail = "";
                                customer.jobAdress = "";
                                customer.jobPostal = "";
                                customer.receiveSMS = 0;
                                customer.jobCity = "";
                                customer.newCustomer = 1;
                                customer.companyType = 0;
                                customer.companyOrgNr = "";
                                customer.companyInvoiceAdress = "";
                                customer.companyInvoicePostal = "";
                                customer.companyInvoiceCity = "";
                                customer.jobContactName = "";
                                customerList.Add(customer);
                            }
                        }

                    }
                }
                //search on organization number
                if (customerList.Count == 0 && searchValue.Length == 9 && int.TryParse(searchValue, out int p))
                {
                    var uri = "https://services.api1881.no/lookup/organizationnumber/" + searchValue;

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://services.api1881.no/");
                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "b9d0a0a201c84ba49caf47f23dd81340");

                        HttpResponseMessage response = await client.GetAsync("lookup/organizationnumber/" + searchValue);
                        if (response.IsSuccessStatusCode)
                        {
                            CustomerModel customer = new CustomerModel();
                            Object clientInfoObject = await response.Content.ReadAsAsync<Object>();
                            var myJsonString = clientInfoObject.ToString();
                            var json = JObject.Parse(myJsonString);
                            var name = json["contacts"][0]["name"].ToString();
                            //var mobile = json["contacts"][0]["contactPoints"][0]["value"].ToString(); //not available from 1881 when searching org.nr.
                            var jobAddress = json["contacts"][0]["geography"]["address"]["street"].ToString();
                            var houseNumber = json["contacts"][0]["geography"]["address"]["houseNumber"].ToString();
                            var entrance = json["contacts"][0]["geography"]["address"]["entrance"].ToString();
                            var jobPostal = json["contacts"][0]["geography"]["address"]["postCode"].ToString();
                            var jobCity = json["contacts"][0]["geography"]["address"]["postArea"].ToString();

                            customer.customerId = 0;
                            customer.companyName = name;
                            customer.companyInvoiceContactName = "";
                            customer.contactMobileNumber = "";
                            customer.contactPhoneNumber = "";
                            customer.companyEmail = "";
                            customer.jobAdress = jobAddress + " " + houseNumber + entrance;
                            customer.jobPostal = jobPostal;
                            customer.receiveSMS = 1;
                            customer.jobCity = jobCity;
                            customer.newCustomer = 1;
                            customer.companyType = 2;
                            customer.companyInvoiceAdress = jobAddress + " " + houseNumber + entrance;
                            customer.companyInvoicePostal = jobPostal;
                            customer.companyInvoiceCity = jobCity;
                            customer.companyOrgNr = searchValue;
                            customer.jobContactName = name;
                            customerList.Add(customer);

                        }

                    }

                }

                return customerList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
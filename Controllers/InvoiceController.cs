using Local24API.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Local24API.Controllers
{
    [RoutePrefix("api/Invoice")]
    public class InvoiceController: ApiController
    {
        private string LOCAL24ReadConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Read_Connection"].ConnectionString;
        private string LOCAL24WriteConnString = System.Configuration.ConfigurationManager.ConnectionStrings["24LOCAL_Booking_Write_Connection"].ConnectionString;


        [Authorize]
        [HttpGet]
        [Route("Invoices")]
        [SwaggerOperation("Invoices")]
        public List<InvoiceModel> Invoices()
        {
            var principal = ClaimsPrincipal.Current;
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub");
            List<InvoiceModel> invoiceList = new List<InvoiceModel>();

            try
            {
                using (var connection = new MySqlConnection(LOCAL24ReadConnString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("SELECT f.fgrNr, e.mamutID as userID, f.dato, f.kundeNr, f.id, f.jobID, e.avdelingID, " +
                        "f.amountReceived, round((arbeidSum+materiellSum+fremmoteSum )*1.25,2) AS final_price, " +
                        "CONCAT_WS(' ', eierAdress, eierPostal, eierCity) as deliveryAdress, fakturaAdresse, fakturaPostal, fakturaCity, " +
                        "COUNT(p.fgrID) as totalProduct, steder1, desc1, steder2, desc2, steder3, desc3 " +
                        "FROM rokea_intranet.jf_fixel_fakturagrunnlag f  " +
                        "LEFT OUTER JOIN rokea_intranet.jf_fixel_fakturagrunnlag_products p ON f.id = p.fgrID, rokea_intranet.jf_fixel_employees e " +
                        "WHERE f.userID = e.userID AND f.statusMamut = 2 AND f.sluttfaktura = 0 AND f.prosjektNo = 0 AND f.kjappBeregningID = 0 " +
                        "AND f.groupID = 0 AND e.mamutID != 0 AND e.cityID = 6 AND f.processedMamut = 0 AND f.kundeNr not like 'tmp%' GROUP BY f.id, p.fgrID", connection))
                    {
                        connection.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            InvoiceModel invoice = new InvoiceModel();

                                                       
                            invoice.fgrNr = reader.GetInt32(0);
                            invoice.mamutUserID = reader.GetInt32(1);
                            invoice.dato = reader.GetDateTime(2);
                            invoice.kundeNr = reader.GetInt32(3);
                            invoice.fakturaId = reader.GetInt32(4);
                            invoice.jobId = reader.GetInt32(5);
                            invoice.avdelingID = reader.GetInt32(6);
                            invoice.amountReceived = reader.GetDecimal(7);
                            invoice.final_price = reader.GetDecimal(8);
                            invoice.deliveryAdress = reader.IsDBNull(9) ? null : reader.GetString(9);
                            invoice.fakturaAdresse = reader.IsDBNull(10) ? null : reader.GetString(10);
                            invoice.fakturaPostal = reader.GetInt32(11);
                            invoice.fakturaCity = reader.IsDBNull(12) ? null : reader.GetString(12);
                            invoice.totalProduct = reader.GetInt32(13);
                            invoice.steder1 = reader.IsDBNull(14) ? null : reader.GetString(14);
                            invoice.desc1 = reader.IsDBNull(15) ? null : reader.GetString(15);
                            invoice.steder2 = reader.IsDBNull(16) ? null : reader.GetString(16);
                            invoice.desc2 = reader.IsDBNull(17) ? null : reader.GetString(17);
                            invoice.steder3 = reader.IsDBNull(18) ? null : reader.GetString(18);
                            invoice.desc3 = reader.IsDBNull(19) ? null : reader.GetString(19);

                            invoiceList.Add(invoice);
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

            return invoiceList;
        }
    }
}
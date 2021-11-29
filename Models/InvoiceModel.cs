using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    public class InvoiceModel
    {
        public int fgrNr { get; set; }  
        public int mamutUserID { get; set; }
        public DateTime dato { get; set; }
        public int kundeNr { get; set; }
        public int fakturaId { get; set; }
        public int jobId { get; set; }
        public int avdelingID { get; set; }
        public decimal amountReceived { get; set; }
        public decimal final_price { get; set; }
        public string deliveryAdress { get; set; }
        public string fakturaAdresse { get; set; }
        public int fakturaPostal { get; set; }
        public string fakturaCity { get; set; }
        public int totalProduct { get; set; }
        public string steder1 { get; set; }
        public string desc1 { get; set; }
        public string steder2 { get; set; }
        public string desc2 { get; set; }
        public string steder3 { get; set; }
        public string desc3 { get; set; }
    }
}

 
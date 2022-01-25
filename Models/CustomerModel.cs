using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    public class CustomerModel
    {
        public int customerId { get; set; }
        public string companyName { get; set; }
        public string contactMobileNumber { get; set; }
        public string contactPhoneNumber { get; set; }
        public string jobContactName { get; set; }
        public string jobAdress { get; set; }
        public string jobPostal { get; set; }
        public string jobCity { get; set; }
        public string companyEmail { get; set; }
        public int receiveSMS { get; set; }
        public int newCustomer { get; set; }
        public string rokeaCompany { get; set; }
        public int companyType { get; set; }
        public string companyInvoiceAdress { get; set; }
        public string companyInvoicePostal { get; set; }
        public string companyInvoiceCity { get; set; }
        public string companyOrgNr { get; set; }
        public string companyInvoiceContactName { get; set; }
        public string creationDate { get; set; }
        public int blacklist { get; set; }

    }
}
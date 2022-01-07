using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    public class JobModel
    {
        public int jobId { get; set; }
        public string resourceId { get; set; }
        public string employeeUserName { get; set; }
        public int jobBookerID { get; set; }
        public string name { get; set; }
        public int jobStatus { get; set; }
        public string JobAdress { get; set; }
        public string jobPostal { get; set; }
        public string jobCity { get; set; }
        public string alternativeJobContactName { get; set; }
        public string alternativeJobContactMobileNumber { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string kundeNo { get; set; }
        public string reclamation { get; set; }
        public string iconCls { get; set; }
        public string eventColor { get; set; }
        public string fromJobID { get; set; }
        public string jobType { get; set; }
        public string resourceImage { get; set; }
        public string jobBookerName { get; set; }
        public string eventType { get; set; }
        public string genderType { get; set; }
        public string jobDescription { get; set; }
        public string ageType { get; set; }
        public int fgrStatus { get; set; }
        public int companyID { get; set; }
        public string orderNo { get; set; }
        public string fakturaVS { get; set; }
        public string buildYear { get; set; }
        public string marketingSource { get; set; }
        public string oppussingdate { get; set; }
        public string oppussingjob { get; set; }
        public string statusSikring { get; set; }
        public string infratekRapportNr { get; set; }
        public string maalerNr { get; set; }
        public string customerEmail { get; set; }
        public string companyInvoiceAdress { get; set; }
        public string companyInvoicePostal { get; set; }
        public string companyInvoiceCity { get; set; }
        public string companyName { get; set; }
        public string companyInvoiceContactName { get; set; }
        public string contactMobileNumber { get; set; }
        public int receiveSMS { get; set; }
    }

    public class GoogleHeatMapDataModel
    {
        public string street { get; set; }
        public string postal { get; set; }
        public string city { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public string jobStart { get; set; }
        public string employeeUserName { get; set; }
        public string companyName { get; set; }
        public string contactMobileNumber { get; set; }
    }

    public class CoordinatesModel
    {
        public string lat { get; set; }
        public string lng { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public string jobStart { get; set; }
        public string employeeUserName { get; set; }
        public string companyName { get; set; }
        public string contactMobileNumber { get; set; }
    }

    public class JobDocumentationModel
    {
        public string documentName { get; set; }
        public string documentURI { get; set; }
    }


}
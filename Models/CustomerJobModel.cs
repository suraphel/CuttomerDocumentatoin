using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// This is for the GetJob Controller 
// This is a class and a function that has the getter funution in it 
namespace Local24API.Models
{
        public class CustomerJobModel  // this is an obj that has  a getter and setter 
        {
                public string jobId { get; set; } // getter function that gives out a string as a return!
                //public string InvoiceId { get; set; }
                public string creationTime { get; set; }
                public string EmployeeId { get; set; }
                public bool SurveyCompleted { get; set; }

    }
}


/*
 


            //public string InvoiceId { get; set; }
            //public DateTime createdDate { get; set; }
            //public DateTime creationTime { get; set; }  
            //  public bool SurveyCompleted { get; set; }
            // another test data
            //public string companyName { get; set; }     
            //public string companyID { get; set;  }
            //public string contactMobileNumber {  get; set; }  
            //public string UnixHour { get; set; }
 */
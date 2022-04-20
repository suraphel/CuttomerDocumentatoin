using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    // Belongs to the jobId
    public class CustomerPortalModel
    {
        public string jobId { get; set; }
        public string EmployeeId { get; set; }
        public bool SurveyCompleted { get; set; }


    }
}


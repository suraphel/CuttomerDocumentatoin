using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    public class UserProfileModel
    {
        public int userID { get; set; }
        public string email { get; set; }
        public string employeeName { get; set; }
        public int companyID { get; set; }
    }
}
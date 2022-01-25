using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Local24API.Models
{
    public class UserModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int cityID { get; set; }
        public string companyName { get; set; }
        public string userID { get; internal set; }
        public string isAdmin { get; internal set; }
    }


    public class UserProfileModel
    {
        public int userID { get; set; }
        public int intranetUserID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string address { get; set; }
        public string postalCode { get; set; }
        public string postalCity { get; set; }
        public string email { get; set; }
        public string employeeName { get; set; }
        public string employeeUserName { get; set; }
        public int companyID { get; set; }
        public string phone { get; set; }
        public string id { get; set; }  //for use by Brynthum Scheduler
        public bool isActive { get; set; }
        public string companyName { get; set; }
    }

}
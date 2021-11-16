using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace Local24API.Models
{
    public class CompanyModel
    {
        public int companyID { get; set; }
        public string companyTitle { get; set; }
        public int companyType { get; set; }
        public CompanyRates companyRates { get; set; }
        public CompanyInfo companyInfo { get; set; }    

    }

    public class CompanyRates
    {
        [JsonProperty(PropertyName = "oppstart")]
        public int oppstart { get; set; }
        [JsonProperty("timepris")]
        public int timepris { get; set; }
    }

    public class CompanyInfo
    {
        public string name { get; set; }
        public string adresse { get; set; }
        public string tel { get; set; }
        public string website { get; set; }
        public string logo { get; set; }
        public string epost { get; set; }
        public string orgnr { get; set; }
        public string kontonr { get; set; }
        public string kundeUrl { get; set; }
    }
}
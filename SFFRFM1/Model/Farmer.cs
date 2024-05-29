using System;
using System.Collections.Generic;

namespace SFFRFM1.Model
{
    public class Farmer
    {
        public string Fullname { get; set; }
        public int FarmerID { get; set; }
        public string NationalID { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public int RegistrationYear { get; set; }
        public string Password { get; set; }
        public int Type { get; set; }
        public List<string> CouponCodes { get; set; }

        // Add more properties as needed
    }
}

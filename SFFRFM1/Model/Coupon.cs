using System;
using System.Collections.Generic;

namespace SFFRFM1.Model
{
    public class Coupon
    {
        public int DistributionID { get; set; }
        public string Region { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public int DistributeYear { get; set; }
        public List<string> CouponCodes { get; set; } // Changed type to List<string>
    }
}

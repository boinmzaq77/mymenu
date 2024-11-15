namespace MyMenuMerchant.Models
{
    public class LoginModel
    {
        public string MoblieNumber { get; set; }
        public string MerchantID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RefOTP { get; set; }

        public string OwnerID { get; set; }
        public string OTP { get; set; }

        public string UDID { get; set; }
        public string Type { get; set; } //InitialLogin , InitialCreate

    }

    public class SendOTP
    {
        public string OwnerID { get; set; }
        public string OTP { get; set; }
        public string UDID { get; set; }
        public int CloudProductID { get; set; }
    }

    public class VerifyOTP
    {
        public string OwnerID { get; set; }
        public string OTP { get; set; }
        public string RefOTP { get; set; }
        public int CloudProductID { get; set; }

    }

    public class MerchantLicenceModel
    {
        public int MerchantID { get; set; }
        public string UserName { get; set; }
        public string MerchantName { get; set; }
        public int TotalDaysGiven { get; set; }
        public string Ownerid { get; set; }
        public string ProductName { get; set; }

        public string FullName { get; set; }

        public string BonusCode { get; set; }
        public string PromotionCode { get; set; }

        public DateTime ExpiryDate { get; set; }
        public string Error { get; set; }
        public int TotalUser { get; set; }
        public int TotalBranch { get; set; }
        public int TotalDayRecieved { get; set; }
    }

}

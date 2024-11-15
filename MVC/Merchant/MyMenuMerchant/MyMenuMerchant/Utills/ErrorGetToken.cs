namespace MyMenuMerchant.Utills
{
    public class ErrorGetToken
    {
        public static string CheckErrorGetToken(string strex)
        {
            try
            {
                string TempStr = string.Empty;
                TempStr = strex;
                strex = strex.Trim().Replace("invalid_grant: ", "");
                strex = strex.Trim().Substring(0, 5);
                switch (strex)
                {
                    case "C0001":
                        return "UserPassIncorrect";
                    case "C0002":
                        return "CloudProductExpired";
                    case "C0003":
                        return "NotCloudProductLicence";
                    case "C0004":
                        return "NotAllowOwnerLoginWithPassword";
                    case "C0005":
                        return "UserCanNotAccessCloudProduct";
                    default:
                        return TempStr;
                }
            }
            catch (Exception)
            {
                return strex;
            }
        }
    }
}

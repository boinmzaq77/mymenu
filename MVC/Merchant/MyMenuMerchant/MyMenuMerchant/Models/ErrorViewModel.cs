namespace MyMenuMerchant.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ErrorgetTokenModel
    {
        public string error {  get; set; }
        public string error_description { get; set; }
    }
}
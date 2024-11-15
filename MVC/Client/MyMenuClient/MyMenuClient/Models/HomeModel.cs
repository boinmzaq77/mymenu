using MyMenu.JAM;
using MyMenu.ORM.Master;

namespace MyMenuClient.Models
{
    public class HomeIndexModel
    {
        public string MerchantName { get; set; }
        public int? FoodTableNo { get; set; }
        public string FoodTableName { get; set; }
        public string? OpenOrdersID { get; set; }
        public string MerchantSlogans { get; set; }
        public string MerchantLogo { get; set; }
        public string MerchantBackgroundPath { get; set; }

        public string OpeningHours { get; set; }
        public string MerchantAddress { get; set; }

        public decimal? CallStaff {  get; set; }

        public List<MenuCategoryModel> lstmenuCategory { get; set; }

        public HomeIndexModel()
        {
            lstmenuCategory = new List<MenuCategoryModel>();
        } 
    }

    public class MenuCategoryModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class SubListAddtoCartModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; } // ราคาปกติ
        public decimal SellingPrice { get; set; } //ตาม Branch ราคาขาย
        public int ItemQuantity { get; set; }
        public string Comment { get; set; }

        public  List<ItemOption> itemOption { get; set; }
        public  List<ItemExtra> itemExtras { get; set; }

        public SubListAddtoCartModel()
        {
            itemOption = new List<ItemOption>(); 
            itemExtras = new List<ItemExtra>();
        }


        public class ItemOption
        {
            public int OptionID { get; set; }
            public string OptionDetailName { get; set; }
            public int  OptionDetailNO { get; set; }
            public decimal Price { get; set; }
        }

        public class ItemExtra
        {
            public int ExtraID { get; set; }

            public string ExtraDetailName { get; set; }

            public int ExtraDetailNO { get; set; }
            public decimal Price { get; set; }
        }
    }
    public class ClinetOrderModel : Order
    {
        public List<ClientOrderDetailModel> OrderDetailModel { get; set; }
        public ClinetOrderModel()
        {
            OrderDetailModel = new List<ClientOrderDetailModel>();
        }
    }
    public class ClientOrderDetailModel : MyMenu.ORM.Master.OrderDetail
    {
        public string itemName { get; set; }

        public decimal NormalPrice { get; set; }

        public List<OrderDetailOptionExtra> OptionExtra { get; set; }
        public ClientOrderDetailModel()
        {
            OptionExtra = new List<OrderDetailOptionExtra>();
        }
    }


    public class EditItem
    {
        public int indexrow { get; set; }
        public GetItemInfoModel item { get; set; }
        public SubListAddtoCartModel chooseitem { get; set; }
    }

    public class data
    {
        public int quantity { get; set; }
    }

}

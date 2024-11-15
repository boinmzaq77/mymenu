using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using IdentityModel.Client;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class ManageMenuController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public ManageMenuController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }

        public async Task<IActionResult> Index()
        {
            ManageMenuModel manageMenuModel = new ManageMenuModel();
            try
            {
                manageMenuModel = await GetManageMenuAsync();
                return View(manageMenuModel);
            }
            catch (Exception ex)
            {
                return View(manageMenuModel);
            }
        }

        public IActionResult Category()
        {
            ViewBag.role = role;
            return View();
        }

        public IActionResult CategoryAdd()
        {
            return View();
        }

        public IActionResult CategoryEdit(string category)
        {
            ViewBag.role = role;
            if (string.IsNullOrEmpty(category))
            {
                return View();
            }
            else
            {
                CategoryModel categoryModel = JsonConvert.DeserializeObject<CategoryModel>(category);
                return View(categoryModel);
            }
        }

        public IActionResult Extra()
        {
            ViewBag.role = role;
            return View();
        }

        public IActionResult ExtraAdd()
        {
            return View();
        }

        public IActionResult ExtraEdit(string extra)
        {
            ViewBag.role = role;
            if (string.IsNullOrEmpty(extra))
            {
                return View();
            }
            else
            {
                ExtraModel extraModel = JsonConvert.DeserializeObject<ExtraModel>(extra);
                return View(extraModel);
            }
            return View();
        }
        public IActionResult Menu()
        {
            ViewBag.role = role;
            return View();
        }
        
        public async Task<IActionResult> MenuAdd()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Item/Config";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return View(JsonConvert.DeserializeObject<ItemConfig>(json_Catagory));
                        }
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<IActionResult> MenuEdit(string id)
        {
            ViewBag.role = role;
            ItemInfoModelEdit itemInfoModelEdit = new ItemInfoModelEdit();
            var Jwt = _cookieService.GetToken();
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "Item/ItemInfo?itemID=" + id;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        itemInfoModelEdit.ItemInfoModel = JsonConvert.DeserializeObject<ItemInfoModel>(json_Catagory);
                    }
                }
            }
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "Item/Config";
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,

                    DefaultValueHandling = DefaultValueHandling.Include

                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        itemInfoModelEdit.ItemConfig = JsonConvert.DeserializeObject<ItemConfig>(json_Catagory);
                       
                    }
                }
            }
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "Category/SubCategory?categoryID=" + itemInfoModelEdit.ItemInfoModel.CategoryID;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        itemInfoModelEdit.subCategoryModels = JsonConvert.DeserializeObject<List<SubCategoryModel>>(json_Catagory);
                    }
                }
            }
            return View(itemInfoModelEdit);
        }

        public IActionResult Option()
        {
            ViewBag.role = role;
            return View();
        }

        public IActionResult OptionAdd()
        {
            return View();
        }

        public IActionResult OptionEdit(string option)
        {
            ViewBag.role = role;
            if (string.IsNullOrEmpty(option))
            {
                return View();
            }
            else
            {
                OptionModel optionmodel = JsonConvert.DeserializeObject<OptionModel>(option);
                return View(optionmodel);
            }
            return View();
        }

        public IActionResult ImportMenu()
        {
            try
            {
                ViewBag.role = role;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> ItemAddSave([FromForm] JsonOfItem itemInfoModel)
        {
            try
            {
                
                var value = JsonConvert.DeserializeObject<ItemInfoModel>(itemInfoModel.value);
                if (value.SubCategoryID == 0) value.SubCategoryID = null;

                var formDataContent = new MultipartFormDataContent();
                if (itemInfoModel.FilePicture != null)
                {
                    var fileStreamContent = new StreamContent(itemInfoModel.FilePicture.OpenReadStream());
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FilePicture", FileName = itemInfoModel.FilePicture.FileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(fileStreamContent);
                }
                formDataContent.Add(new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"), "value");

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    //Client.SetBearerToken(Jwt);
                    //Client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationID);
                    string url = MicroServiceName.MyMenuAPI + "Item";

                    var result = await Client.PostAsync(url, formDataContent);

                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Ok();
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        [HttpPost]
        public async Task<ActionResult> ItemEditSave([FromForm] JsonOfItem itemInfoModel)
        {
            try
            {

                var value = JsonConvert.DeserializeObject<ItemInfoModel>(itemInfoModel.value);
                if (value.SubCategoryID == 0) value.SubCategoryID = null;
                               
                var formDataContent = new MultipartFormDataContent();
                if (itemInfoModel.FilePicture != null)
                {
                    var fileStreamContent = new StreamContent(itemInfoModel.FilePicture.OpenReadStream());
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FilePicture", FileName = itemInfoModel.FilePicture.FileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(fileStreamContent);
                }
                formDataContent.Add(new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"), "value");

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Item";

                    var result = await Client.PutAsync(url, formDataContent);

                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Ok();
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public async Task<ActionResult> CategoryAddSave(List<CategoryModel> categoryModels)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Category";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include
                    
                    };
                    var json = JsonConvert.SerializeObject(categoryModels, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                        //return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CategoryEditSave(CategoryModel categorymodel)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Category";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(categorymodel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }


        public async Task<ActionResult> OptionAddSave(List<OptionModel> optionModels)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Option";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(optionModels, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> OptionEditSave(OptionModel optionmodel)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Option";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(optionmodel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                    
                }
               
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<ActionResult> ExtraAddSave(List<ExtraModel> extraModels)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Extra";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(extraModels, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                    }

                }

                //List<CategoryModel> categoryModel = JsonConvert.DeserializeObject<List<CategoryModel>>(categoryModels);
                
                
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> ExtraEditSave(ExtraModel extramodel)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Extra";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(extramodel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    var contents = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(contents);
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<string> GetMenuCategory()
        {
            try
            {
                //var Cookies = new Cookies(HttpContext);
                //var Jwt = Cookies.GetToken();
                //if (Jwt == null)
                //{
                //    return null;
                //}
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Category";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw ex; 
            }
            
            //List<SubCategoryModel> subCategoryandCountModels = new List<SubCategoryModel>()
            //{
            //    new SubCategoryModel()
            //    {
            //        UserNameModified="UserNameModified",
            //        DateModified=new DateTime(),
            //        CategoryID=1,
            //        count=0,
            //        DateCreated=new DateTime(),
            //        SubCategoryID=0,
            //        SubCategoryName="SubCategoryName1"
            //    },
            //    new SubCategoryModel()
            //    {
            //        UserNameModified="UserNameModified",
            //        DateModified=new DateTime(),
            //        CategoryID=1,
            //        count=0,
            //        DateCreated=new DateTime(),
            //        SubCategoryID=0,
            //        SubCategoryName="SubCategoryName2"
            //    },
            //    new SubCategoryModel()
            //    {
            //        UserNameModified="UserNameModified",
            //        DateModified=new DateTime(),
            //        CategoryID=1,
            //        count=0,
            //        DateCreated=new DateTime(),
            //        SubCategoryID=0,
            //        SubCategoryName="SubCategoryName3"
            //    }
            //};



            //List<CategoryModel> manageMenuCategoryModels = new List<CategoryModel>();
            //CategoryModel model = new CategoryModel()
            //{
            //    CategoryID = 1,
            //    CategoryName = "Name",
            //    count = 1,
            //    DateCreated = DateTime.Now,
            //    DateModified = DateTime.Now,
            //    MerchantID = 1,
            //    UserNameModified = DateTime.Now.ToLongDateString(),
            //    Subcategorymodel = subCategoryandCountModels
            //};
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);

        }

        public async Task<string> GetMenuOption()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Option";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //List<OptionDetail> optionDetails = new List<OptionDetail>()
            //{
            //    new OptionDetail()
            //    {
            //        OptionDetailName = "Name1",
            //        OptionDetailNO = 1 ,
            //        OptionID = 1 ,
            //        Price = 100
            //    },
            //    new OptionDetail()
            //    {
            //        OptionDetailName = "Name2",
            //        OptionDetailNO = 2 ,
            //        OptionID = 2 ,
            //        Price = 200
            //    },
            //    new OptionDetail()
            //    {
            //        OptionDetailName = "Name3",
            //        OptionDetailNO = 3 ,
            //        OptionID = 3 ,
            //        Price = 300
            //    },
            //    new OptionDetail()
            //    {
            //        OptionDetailName = "Name4",
            //        OptionDetailNO = 4 ,
            //        OptionID = 4 ,
            //        Price = 400
            //    }
            //};



            //List<ManageMenuOptionModel> manageMenuCategoryModels = new List<ManageMenuOptionModel>();
            //ManageMenuOptionModel model = new ManageMenuOptionModel()
            //{

            //    DateCreated = DateTime.Now,
            //    DateModified = DateTime.Now,
            //    MerchantID = 1,
            //    UserNameModified = DateTime.Now.ToLongDateString(),
            //    optionDetails = optionDetails,
            //    OptionID = 1,
            //    OptionName = "name1"
            //};
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //manageMenuCategoryModels.Add(model);
            //return JsonConvert.SerializeObject(manageMenuCategoryModels);
        }

        public async Task<string> GetMenuExtra()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Extra";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //List<ExtraDetail> extraDetails = new List<ExtraDetail>()
            //{
            //    new ExtraDetail()
            //    {
            //        ExtraDetailName = "Name1",
            //        ExtraDetailNO = 1 ,
            //        ExtraID = 1 ,
            //        Price = 100
            //    },
            //    new ExtraDetail()
            //    {
            //        ExtraDetailName = "Name2",
            //        ExtraDetailNO = 2 ,
            //        ExtraID = 2 ,
            //        Price = 200
            //    },
            //    new ExtraDetail()
            //    {
            //        ExtraDetailName = "Name3",
            //        ExtraDetailNO = 3 ,
            //        ExtraID = 3 ,
            //        Price = 300
            //    },
            //    new ExtraDetail()
            //    {
            //        ExtraDetailName = "Name4",
            //        ExtraDetailNO = 4 ,
            //        ExtraID = 4 ,
            //        Price = 400
            //    }
            //};



            //List<ManageMenuExtraModel> manageMenuExtraModels = new List<ManageMenuExtraModel>();
            //ManageMenuExtraModel model = new ManageMenuExtraModel()
            //{

            //    DateCreated = DateTime.Now,
            //    DateModified = DateTime.Now,
            //    MerchantID = 1,
            //    UserNameModified = DateTime.Now.ToLongDateString(),
            //    extraDetails = extraDetails,
            //    ExtraID = 1,
            //    ExtraName = "name1"
            //};
            //manageMenuExtraModels.Add(model);
            //manageMenuExtraModels.Add(model);
            //manageMenuExtraModels.Add(model);
            //manageMenuExtraModels.Add(model);
            //manageMenuExtraModels.Add(model);
            //return JsonConvert.SerializeObject(manageMenuExtraModels);
        }

        public async Task<string> GetMenuItem()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Item";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public async Task<string> GetMenuItemoffset(string offset)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Item?offset="+offset;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost]
        public async Task<List<SubCategoryModel>>  GetSubCategory(string id)
        {
            var Jwt = _cookieService.GetToken();
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "Category/SubCategory?categoryID="+id;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        return JsonConvert.DeserializeObject<List<SubCategoryModel>>( json_Catagory);
                    }
                }
            }
            return null;
        }
        
        public async Task<ManageMenuModel> GetManageMenuAsync()
        {
            var branchid = "1";
            var Jwt = _cookieService.GetToken();
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "ManageMenu";

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        return JsonConvert.DeserializeObject<ManageMenuModel>(json_Catagory);
                    }
                }
            }
            return null;
        }

        [HttpPost]
        public async Task<ItemInfoModel> GetDtailItem(string id,bool onbranch)
        {
            var branchID = _cookieService.GetBranch();
            var Jwt = _cookieService.GetToken();
            using (var Client = new HttpClient())
            {
                Client.SetBearerToken(Jwt);
                string url = MicroServiceName.MyMenuAPI + "Item/ItemInfo?itemID=" + id + (onbranch == true ? "&BranchID=" + branchID : "");

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
                var result = await Client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json_Catagory = await result.Content.ReadAsStringAsync();
                    if (json_Catagory != null)
                    {
                        return JsonConvert.DeserializeObject<ItemInfoModel>(json_Catagory);
                    }
                }
            }
            return null;
        }
        [HttpPost]
        public async Task<ActionResult> DeleteCategory(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var BranchID = "1";
                    string url = MicroServiceName.MyMenuAPI + "Category?categoryID=" + id ;

                    var result = await Client.DeleteAsync(url);

                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }


            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteExtra(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var BranchID = "1";
                    string url = MicroServiceName.MyMenuAPI + "Extra"+"?extraID=" + id;

                    var result = await Client.DeleteAsync(url);

                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }


            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteOption(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var BranchID = "1";
                    string url = MicroServiceName.MyMenuAPI + "Option" + "?optionID=" + id;

                    var result = await Client.DeleteAsync(url);

                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<ActionResult> DeleteItem(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var BranchID = "1";
                    string url = MicroServiceName.MyMenuAPI + "Item" + "?itemID=" + id;

                    var result = await Client.DeleteAsync(url);

                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public static string ConvertToCurrency(decimal txt)
        {
            decimal decimalValue = txt;
            decimalValue = Math.Round(decimalValue, 2, MidpointRounding.AwayFromZero);
            var x = decimalValue.ToString("#,###0.#0");
            return x;
        }

        public async Task<string> GetMenusImportConfig()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "item/ImportConfig";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> ItemsImport(List<ItemsImportModel> itemModels)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    var nullCategoryItems = itemModels.Where(x => x.CategoryName == null).ToList();
                    nullCategoryItems.ForEach(x => x.CategoryName = "");

                    var nullsubCategoryItems = itemModels.Where(x => x.SubCategoryName == null).ToList();
                    nullsubCategoryItems.ForEach(x => x.SubCategoryName = "");

                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Item/ImportItems";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(itemModels, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    string content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(content);
                        }
                    }else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}
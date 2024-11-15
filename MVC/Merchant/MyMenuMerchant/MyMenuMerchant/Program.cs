using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MyMenuMerchant;
using MyMenuMerchant.Utills;
using System.Configuration;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
bool forceEngCluture = false;

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.Configure<MicroServiceNameModel>(Configuration.GetSection("MicroServiceHostName"));
BlueidConnect.SeAuth2AuthorityHost = builder.Configuration["MicroServiceHostName:SeAuth2AuthorityHost"];
builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ICookieManager, ChunkingCookieManager>();
builder.Services.AddScoped<ICookieService, Cookies>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc(option => option.EnableEndpointRouting = false)
              .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
              .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("th-TH")
    };
    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
    {
        if (forceEngCluture)
        {
            forceEngCluture = false;
            return Task.FromResult(new ProviderCultureResult("en-US", "en-US"));
        }

        var myLanguage = context.Request.Cookies["MyMenuLanguage"];
        if (myLanguage == null)
        {
            return Task.FromResult(new ProviderCultureResult("th-TH", "th-TH"));
        }


        if (myLanguage == "Th")
        {
            return Task.FromResult(new ProviderCultureResult("th-TH", "th-TH"));
        }
        else
        {

            return Task.FromResult(new ProviderCultureResult("en-US", "en-US"));
        }
    }));
});


var app = builder.Build();

var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UsePathBase("/mymenumerchant"); // DON'T FORGET THE LEADING SLASH!

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Login}/{action=Index}/{id?}");
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

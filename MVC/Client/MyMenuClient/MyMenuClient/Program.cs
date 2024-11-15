using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using MyMenuClient;
using MyMenuClient.Utills;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
bool forceEngCluture = false;


// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
BlueidConnect.SeAuth2AuthorityHost = builder.Configuration["MicroServiceHostName:SeAuth2AuthorityHost"];
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICookieService, Cookies>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc(option => option.EnableEndpointRouting = false)
              .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
              .AddDataAnnotationsLocalization();
builder.Services.Configure<MicroServiceHostName>(builder.Configuration.GetSection("MicroServiceHostName"));

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

        var myLanguage = context.Request.Cookies["MyMenuClientLanguage"];
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
app.UsePathBase("/mymenuclient"); // DON'T FORGET THE LEADING SLASH!

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

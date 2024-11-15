using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using MyMenuClient.Utills;

namespace MyMenuClient
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class JWTAttribute : ActionFilterAttribute
    {
        //public override void OnActionExecuted(ActionExecutedContext context)
        //{

        //}

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (!context.HttpContext.Request.Cookies.ContainsKey("access_token"))
                {
                    var refresh_token = context.HttpContext.Request.Cookies["refresh_token"];

                    if (string.IsNullOrEmpty(refresh_token))
                    {
                        context.Result = new RedirectToActionResult("Store", "Home", null);
                    }
                    else
                    {
                        var jwt = BlueidConnect.Refresh_MyMenuClient(refresh_token);
                        context.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                        context.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                        var controllerName = context.Controller.GetType().Name;
                        controllerName = controllerName.Remove(controllerName.Length - "Controller".Length);
                        context.Result = new RedirectToActionResult(context.RouteData.Values["action"].ToString(), controllerName, context.ActionDescriptor.RouteValues);
                    }
                }
                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                context.Result = new RedirectToActionResult("Store", "Home", new RouteValueDictionary(new { UserMessage = ex.Message }));
                base.OnActionExecuting(context);
            }

        }
    }
}

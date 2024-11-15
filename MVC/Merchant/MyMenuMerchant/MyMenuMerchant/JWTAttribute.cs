namespace MyMenuMerchant
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using MyMenuMerchant.Utills;

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
                        context.Result = new RedirectToActionResult("Index", "Login", null);
                    }
                    else
                    {
                        var type_token = context.HttpContext.Request.Cookies["type_token"];
                        if (string.IsNullOrEmpty(type_token))
                        {
                            context.Result = new RedirectToActionResult("Index", "Login", null);
                        }
                        else if (type_token == "code")
                        {
                            var jwt = BlueidConnect.Refresh_MyMenuDC(refresh_token);
                            context.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                            context.HttpContext.Response.Cookies.Append("type_token", "code");
                            context.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                            var controllerName = context.Controller.GetType().Name;
                            controllerName = controllerName.Remove(controllerName.Length - "Controller".Length);
                            context.Result = new RedirectToActionResult(context.RouteData.Values["action"].ToString(), controllerName, context.ActionDescriptor.RouteValues);
                        }
                        else if (type_token == "password")
                        {
                            var jwt = BlueidConnect.Refresh_MyMenuApp(refresh_token);
                            context.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                            context.HttpContext.Response.Cookies.Append("type_token", "password");
                            context.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                            var controllerName = context.Controller.GetType().Name;
                            controllerName = controllerName.Remove(controllerName.Length - "Controller".Length);
                            context.Result = new RedirectToActionResult(context.RouteData.Values["action"].ToString(), controllerName, context.ActionDescriptor.RouteValues);
                        }
                        else
                        {
                            context.Result = new RedirectToActionResult("Index", "Login", null);
                        }
                    }
                }
                base.OnActionExecuting(context);
            }
            catch (Exception ex) 
            {
                context.Result = new RedirectToActionResult("Index", "Login", new RouteValueDictionary(new { UserMessage = ex.Message }));
                base.OnActionExecuting(context);
            }

        }
    }
}

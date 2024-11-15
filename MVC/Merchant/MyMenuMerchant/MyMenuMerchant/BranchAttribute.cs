using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyMenuMerchant.Utills;

namespace MyMenuMerchant
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class BranchAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (!context.HttpContext.Request.Cookies.ContainsKey("branch"))
                {
                    context.Result = new RedirectToActionResult("LoginBranchs", "Login", null);
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

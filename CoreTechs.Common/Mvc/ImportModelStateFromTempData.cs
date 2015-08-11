using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CoreTechs.Common.Mvc
{
    /*
     * Credit to Kazi Manzur Rashid for his ModelState transfer filters
     * http://weblogs.asp.net/rashid/asp-net-mvc-best-practices-part-1
     */
    public class ImportModelStateFromTempData : ModelStateTempDataTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ModelStateDictionary modelState = filterContext.Controller.TempData[Key] as ModelStateDictionary;

            if (modelState != null)
            {
                //Only Import if we are viewing
                if (filterContext.Result is ViewResult)
                {
                    filterContext.Controller.ViewData.ModelState.Merge(modelState);
                }
                else
                {
                    //Otherwise remove it.
                    filterContext.Controller.TempData.Remove(Key);
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }
}

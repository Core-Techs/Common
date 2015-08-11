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
    public abstract class ModelStateTempDataTransfer : ActionFilterAttribute
    {
        protected static readonly string Key = typeof(ModelStateTempDataTransfer).FullName;
    }
}

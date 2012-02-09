using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using jqGridExample.Controllers;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace jqGridExample.Helpers.Extensions
{
    public static class GridExtensions
    {
        public static void RenderGrid(this HtmlHelper htmlHelper, string name, IDictionary<string, object> parameters = null, IDictionary<string, string> actions = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            RouteValueDictionary values = new RouteValueDictionary();
            values.Add("controller", "GridExecution");
            values.Add("action", "GridSetup");
            values.Add("name", name);
            values.Add("parameters", parameters);
            values.Add("actions", actions);

            htmlHelper.RenderAction("GridSetup", values);
        }
    }
}
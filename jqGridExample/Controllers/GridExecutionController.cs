using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jqGridExample.Models;
using System.Data;
using System.Text;
using jqGridExample.Helpers;

namespace jqGridExample.Controllers
{
    public class GridExecutionController : Controller
    {
        public ActionResult SyncGrids()
        {
            GridManager gridManager = new GridManager();

            try
            {
                gridManager.SyncGridDefinitions(Server.MapPath("~/App_Data"));
                @ViewBag.Message = "Successfully updated grid definitions";
            }
            catch
            {
                @ViewBag.Message = "Failed to update grid definitions";
            }
            return View();
        }
        [ChildActionOnly]
        public ActionResult GridSetup(string name, IDictionary<string, object> parameters, IDictionary<string, string> actions)
        {
            GridManager gridManager = new GridManager();
            string appDataPath = Server.MapPath("~/App_Data");
            Grid model = gridManager.GetGridDefinition(name, appDataPath);

            //Add parameters to the model
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (parameters[key] != null)
                    {
                        GridParameter parameter = model.Parameters.Where(p => p.Name == key).FirstOrDefault();
                        if (parameter != default(GridParameter))
                        {
                            parameter.DefaultValue = parameters[key].ToString();
                        }
                    }
                }
            }

            //Add actions to the model
            if (actions != null)
            {
                foreach (string key in actions.Keys)
                {
                    if (actions[key] != null)
                    {
                        model.Actions.Add(new GridAction(key, actions[key]));
                    }
                }
            }

            return PartialView("Grid", model);
        }
        public ActionResult GridData(string sidx, string sord, int page, int rows)
        {
            //parameters added by jqGrid that we want to ignore
            string[] parametersToIgnore = { "sidx", "sord", "page", "rows", "_search", "GridName", "nd" };
            string gridName = this.Request.QueryString.Get("GridName");

            //populate a list of stored procedure parameters
            IDictionary<string,object> parameters = new Dictionary<string, object>();
            foreach (string key in this.Request.QueryString.Keys)
            {
                if (!parametersToIgnore.Contains(key) && !string.IsNullOrEmpty(key))
                {
                    parameters.Add(key, this.Request.QueryString[key]);
                }
            }

            //run the stored procedure and return the data table
            DataTable table = new DataTable();
            using (jqGridExampleDbContext context = new jqGridExampleDbContext())
            {
                table = context.GetDataTable(gridName, parameters);
            }

            //Grab the page, page size
            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;
            if (pageSize > table.Rows.Count)
                pageIndex = 0;

            int totalRecords = table.Rows.Count;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            //Only sort, skip and take if we have row counts
            if (table.Rows.Count > 0)
            {
                string sort = string.Empty;
                if (sidx.Trim().EndsWith(","))
                {
                    //sidx looks like "GroupingField asc, "
                    sort = sidx.Trim().TrimEnd(',');
                }
                else if (!string.IsNullOrEmpty(sidx))
                {
                    sort += sidx + " " + sord;
                }

                table.DefaultView.Sort = sort;
                table = table.DefaultView.ToTable();
                table.AcceptChanges();
                table = table.AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize).CopyToDataTable();
            }
            table.TableName = gridName;

            GridManager gridManager = new GridManager();
            string appDataPath = Server.MapPath("~/App_Data");
            Grid model = gridManager.GetGridDefinition(gridName, appDataPath);

            //create the json for the rows of data
            StringBuilder jsonRows = new StringBuilder();
            jsonRows.Append("\"rows\" : [ ");

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow dr = table.Rows[i];
                if (i > 0)
                    jsonRows.Append(",");
                jsonRows.AppendFormat("{{\"id\":\"{0}\",\"cell\":[", i.ToString());
                bool firstColumn = true;
                foreach (GridColumn col in model.Columns)
                {
                    if (!firstColumn)
                        jsonRows.Append(",");
                    if (col.DataType == typeof(DateTime).ToString())
                        jsonRows.AppendFormat("\"{0}\"", DateTime.Parse(dr[col.Name].ToString()).ToString("MM/dd/yyyy hh:mm tt"));
                    else
                        jsonRows.AppendFormat("\"{0}\"", dr[col.Name]);
                    firstColumn = false;
                }
                jsonRows.Append("]}");
            }
            jsonRows.Append("]");

            //create the returned json structure
            StringBuilder jsonData = new StringBuilder();
            jsonData.AppendFormat("{{ \"page\":\"{0}\", \"total\":\"{1}\", \"records\":\"{2}\", {3} }}", pageIndex + 1, totalPages, totalRecords, jsonRows.ToString());

            return Content(jsonData.ToString(), "application/json");
        }
    }
}

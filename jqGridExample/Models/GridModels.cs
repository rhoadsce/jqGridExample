using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace jqGridExample.Models
{
    public class Grid
    {
        public Grid()
        {
            this.Columns = new List<GridColumn>();
            this.Parameters = new List<GridParameter>();
            this.Actions = new List<GridAction>();
        }

        [Key]
        public int GridId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string StoredProcedure { get; set; }
        public List<GridColumn> Columns { get; set; }
        public List<GridParameter> Parameters { get; set; }
        public List<GridAction> Actions { get; set; }
    }
    public class GridColumn
    {
        [Key]
        public int GridColumnId { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string DataType { get; set; }
        public bool IsHidden { get; set; }
    }
    public class GridParameter
    {
        public GridParameter()
        {
        }
        public GridParameter(string name, string value)
        {
            this.Name = name;
            this.DefaultValue = value;
        }

        public int GridParameterId { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsHidden { get; set; }
    }
    public class GridAction
    {
        public GridAction()
        {
        }
        public GridAction(string caption, string link)
        {
            this.Caption = caption;
            this.Link = link;
        }
        public string Caption { get; set; }
        public string Link { get; set; }
    }
}
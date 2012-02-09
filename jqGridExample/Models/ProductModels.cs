using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jqGridExample.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public decimal Price { get; set; }

        public virtual Category Category { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
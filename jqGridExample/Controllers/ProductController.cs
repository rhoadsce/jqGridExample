using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jqGridExample.Models;

namespace jqGridExample.Controllers
{ 
    public class ProductController : Controller
    {
        private jqGridExampleDbContext db = new jqGridExampleDbContext();

        public ViewResult AllProducts()
        {
            return View();
        }

        public ViewResult Index(int categoryId)
        {
            Category model = db.Categories.Where(c => c.CategoryId == categoryId).FirstOrDefault();
            return View(model);
        }
        public ViewResult Details(int id)
        {
            Product product = db.Products.Find(id);
            return View(product);
        }
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        } 
        [HttpPost]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId = product.CategoryId });  
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        [HttpPost]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { categoryId = product.CategoryId });
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index", new { categoryId = product.CategoryId });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
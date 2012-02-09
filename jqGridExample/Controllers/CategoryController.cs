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
    public class CategoryController : Controller
    {
        private jqGridExampleDbContext db = new jqGridExampleDbContext();

        public ViewResult Index()
        {
            return View();
        }
        public ViewResult Details(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }
        public ActionResult Create()
        {
            return View();
        } 
        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(category);
        }
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }
        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
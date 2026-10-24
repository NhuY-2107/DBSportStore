using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ProductsController : Controller
    {
        private DBSportStoreEntities db = new DBSportStoreEntities();
        // Hàm dùng chung để lưu file ảnh vào Content/images
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            // Không có file ảnh
            if (imageFile == null || imageFile.ContentLength == 0)
            {
                return null;
            }

            // Kiểm tra dung lượng tối đa 2 MB
            if (imageFile.ContentLength > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Dung lượng ảnh không được vượt quá 2 MB."
                );
                return null;
            }

            // Lấy tên file và phần mở rộng
            string originalFileName = Path.GetFileName(imageFile.FileName);
            string extension = Path.GetExtension(originalFileName).ToLower();

            // Chỉ cho phép JPG, JPEG và PNG
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Chỉ chấp nhận ảnh JPG, JPEG hoặc PNG."
                );
                return null;
            }

            // Tạo đường dẫn thư mục chứa ảnh
            string folder = Server.MapPath("~/Content/images");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Tạo tên file
            string fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(originalFileName);

            string fileName = fileNameWithoutExtension + extension;
            string path = Path.Combine(folder, fileName);

            // Tránh trùng tên file
            int count = 1;

            while (System.IO.File.Exists(path))
            {
                fileName = fileNameWithoutExtension + "-" + count + extension;
                path = Path.Combine(folder, fileName);
                count++;
            }

            // Lưu ảnh
            imageFile.SaveAs(path);

            // Trả về tên ảnh để lưu vào database
            return fileName;
        }

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,ImageUrl,StockQuantity,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
     [Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,ImageUrl,StockQuantity,IsActive")] Product product,
     HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin sản phẩm hiện có trong database
                var existingProduct = db.Products.Find(product.ProductID);

                if (existingProduct == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin sản phẩm
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.CategoryID = product.CategoryID;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.IsActive = product.IsActive;

                // Chỉ đổi ảnh nếu người dùng chọn ảnh mới
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    existingProduct.ImageUrl = SaveImage(imageFile);
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }
        

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

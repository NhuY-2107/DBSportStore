using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class StoreController : Controller
    {
        private DBSportStoreEntities db = new DBSportStoreEntities();

        public ActionResult Index(
       int? categoryId,
       string keyword,
       decimal? minPrice,
       decimal? maxPrice,
       string sortOrder,
       int page = 1)
        {
            const int pageSize = 6;

            if (page < 1)
            {
                page = 1;
            }

            var products = db.Products
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }
            //Tìm kiếm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                products = products.Where(
                    p => p.ProductName.Contains(keyword)
                      || p.Description.Contains(keyword));
            }
            // Lọc giá
            if (minPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price <= maxPrice.Value);
            }

            // Đếm tổng số sản phẩm sau khi lọc
            int totalItems = products.Count();

            int totalPages =
                (int)Math.Ceiling((double)totalItems / pageSize);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            // Xử lý sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderByDescending(p => p.ProductID); // Mặc định
                    break;
            }

            var model = new StoreHomeViewModel
            {
                Categories = db.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList(),

                // ⭐ ĐÃ XÓA BỎ CHỮ OrderByDescending(p => p.ProductID) THỪA THÃI Ở ĐÂY 
                // ĐỂ GIỮ NGUYÊN THỨ TỰ SẮP XẾP THEO GIÁ RỒI MỚI SKIP/TAKE:
                Products = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductCardViewModel
                    {
                        ProductID = p.ProductID,
                        NamePro = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Price = p.Price,
                        ImagePro = p.ImageUrl
                    })
                    .ToList(),

                SelectedCategoryId = categoryId,
                SearchTerm = keyword,
                MinPrice = minPrice,
                MaxPrice = maxPrice,

                TotalItems = totalItems,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }
    }
}
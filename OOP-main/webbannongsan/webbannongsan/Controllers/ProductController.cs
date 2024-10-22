using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using webbannongsan.Models;

namespace webbannongsan.Controllers
{
    public class ProductController : Controller
    {
        private readonly DB_TadEntities _context;

        public ProductController()
        {
            _context = new DB_TadEntities(); // Khởi tạo DB_TadEntities
        }

        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Product(string priceRange, string category, int?[] discount)
        {
            // Lấy danh sách tất cả sản phẩm từ cơ sở dữ liệu
            List<Product> products = _context.Products.ToList();
            ViewBag.categories = _context.Categories.ToList(); // Lấy danh sách danh mục để hiển thị trên view
            ViewBag.priceRange = priceRange;
            ViewBag.category = category;

            // Kiểm tra khoảng giá và lọc sản phẩm
            if (!string.IsNullOrEmpty(priceRange) && priceRange != "all")
            {
                string[] priceRanges = priceRange.Split('-'); // Chia khoảng giá thành các giá trị min và max
                int minPrice = int.Parse(priceRanges[0]); // Giá tối thiểu
                int maxPrice = int.Parse(priceRanges[1]); // Giá tối đa
                products = products.Where(i => i.Price >= minPrice && i.Price <= maxPrice).ToList(); // Lọc sản phẩm theo khoảng giá
            }

            // Kiểm tra danh mục và lọc sản phẩm
            if (!string.IsNullOrEmpty(category) && category != "all")
            {
                products = products.Where(i => i.Category.Name == category).ToList(); // Lọc sản phẩm theo danh mục
            }

            // Kiểm tra discount và lọc sản phẩm
            if (discount != null && discount.Length != 0)
            {
                // Lọc sản phẩm theo CouponID
                foreach (var item in discount)
                {
                    products = products.Where(i => i.CouponID == item).ToList();
                }
            }

            // Tạo danh sách giá đã giảm
            List<decimal> discountedPrices = new List<decimal>();

            // Cập nhật giá đã giảm cho từng sản phẩm
            foreach (var product in products)
            {
                // Lấy giá trị giảm giá cho sản phẩm hiện tại
                var discountValue = GetDiscountForProduct(product.ProductID);
                // Tính giá đã giảm
                var discountedPrice = product.Price * (1 - discountValue);
                discountedPrices.Add(discountedPrice); // Thêm giá đã giảm vào danh sách
                //product.DiscountedPrice = discountedPrice; // Cập nhật giá đã giảm trong model
            }

            // Lưu giá đã giảm vào ViewBag
            ViewBag.DiscountedPrices = discountedPrices;

            // Trả về view với danh sách sản phẩm đã có giá đã giảm
            return View(products);
        }

        public ActionResult DetailProduct(int? detailPrID)
        {
            if (detailPrID == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy sản phẩm
            }

            Product product = _context.Products.FirstOrDefault(i => i.ProductID == detailPrID);
            if (product == null)
            {
                return HttpNotFound(); // Trả về lỗi 404 nếu không tìm thấy sản phẩm
            }

            return View(product);
        }

        public decimal GetDiscountForProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId); // Lấy sản phẩm
            if (product == null || product.CouponID == null) return 0; // Nếu không tìm thấy hoặc không có coupon

            var coupon = _context.Coupons.FirstOrDefault(c => c.CouponID == product.CouponID); // Lấy coupon dựa trên CouponID
            return coupon != null ? (decimal)coupon.Discount : 0; // Trả về discount
        }
    }
}

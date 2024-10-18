using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webbannongsan.Models;

namespace webbannongsan.Controllers
{
    public class CartController : Controller
    {
        DB_TadEntities DB = new DB_TadEntities();

        public ActionResult CartIndex()
        {
            var Ac = (Account)Session["Account"];
            if (Ac == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<int> cartQuantity = new List<int>();
            var AcID = Ac.AccountID;
            var carts = DB.Carts.Where(i => i.AccountID == AcID).ToList();
            List<Product> products = new List<Product>();
            float sumPrice = 0;

            foreach (var cart in carts)
            {
                Product product = DB.Products.Where(i => i.ProductID == cart.ProductID).FirstOrDefault();
                if (product != null)
                {
                    products.Add(product);
                    cartQuantity.Add(cart.Quantity);

                    // Tính giá sau khi giảm giá
                    var discountValue = GetDiscountForProduct(product.ProductID);
                    var finalPrice = product.Price * (1 - discountValue);

                    sumPrice += (float)(finalPrice * cart.Quantity);
                }
            }

            ViewBag.sumPrice = sumPrice;
            ViewBag.CartQuantity = cartQuantity;
            return View(products);
        }

        [HttpPost]
        public ActionResult CartIndex(int ProductID)
        {
            var Ac = (Account)Session["Account"];
            if (Ac == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartTemp = DB.Carts.FirstOrDefault(i => i.ProductID == ProductID && i.AccountID == Ac.AccountID);
            if (cartTemp == null)
            {
                Cart newCart = new Cart
                {
                    ProductID = ProductID,
                    AccountID = Ac.AccountID,
                    Quantity = 1
                };

                DB.Carts.Add(newCart);
                DB.SaveChanges();
            }

            return RedirectToAction("CartIndex");
        }

        // Phương thức lấy giá trị giảm giá
        public decimal GetDiscountForProduct(int productId)
        {
            var product = DB.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null || product.CouponID == null) return 0;

            var coupon = DB.Coupons.FirstOrDefault(c => c.CouponID == product.CouponID);
            return coupon != null ? (decimal)coupon.Discount : 0;
        }
        [HttpPost]
        public ActionResult UpdateCart(int[] selectedProducts, int[] quantity)
        {
            var Ac = (Account)Session["Account"];
            if (Ac == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var AcID = Ac.AccountID;
            var carts = DB.Carts.Where(i => i.AccountID == AcID).ToList();
            float sumPrice = 0;

            List<Coupon> Coupon= DB.Coupons.Where(i=>i.AccountID== Ac.AccountID).ToList();
            ViewBag.Coupon = Coupon;
            

            // Danh sách sản phẩm đã chọn
            List<Product> selectedItems = new List<Product>();
            List<int> selectedQuantities = new List<int>();

            for (int i = 0; i < carts.Count; i++)
            {
                var cart = carts[i];
                if (selectedProducts != null && selectedProducts.Contains(cart.ProductID))
                {
                    Product product = DB.Products.FirstOrDefault(p => p.ProductID == cart.ProductID);
                    if (product != null)
                    {
                        var discountValue = GetDiscountForProduct(product.ProductID);
                        var finalPrice = product.Price * (1 - discountValue);
                        sumPrice += (float)(finalPrice * quantity[i]);

                        // Lưu sản phẩm và số lượng vào danh sách
                        selectedItems.Add(product);
                        selectedQuantities.Add(quantity[i]);
                    }
                }
            }

            var discounts = DB.Coupons.Select(c => c.Discount).ToList();
            // Lưu danh sách sản phẩm và số lượng vào TempData
            ViewBag.SelectedItems = selectedItems;
            ViewBag.SelectedQuantities = selectedQuantities;
            ViewBag.TotalPrice = sumPrice; // Cập nhật tổng giá
            ViewBag.SumPrice = sumPrice; // Lưu vào ViewBag để truyền sang View










            return View(); // Chuyển đến trang xác nhận đơn hàng

        }




    }
}

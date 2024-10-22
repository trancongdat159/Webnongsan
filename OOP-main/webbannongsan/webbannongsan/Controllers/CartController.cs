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
                Product product = DB.Products.FirstOrDefault(i => i.ProductID == cart.ProductID);
                if (product != null)
                {
                    products.Add(product);
                    cartQuantity.Add(cart.Quantity);

                    var discountValue = GetDiscountForProduct(product.ProductID);
                    var finalPrice = product.Price * (1 - discountValue);
                    sumPrice += (float)(finalPrice * cart.Quantity);
                }
            }

            ViewBag.sumPrice = sumPrice;
            ViewBag.CartQuantity = cartQuantity;
            return View(products);
        }

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

            List<Coupon> Coupon = DB.Coupons.Where(i => i.AccountID == Ac.AccountID).ToList();
            ViewBag.Coupon = Coupon;

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

                        selectedItems.Add(product);
                        selectedQuantities.Add(quantity[i]);
                    }
                }
            }

            ViewBag.SelectedItems = selectedItems;
            ViewBag.SelectedQuantities = selectedQuantities;
            ViewBag.TotalPrice = sumPrice;
            ViewBag.SumPrice = sumPrice;

            return View(); // Chuyển đến trang xác nhận đơn hàng
        }

        [HttpPost]
       
        public ActionResult ConfirmOrder(string deliveryAddress, float totalPrice)
        {
            var Ac = (Account)Session["Account"];

            //// Kiểm tra xem Ac có khác null không
            //if (Ac == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            //// Kiểm tra xem AccountID có hợp lệ không
            //if (Ac.AccountID <= 0)
            //{
            //    // Xử lý nếu AccountID không hợp lệ, có thể là redirect hoặc thông báo lỗi
            //    return RedirectToAction("Index", "Home");
            //}

            //// Kiểm tra totalPrice có khác null không
            //if (!totalPrice.HasValue)
            //{
            //    // Xử lý nếu totalPrice không có giá trị
            //    return RedirectToAction("Index", "Home");
            //}
           
            Order newOrder = new Order
            {
                AccountID = Ac.AccountID,
                DefaultAddress = deliveryAddress,
                OrderTime = DateTime.Now,
                DeliveryTime = DateTime.Now.AddDays(3),
                StatusOrder = 2,
                Price = (decimal)totalPrice // Sử dụng totalPrice.Value vì nó đã được kiểm tra  
            };

            DB.Orders.Add(newOrder);
            DB.SaveChanges();

            // Lấy OrderID vừa tạo để lưu OrderDetails
            var orderId = newOrder.OrderID;

            // Lưu các chi tiết đơn hàng
            var selectedProducts = Request.Form["selectedProducts"].Split(',');
            var quantities = Request.Form["quantities"].Split(',');
            var coupon = DB.Coupons.FirstOrDefault(c => c.AccountID == Ac.AccountID);


            for (int i = 0; i < selectedProducts.Length; i++)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = int.Parse(selectedProducts[i]),
                    Quantity = int.Parse(quantities[i]),
                    Coupon=(coupon.Discount)
                };
                DB.OrderDetails.Add(orderDetail);
            }

            
            
            DB.SaveChanges();
            

            var orders = DB.Orders.Where(o => o.AccountID == Ac.AccountID).ToList(); // Lấy danh sách các đơn hàng của tài khoản
            return View(orders); // Truyền danh sách đơn hàng tới View

           
        }

       
    }
}

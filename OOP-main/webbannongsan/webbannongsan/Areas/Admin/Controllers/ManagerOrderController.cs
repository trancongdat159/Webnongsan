using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webbannongsan.Models;

namespace webbannongsan.Areas.Admin.Controllers
{
    public class ManagerOrderController : Controller
    {
        // GET: Admin/Order
        DB_TadEntities DB = new DB_TadEntities();
        public ActionResult ListOrder()
        {
            List<Order> orders= DB.Orders.ToList();
            
            return View(orders);
        }
    }
}
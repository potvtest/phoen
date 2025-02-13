using System.Web.Mvc;

namespace Pheonix.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return Redirect("/Help");
            return View();
        }

        public ActionResult About(bool notValid = false)
        {
            ViewBag.Title = "About";

            return View(notValid);
        }
    }
}
using System.Web.Mvc;

namespace DocStorage.Controllers
{
    public class HomeController : Controller
    {

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Message = "Тестовое задание - хранилище документов";

            return View();
        }
    }
}

using System.Web.Mvc;

namespace MemoriesCollection.Controllers
{
    public class ErrorHandleController : Controller
    {
        // GET: ErrorHandle
        public ActionResult Index()
        {
            ViewBag.ErrMsg = Request.QueryString["ErrMsg"];
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
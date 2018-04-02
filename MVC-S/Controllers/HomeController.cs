using System.Web.Mvc;
using IP3_8IEN.BL;

namespace MVC_S.Controllers
{
    public class HomeController : Controller
    {
        private IDataManager dMgr;
        private IGebruikerManager gMgr;

        public HomeController()
        {
            // Hier wordt voorlopig gewoon wat testdata doorgegeven aan de 'Managers'
            dMgr = new DataManager();
            gMgr = new GebruikerManager();

            dMgr.AddMessages($"c:\\textgaindump.json");
            dMgr.AddOrganisation("Groen");
            dMgr.AddOrganisation("Groen");
            dMgr.AddOrganisation("VLD");
            dMgr.AddTewerkstelling("Imade", "Annouri",  "Groen");
            dMgr.AddTewerkstelling("Annick", "De Ridder", "Groen");
            gMgr.AddGebruikers($"c:\\AddGebruikersInit.Json");
            gMgr.AddAlertInstelling($"c:\\AddAlertInstelling.json");
            gMgr.AddAlerts($"c:\\AddAlerts.json");
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
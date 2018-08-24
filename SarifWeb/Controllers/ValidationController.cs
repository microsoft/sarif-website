using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace SarifWeb.Controllers
{
    public class ValidationController : Controller
    {
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ValidateFiles(IEnumerable<HttpPostedFileBase> files)
        {
            foreach (var file in files)
            {
                string filePath = Guid.NewGuid() + Path.GetExtension(file.FileName);
                //file.SaveAs(Path.Combine(Server.MapPath("~/UploadedFiles"), filePath));
            }

            return Json("File successfully uploaded for validation.");
        }
    }
}
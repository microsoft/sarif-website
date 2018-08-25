using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace SarifWeb.Controllers
{
    public class ValidationUiController : Controller
    {
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ValidateFiles(IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
            {
                string uploadedFileName = Guid.NewGuid() + Path.GetExtension(uploadedFile.FileName);
                string uploadedFilePath = Path.Combine(Server.MapPath("~/UploadedFiles"), uploadedFileName);

                try
                {
                    uploadedFile.SaveAs(uploadedFilePath);
                }
                finally
                {
                    if (System.IO.File.Exists(uploadedFilePath))
                    {
                        System.IO.File.Delete(uploadedFilePath);
                    }
                }
            }

            return Json("File successfully uploaded for validation.");
        }
    }
}
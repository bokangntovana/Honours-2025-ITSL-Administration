using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.IO;

namespace ITSL_Administration.Controllers
{
    public class UploadFilesController : Controller
    {

        //// GET: Upload
        //public ActionResult Index()
        //{
        //    return View();
        //}
        //[HttpGet]
        //public ActionResult UploadFile()
        //{
        //    return View();
        //}
        //// Replace the parameter type 'HttpPostedFileBase' with 'IFormFile' in the UploadFile POST action
        //[HttpPost]
        //public ActionResult UploadFile(IFormFile file)
        //{
        //    try
        //    {
        //        if (file != null && file.Length > 0)
        //        {
        //            string _FileName = Path.GetFileName(file.FileName);
        //            // Use HttpContext to get the web root path
        //            string uploadPath = Path.Combine(HttpContext.Request.PathBase, "UploadedFiles");
        //            if (!Directory.Exists(uploadPath))
        //            {
        //                Directory.CreateDirectory(uploadPath);
        //            }
        //            string _path = Path.Combine(uploadPath, _FileName);
        //            using (var stream = new FileStream(_path, FileMode.Create))
        //            {
        //                file.CopyTo(stream);
        //            }
        //        }
        //        ViewBag.Message = "File Uploaded Successfully!!";
        //        return View();
        //    }
        //    catch
        //    {
        //        ViewBag.Message = "File upload failed!!";
        //        return View();
        //    }
        //}

        //public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        //{
        //    long size = files.Sum(f => f.Length);

        //    foreach (var formFile in files)
        //    {
        //        if (formFile.Length > 0)
        //        {
        //            var filePath = Path.GetTempFileName();

        //            using (var stream = System.IO.File.Create(filePath))
        //            {
        //                await formFile.CopyToAsync(stream);
        //            }
        //        }
        //    }

        //    // Process uploaded files
        //    //Don't rely on or trust the FileName property without validation.
        //    return Ok(new { count = files.Count, size });
        //}

    }
}

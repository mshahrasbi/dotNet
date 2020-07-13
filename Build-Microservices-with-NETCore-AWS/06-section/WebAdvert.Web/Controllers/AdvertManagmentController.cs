using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagmentController : Controller
    {
        private IFileUploader _s3FileLoader;

        public AdvertManagmentController(IFileUploader s3FileUploader)
        {
            this._s3FileLoader = s3FileUploader;
        }

        public IActionResult Create(CreateAdvertViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // here we must call the AdvertApi, create the Advertisement in the database and return id
                var id = "xxxxx";
                var filename = "";
                if (imageFile != null)
                {
                    filename = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : id;
                    var filePath = $"{id}/{filename}";

                    try
                    {
                        using (var readStream = imageFile.OpenReadStream())
                        {
                            var result = await this._s3FileLoader.UploadFileAsync(filePath, readStream)
                                    .ConfigureAwait(false);

                            if (!result)
                            {
                                throw new Exception("Could not upload the image to file repository. Please see the logs for details");
                            }

                            // now we call AdvertAPI and confirm the Advertisement

                            return RedirectToAction("Index", "Home");
                        }
                    } 
                    catch(Exception e)
                    {
                        // Call the AdvertApi and cancel the Advertisement
                        Console.WriteLine(string.Format("[AdvertManagementController] Create Action: Error - {0}", e.Message));
                    }
                }
            }
            return View(model);
        }
    }
}

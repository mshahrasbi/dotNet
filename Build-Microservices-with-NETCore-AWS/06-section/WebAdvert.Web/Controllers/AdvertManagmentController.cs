using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagmentController : Controller
    {
        private readonly IFileUploader _s3FileLoader;
        private readonly IAdvertApiCleint _advertApiCleint;
        private readonly IMapper _mapper;

        public AdvertManagmentController(IFileUploader s3FileUploader, IAdvertApiCleint advertApiCleint, IMapper mapper)
        {
            this._s3FileLoader = s3FileUploader;
            this._advertApiCleint = advertApiCleint;
            this._mapper = mapper;
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
                var createAdvertModel = this._mapper.Map<CreateAdvertModel>(model);
                var apiCallResponse = await this._advertApiCleint.Create(createAdvertModel);
                var id = apiCallResponse.Id

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
                            var confirmModel = new ConfirmAdvertRequest()
                            {
                                Id = id,
                                FilePath = filePath,
                                Status = AdvertStatus.Active
                            };
                            var canConfirm = await this._advertApiCleint.Confirm(confirmModel).ConfigureAwait(false);
                            if (!canConfirm)
                            {
                                throw new Exception($"Cannot Confirm advert of Id = {id}");
                            }


                            return RedirectToAction("Index", "Home");
                        }
                    } 
                    catch(Exception e)
                    {
                        // Call the AdvertApi and cancel the Advertisement
                        var confirmModel = new ConfirmAdvertRequest()
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Pending
                        };
                        await this._advertApiCleint.Confirm(confirmModel).ConfigureAwait(false);

                        Console.WriteLine(string.Format("[AdvertManagementController] Create Action: Error - {0}", e.Message));
                    }
                }
            }
            return View(model);
        }
    }
}

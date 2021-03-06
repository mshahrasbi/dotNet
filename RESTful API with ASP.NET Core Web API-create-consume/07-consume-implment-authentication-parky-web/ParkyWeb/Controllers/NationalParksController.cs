﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkyWeb.Models;
using ParkyWeb.repository.IRepository;

namespace ParkyWeb.Controllers
{
    public class NationalParksController : Controller
    {
        private INationalParkRepository _npRepo;

        public NationalParksController(INationalParkRepository npRepo)
        {
            this._npRepo = npRepo;
        }

        public IActionResult Index()
        {
            return View(new NationalPark() { });
        }

        public async Task<IActionResult> Upsert(int? Id)
        {
            NationalPark obj = new NationalPark();

            if (Id == null)
            {
                // this will be true for insert / create
                return View(obj);
            }

            // otherwise it is for update
            obj = await this._npRepo.GetAsync(SD.NationalParkAPIPath, Id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(NationalPark obj)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using var ms1 = new MemoryStream();

                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();

                    }

                    obj.Picture = p1;
                } else
                {
                    var objFromDb = await this._npRepo.GetAsync(SD.NationalParkAPIPath, obj.Id);
                    obj.Picture = objFromDb.Picture;
                }

                if (obj.Id == 0)
                {
                    await this._npRepo.CreateAsync(SD.NationalParkAPIPath, obj);
                } else
                {
                    await this._npRepo.updateAsync(SD.NationalParkAPIPath + obj.Id, obj);
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(obj);
            }
        }


        public async Task<IActionResult> GetAllNationalPark()
        {
            return Json(new { data = await this._npRepo.GetAllAsync(SD.NationalParkAPIPath) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var status = await this._npRepo.DeleteAsync(SD.NationalParkAPIPath, Id);
            if (status)
            {
                return Json(new { success = true, message = "Delete Successful" });
            }
            return Json(new { success = false, message = "Delete Not Successful" });
        }

    }
}

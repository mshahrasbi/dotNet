﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModel;
using ParkyWeb.repository.IRepository;

namespace ParkyWeb.Controllers
{
    public class TrailsController : Controller
    {
        private INationalParkRepository _npRepo;
        private ITrailRepository _trailRepo;

        public TrailsController(INationalParkRepository npRepo, ITrailRepository trailRepo)
        {
            this._npRepo = npRepo;
            this._trailRepo = trailRepo;
        }

        public IActionResult Index()
        {
            return View(new Trail() { });
        }

        public async Task<IActionResult> Upsert(int? Id)
        {
            IEnumerable<NationalPark> npList = await this._npRepo.GetAllAsync(SD.NationalParkAPIPath);
            TrailsVM objVM = new TrailsVM()
            {
                NationalParkList = npList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                Trail = new Trail()
            };

            if (Id == null)
            {
                // this will be true for insert / create
                return View(objVM);
            }

            // otherwise it is for update
            objVM.Trail = await this._trailRepo.GetAsync(SD.TrailAPIPath, Id.GetValueOrDefault());
            if (objVM == null)
            {
                return NotFound();
            }

            return View(objVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(TrailsVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Trail.Id == 0)
                {
                    await this._trailRepo.CreateAsync(SD.TrailAPIPath, obj.Trail);
                } else
                {
                    await this._trailRepo.updateAsync(SD.TrailAPIPath + obj.Trail.Id, obj.Trail);
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                IEnumerable<NationalPark> npList = await this._npRepo.GetAllAsync(SD.NationalParkAPIPath);
                TrailsVM objVM = new TrailsVM()
                {
                    NationalParkList = npList.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    Trail = obj.Trail
                };
                // this will be true for insert / create
                return View(objVM);
            }
        }


        public async Task<IActionResult> GetAllTrail()
        {
            return Json(new { data = await this._trailRepo.GetAllAsync(SD.TrailAPIPath) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var status = await this._trailRepo.DeleteAsync(SD.TrailAPIPath, Id);
            if (status)
            {
                return Json(new { success = true, message = "Delete Successful" });
            }
            return Json(new { success = false, message = "Delete Not Successful" });
        }

    }
}

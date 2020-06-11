using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using ParkyAPI.Repository.IRepository;

namespace ParkyAPI.Controllers
{
    [Route("api/v{version:apiVersion}/trails")]
    // [Route("api/Trails")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class TrailsController : ControllerBase
    {
        private ITrailRepository _trailRepo;
        private IMapper _mapper;

        public TrailsController(ITrailRepository trailRepo, IMapper mapper)
        {
            this._mapper = mapper;
            this._trailRepo = trailRepo;
        }

        /// <summary>
        /// Get Trail Parks
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TrailDto>))]
        public IActionResult GetTrails()
        {
            var objList = this._trailRepo.GetTrails();
            var objDto = new List<TrailDto>();

            foreach (var obj in objList)
            {
                objDto.Add(_mapper.Map<TrailDto>(obj));
            }

            return Ok(objDto);
        }

        /// <summary>
        /// Get National Park
        /// </summary>
        /// <param name="trailId">trailId</param>
        /// <returns></returns>
        [HttpGet("{trailId:int}", Name = "GetTrail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrail(int trailId)
        {
            var obj = this._trailRepo.GetTrail(trailId);
            if (obj == null)
            {
                return NotFound();
            }

            var objDto = this._mapper.Map<TrailDto>(obj);

            return Ok(objDto);
        }


        /// <summary>
        /// Get National Park
        /// </summary>
        /// <param name="trailId">trailId</param>
        /// <returns></returns>
        [HttpGet("[action]/{nationalParkId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrailsInNationalPark(int nationalParkId)
        {
            var objlist = this._trailRepo.GetTrailsInNationalPark(nationalParkId);
            if (objlist == null)
            {
                return NotFound();
            }

            var objDtos = new List<TrailDto>();
            foreach(var obj in objlist)
            {
                objDtos.Add(this._mapper.Map<TrailDto>(obj));
            }

            return Ok(objDtos);
        }


        /// <summary>
        /// Create National Park
        /// </summary>
        /// <param name="trailDto">trailDto</param>
        /// <returns></returns>

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateTrail([FromBody] TrailCreateDto trailDto)
        {
            if (trailDto == null)
            {
                // ModelState typically contains all of the errors if any are encountered.
                return BadRequest(ModelState);
            }

            if (_trailRepo.TrailExists(trailDto.Name))
            {
                ModelState.AddModelError("", "Trail Exists");
                return StatusCode(404, ModelState);
            }

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            var trailObj = this._mapper.Map<Trail>(trailDto);

            if (!_trailRepo.CreateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("Trail", new { trailId = trailObj.Id }, trailObj);
        }


        [HttpPatch("{trailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateTrail(int trailId, [FromBody] TrailUpdateDto trailDto)
        {
            if (trailDto == null || trailId != trailDto.Id)
            {
                // ModelState typically contains all of the errors if any are encountered.
                return BadRequest(ModelState);
            }

            var trailObj = this._mapper.Map<Trail>(trailDto);

            if (!_trailRepo.UpdateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }


        /// <summary>
        /// Delete National Park
        /// </summary>
        /// <param name="trailId">trailId</param>
        /// <returns></returns>

        [HttpDelete("{trailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteTrail(int trailId)
        {
            if (!this._trailRepo.TrailExists(trailId))
            {
                return NotFound();
            }

            var trailObj = this._trailRepo.GetTrail(trailId);

            if (!_trailRepo.DeleteTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }



    }
}

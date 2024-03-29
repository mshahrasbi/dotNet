﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Models.messages;
using AdvertApi.Services;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AdvertApi.Controllers
{
    [ApiController]
    [Route("api/v1/adverts")]
    public class AdvertController : ControllerBase  
    {
        private readonly IAdvertStorageService _advertStorageService;
        private readonly IConfiguration _configuration;

        public AdvertController(IAdvertStorageService advertStorageService, IConfiguration configuration)
        {
            this._advertStorageService = advertStorageService;
            this._configuration = configuration;
        }


        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(404)]
        [ProducesResponseType(201, Type = typeof(CreateAdvertResponse))]
        public async Task<IActionResult> create(AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await this._advertStorageService.AddAsync(model);

            } 
            catch(KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return StatusCode(201, new CreateAdvertResponse { Id = recordId });

        }


        [HttpPut]
        [Route("Confirm")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model)
        {
            try
            {
                await this._advertStorageService.ConfirmAsync(model);
                await RaiseAdvertConfirmedMessage(model);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

            return new OkResult();
        }

        private async Task RaiseAdvertConfirmedMessage(ConfirmAdvertModel model)
        {
            var topicARN = this._configuration.GetValue<string>("TopicARN");
            var dbModel = await this._advertStorageService.GetByIdAsync(model.Id);

            using (var client = new AmazonSimpleNotificationServiceClient())
            {
                var message = new AdvertConfirmedMessage {
                    Id = model.Id,
                    Title = dbModel.Title
                };
                var messageJson = JsonConvert.SerializeObject(message);
                await client.PublishAsync(topicARN, messageJson);
            }
        }
    }
}

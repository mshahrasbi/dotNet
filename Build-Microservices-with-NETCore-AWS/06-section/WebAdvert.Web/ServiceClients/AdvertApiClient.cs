using AdvertApi.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiCleint
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public AdvertApiClient(IConfiguration configuration, HttpClient client)
        {
            this._configuration = configuration;
            this._client = client;

            // now we need to configure our client 
            var createUrl = this._configuration.GetSection("AdvertApi").GetValue<string>("CreateUrl");
            this._client.BaseAddress = new Uri(createUrl);
            // then set the headers
            this._client.DefaultRequestHeaders.Add("Content-type", "application/json");
        }


        public async Task<AdvertResponse> Create(CreateAdvertModel model)
        {
            // first we need to map the model to model that is understood by AdvertApi our service
            var advertApiModel = new AdvertModel(); // automapper
            // need toserialize the model
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);
            var response = await this._client.PostAsync(this._client.BaseAddress, new StringContent(jsonModel)).ConfigureAwait(false);
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);

            var advertReponse = new AdvertResponse(); // automapper

            return advertReponse;
        }
    }
}

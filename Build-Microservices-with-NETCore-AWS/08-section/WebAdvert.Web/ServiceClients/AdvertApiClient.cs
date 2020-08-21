using AdvertApi.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {
        private readonly string _baseAddress;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public AdvertApiClient(IConfiguration configuration, HttpClient client, IMapper mapper)
        {
            this._client = client;
            this._mapper = mapper;

            // now we need to configure our client 
            _baseAddress = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            //this._client.BaseAddress = new Uri(_baseAddress);
            //// then set the headers
            //this._client.DefaultRequestHeaders.Add("Content-type", "application/json");
        }

        public async Task<bool> ConfirmAsync(ConfirmAdvertRequest model)
        {
            var advertModel = this._mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            var response = await this._client
                .PutAsync(new Uri($"{this._client.BaseAddress}/confirm"), new StringContent(jsonModel))
                .ConfigureAwait(false);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<AdvertResponse> CreateAsync(CreateAdvertModel model)
        {
            // first we need to map the model to model that is understood by AdvertApi our service
            var advertApiModel = this._mapper.Map<AdvertModel>(model); // automapper

            // need toserialize the model
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);
            var response = await this._client
                .PostAsync(new Uri($"{this._client.BaseAddress}/create"), new StringContent(jsonModel))
                .ConfigureAwait(false);
            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);

            var advertReponse = this._mapper.Map<AdvertResponse>(createAdvertResponse); // automapper

            return advertReponse;
        }

        public async Task<List<Advertisement>> GetAllAsync()
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/all")).ConfigureAwait(false);
            var allAdvertModels = await apiCallResponse.Content.ReadAsAsync<List<AdvertModel>>().ConfigureAwait(false);
            return allAdvertModels.Select(x => _mapper.Map<Advertisement>(x)).ToList();
        }

        public async Task<Advertisement> GetAsync(string advertId)
        {
            var apiCallResponse = await _client.GetAsync(new Uri($"{_baseAddress}/{advertId}")).ConfigureAwait(false);
            var fullAdvert = await apiCallResponse.Content.ReadAsAsync<AdvertModel>().ConfigureAwait(false);
            return _mapper.Map<Advertisement>(fullAdvert);
        }


    }
}

using AdvertApi.Models;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
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

            var discoveryClient = new AmazonServiceDiscoveryClient();
            var discoveryTask = discoveryClient.DiscoverInstancesAsync(new DiscoverInstancesRequest { 
                ServiceName="advertapi",
                NamespaceName="WebAdvertisement"
            });
            discoveryTask.Wait();
            // This is just an example and it shouldnot be run from Constructor. best to have its own class
            /*
             * Service Discovery does not provide LB, so one thing you can do is to randomize
             *  this list, you can try to linq expiration to randomize and randomly pick up
             *  any instance just to scatter the load across all the instances
             */
            var instances = discoveryTask.Result.Instances;
            // how to get Ip
            var ipv4 = instances[0].Attributes["AWS_INSTANCE_IPV4"];
            var port = instances[0].Attributes["AWS_INSTANCE_PORT"];
            // then use ipv4 and port to create base url
            _baseAddress = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            _client.BaseAddress = new Uri(_baseAddress);
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

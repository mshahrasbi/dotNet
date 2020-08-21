﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebAdvert.Web.Models;


namespace WebAdvert.Web.ServiceClients
{
    public class SearchApiClient : ISearchApiClient
    {
        private readonly HttpClient _client;
        private readonly string BaseAddress = string.Empty;


        public SearchApiClient(HttpClient client, IConfiguration configuration)
        {
            this._client = client;
            this.BaseAddress = configuration.GetSection("SearchApi").GetValue<string>("url");
        }

        public async Task<List<AdvertType>> Search(string keyword)
        {
            var result = new List<AdvertType>();
            var callUrl = $"{BaseAddress}/search/v1/{keyword}";
            var httpResponse = await _client.GetAsync(new Uri(callUrl)).ConfigureAwait(false);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // we need to get Microsoft.AspNet.WebApi.Client from NuGet package to be able to
                // use the ReadAsAsync method of HttpResponseMessage
                var allAdverts = await httpResponse.Content.ReadAsAsync<List<AdvertType>>().ConfigureAwait(false);
                result.AddRange(allAdverts);
            }

            return result;
        }
    }
}

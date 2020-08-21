using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.SearchApi.Models;

namespace WebAdvert.SearchApi.Services
{
    public class SearchService : ISearchService
    {

        private readonly IElasticClient _elasticClient;

        public SearchService(IElasticClient elasticClient)
        {
            this._elasticClient = elasticClient;
        }

        public async Task<List<AdvertType>> Search(string keyword)
        {
            var searchResponse = await _elasticClient
                    .SearchAsync<AdvertType>(s => s
                                                .Query(q => q
                                                            .Term(f => f.Title, keyword.ToLower()))
                                                );

            return searchResponse.Hits.Select(hit => hit.Source).ToList();
        }
    }
}

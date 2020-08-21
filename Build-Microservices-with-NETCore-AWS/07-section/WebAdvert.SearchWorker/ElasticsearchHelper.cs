using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebAdvert.SearchWorker
{
    public static class ElasticsearchHelper
    {
        private static IElasticClient _elasticClient = null;

        public static IElasticClient getInstance(IConfiguration configuration)
        {
            if (_elasticClient == null)
            {
                var url = configuration.GetSection("ES").GetValue<string>("url");
                var settings = new ConnectionSettings(new Uri(url)).DefaultIndex("adverts");

                _elasticClient = new ElasticClient(settings);
            }

            return _elasticClient;
        }
    }
}

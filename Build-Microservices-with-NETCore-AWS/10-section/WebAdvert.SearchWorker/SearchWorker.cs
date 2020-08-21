using AdvertApi.Models.messages;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Nest;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace WebAdvert.SearchWorker
{
    public class SearchWorker
    {
        private readonly IElasticClient _elasticClient;

        public SearchWorker():this(ElasticsearchHelper.getInstance(ConfigurationHelper.Instance))
        {
        }
        public SearchWorker(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task MyFunction(SNSEvent snsEvent, ILambdaContext context)
        {
            foreach (var record in snsEvent.Records)
            {
                context.Logger.LogLine(record.Sns.Message);

                var message = JsonConvert.DeserializeObject<AdvertConfirmedMessage>(record.Sns.Message);
                var AdvertDocument = MappingHelper.Map(message);

                await _elasticClient.IndexDocumentAsync(AdvertDocument);
            }
        }
    }
}

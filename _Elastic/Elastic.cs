using Nest;
using System.Reflection.Metadata;

namespace _Elastic
{
    public class Elastic
    {
        public static ElasticClient CreateIndex()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("news_index");

            var client = new ElasticClient(settings);

            var createIndexResponse = client.Indices
                .Create("news_index", c => c
                .Map<NewsModel>(m => m.AutoMap())
            );

            return client;
        }
    }
}
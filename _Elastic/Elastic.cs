using Nest;

namespace _Elastic
{
    public class Elastic
    {
        public static ElasticClient CreateIndex()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("news_index");

            var client = new ElasticClient(settings);

            var createIndexResponse = client.Indices
                .Create("news_index", c => c
                .Map<NewsModel>(m => m.AutoMap())
            );

            return client;
        }

        public static ISearchResponse<NewsModel> GetNewsByHead(string head)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("news_index");

            var client = new ElasticClient(settings);

            var searchResponse = client.Search<NewsModel>(s => s
                                        .Query(q => q
                                        .MultiMatch(m => m
                                        .Fields(f => f
                                        .Field(ff => ff.Head))
                                        .Query(head))));

            return searchResponse;
        }
        public static ISearchResponse<NewsModel> GetNewsByTitleOrText(string title, string text)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("news_index");

            var client = new ElasticClient(settings);

            var searchResponse = client.Search<NewsModel>(s => s
                            .Query(q => q
                                .Bool(b => b
                                    .Should(sh => sh
                                        .Match(m => m
                                            .Field(f => f.Title)
                                            .Query(title)
                                        ),
                                        sh => sh
                                        .Match(m => m
                                            .Field(f => f.Text)
                                            .Query(text)
                                        )
                                    )
                                )
                            )
                        );
            return searchResponse;

        }

    }
}
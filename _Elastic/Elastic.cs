using Nest;
using System.Reflection.Metadata;

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

        //public static ISearchResponse<NewsModel> Get()
        //{
        //    var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("news_index");

        //    var client = new ElasticClient(settings);

        //    var searchResponse = client.Search<NewsModel>(s => s
        //             .Index("news_index")
        //             .Query(q => q
        //             .MoreLikeThis(m => m
        //             .Fields(f => f.Field(fа => fа.Head))
        //             .Like(l => l.Text("GET /news_index/_search\r\n{\r\n  \"query\": {\r\n    \"match_all\": {}\r\n  }\r\n}"))
        //             .MinTermFrequency(3)
        //             .MinDocumentFrequency(1)
        //             .MaxQueryTerms(1)
        //             .MinimumShouldMatch(new MinimumShouldMatch("10%"))
        //         )
        //     )
        // );

        //    return searchResponse;
        //}

        public static void GetRegularExpression()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("news_index");

            var client = new ElasticClient(settings);

            //var searchResponse = client.Search<NewsModel>(s => s
            //            .Query(q => q
            //                .Bool(b => b
            //                    .Must(m => m
            //                        .Match(mt => mt
            //                            .Field(f => f.Text)
            //                            .Query("Россия")
            //                        ),
            //                        m => m
            //                        .Regexp(r => r
            //                            .Field(f => f.Text)
            //                            .Value("")
            //                        )
            //                    )
            //                )
            //            )
            //        );


            var searchResponse = client.Search<NewsModel>(s => s
                        .Query(q => q
                            .Regexp(r => r
                                .Field(f => f.Text)
                                .Value("Спецоперации")
                            )
                        )
                        .Aggregations(a => a
                            .Terms("Новости", t => t
                                .Field(f => f.Title)
                                .Size(10000)
                            )
                        )
                    );

            var duplicates = searchResponse.Aggregations.Terms("Новости").Buckets
                .Where(b => b.DocCount > 1)
                .Select(b => b.Key);

            foreach (var duplicate in duplicates)
            {
                var duplicateResponse = client.Search<NewsModel>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Text)
                            .Query(duplicate)
                        )
                    )
                );

                var documents = duplicateResponse.Documents;
            }
        }
    }
}
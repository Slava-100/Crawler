using _Crawler;
using _Elastic;
using Nest;
using RabbitMQ;

//string url = "https://www.1tv.ru/news";
//Crawler cr = Crawler.GetInstance();
//await cr.ParseAsync(url);

//await Task.Delay(5000);
var getNews = Elastic.GetNewsByHead("лет");

Console.WriteLine(getNews.Hits.Count);

foreach (var hit in getNews.Hits)
{
    Console.WriteLine($"Title: {hit.Source.Title}");
    Console.WriteLine($"Content: {hit.Source.Head}");
    Console.WriteLine($"Date: {hit.Source.PublishDate}");
    Console.WriteLine($"Text: {hit.Source.Text}");
    Console.WriteLine();
}

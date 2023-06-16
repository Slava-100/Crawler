using _Crawler;
using _Elastic;
using RabbitMQ;

string url = "https://www.1tv.ru/news";
Crawler cr = Crawler.GetInstance();
await cr.ParseAsync(url);

//await Task.Delay(5000);

Console.ReadLine();
using _Crawler;
using RabbitMQ;

string url = "https://www.1tv.ru/news";
Crawler cr = Crawler.GetInstance();
await cr.ParseAsync(url);

await Task.Delay(5000);

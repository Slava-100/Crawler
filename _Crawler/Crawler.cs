using HtmlAgilityPack;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace _Crawler
{
    public class Crawler
    {
        private HashSet<string> _visitedUrls = new HashSet<string>();
        private RabbitData _rabbitD = new RabbitData();
        private RabbitLinks _rabbitL = new RabbitLinks();
        private bool _flagStarterLinks = false;
        private static Crawler instance;

        private Crawler()
        { }

        public static Crawler GetInstance()
        {
            if (instance == null)
                instance = new Crawler( );
            return instance;
        }
        public async Task ParseAsync(string url)
        {
            try
            {
                if (_visitedUrls.Contains(url))
                {
                    return;
                }
                else
                {
                    _visitedUrls.Add(url);

                    await ParsesAndPublishesDataAndLinkInRabbit(url);

                    await _rabbitL.ConsumesLinkAsync();
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                await _rabbitD.PublishesDataAsync(ex.Message);
            }
        }
        private async Task GetHeadingsAsync(HtmlNode node)
        {
            var headings = node.SelectNodes(".//article");
            if (headings != null && headings.Count > 0)
            {
                foreach (var node2 in headings)
                {
                    var time = node2.SelectSingleNode(".//time");
                    if (time != null)
                    {
                        _rabbitD.PublishesDataAsync(time.InnerText);
                    }
                    var text_h2 = node2.SelectSingleNode(".//h2");
                    var text_h1 = node2.SelectSingleNode(".//h1");
                    var text_h3 = node2.SelectSingleNode(".//h3/a");
                    if (text_h1 != null)
                    {
                        _rabbitD.PublishesDataAsync(text_h1.InnerText);
                    }
                    if (text_h2 != null)
                    {
                        _rabbitD.PublishesDataAsync(text_h2.InnerText);
                    }
                    if (text_h3 != null)
                    {
                        _rabbitD.PublishesDataAsync(text_h3.InnerText);
                    }
                }
            }
        }

        private async Task GetTextInPAndSpanAsync(HtmlDocument doc)
        {
            var text_div = doc.DocumentNode.SelectSingleNode(".//div[@class='editor text-block active']");
            if (text_div != null)
            {
                var text_p_span = text_div.SelectNodes(".//p//span");
                if (text_p_span != null && text_p_span.Count > 0)
                {
                    foreach(var node in text_p_span)
                    {
                        _rabbitD.PublishesDataAsync(node.InnerText);
                    }
                }
                else
                {
                    var text_p = text_div.SelectNodes(".//p");
                    if(text_p != null && text_p.Count > 0)
                    foreach (var node in text_p)
                    {
                            _rabbitD.PublishesDataAsync(node.InnerText);
                        }
                }
            }
        }

        private async Task ParsesAndPublishesDataAndLinkInRabbit(string url)
        {
            var httpClient = new HttpClient();
            string htmlContent = await httpClient.GetStringAsync(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var sections = doc.DocumentNode.SelectNodes(".//section");
            if (sections != null && sections.Count() > 0)
            {
                foreach (var node1 in sections)
                {
                    var text1 = node1.SelectSingleNode(".//h2");
                    if (text1 != null)
                    {
                        _rabbitD.PublishesDataAsync(text1.InnerText);
                    }
                    await GetHeadingsAsync(node1);
                }
            }
            else
            {
                await GetHeadingsAsync(doc.DocumentNode);
            }
            await GetTextInPAndSpanAsync(doc);

            if (!_flagStarterLinks)
            {
                var links = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'http://www.1tv.ru/') or starts-with(@href, 'https://www.1tv.ru/')]/@href");

                foreach (var link in links)
                {
                    _rabbitL.PublishesLinkAsync(link.Attributes["href"]?.Value);
                }
                _flagStarterLinks = true;
            }
        }
    }
}



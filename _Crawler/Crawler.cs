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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task GetHeadingsAsync(HtmlNode node, NewsModel news = null)
        {
            var headings = node.SelectNodes(".//article");

            if (headings != null && headings.Count > 0)
            {
                foreach (var node2 in headings)
                {
                    NewsModel news1 = new NewsModel();
                    if (news != null) { news1.Title = news.Title; }
                    var time = node2.SelectSingleNode(".//time");

                    if (time != null)
                    {

                        news1.PublishDate = time.InnerText;
                    }

                    var head2 = node2.SelectSingleNode(".//h2");
                    var head1 = node2.SelectSingleNode(".//h1");
                    var head3 = node2.SelectSingleNode(".//h3/a");
                    var text_div = node2.SelectSingleNode(".//div[@class='text']");

                    if (head1 != null)
                    {
                        news1.Head = head1.InnerText;
                    }
                    if (head2 != null)
                    {
                        news1.Head = head2.InnerText;
                    }
                    if (head3 != null)
                    {
                        news1.Head = head3.InnerText;
                    }
                    if (text_div != null)
                    {
                        news1.Text = text_div.InnerText;
                    }
                    else
                    {
                        var text_div_p = node2.SelectSingleNode(".//div[@class='editor text-block active']");

                        if (text_div_p != null)
                        {
                            var text_p = text_div_p.SelectNodes(".//p");

                            if (text_p != null && text_p.Count > 0)
                            {
                                foreach (var node3 in text_p)
                                {

                                    news1.Text += node3.InnerText;
                                }
                            }
                        }
                    }

                    _rabbitD.PublishesDataAsync(news1);
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
                    NewsModel news = new NewsModel();
                    var text1 = node1.SelectSingleNode(".//h2");
                    if (text1 != null)
                    {
                        news.Title = text1.InnerText;
                    }
                    await GetHeadingsAsync(node1, news);
                }
            }
            else
            {
                await GetHeadingsAsync(doc.DocumentNode);
            }

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



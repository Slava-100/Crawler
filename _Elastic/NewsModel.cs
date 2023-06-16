using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Elastic
{
    public class NewsModel
    {
        public string? Title { get; set; }
        public string? Head { get; set; }
        public string? PublishDate { get; set; }
        public string? Text { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}\nHead: {Head}\nPublishDate: {PublishDate}\nText: {Text}";
        }
    }
}

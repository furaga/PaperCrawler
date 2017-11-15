using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;

namespace PaperCrawlerLib
{
    public class SIGGRAPH
    {
        public class Paper
        {
            public string Conference { get; set; }
            public string Title { get; set; }
            public string[] Authors { get; set; }
            public string OfficialSiteURL { get; set; }
            public string[] OtherURLs { get; set; }
        }


        public string Crawl(string url, string savepath)
        {
            try
            {
                var wc = new System.Net.WebClient();
                wc.DownloadFile(url, savepath);
                wc.Dispose();
                return $"crawled: {url} -> {savepath}";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public List<Paper> Scrape(string htmlPath, string conferenceName)
        {
            try
            {
                var parser = new HtmlParser();
                var document = parser.Parse(System.IO.File.ReadAllText(htmlPath));

                var h2s = document.QuerySelectorAll("h2").ToList();
                var dls = document.QuerySelectorAll("dl").ToList();
                System.Diagnostics.Debug.Assert(h2s.Count == dls.Count);


                List<Paper> paperList = new List<Paper>();

                for (int i = 0; i < h2s.Count; i++)
                {
                    var h2 = h2s[i].Text();

                    var dl = dls[i];
                    var dts = dl.QuerySelectorAll("dt").Where(dt =>false == string.IsNullOrWhiteSpace(dt.InnerHtml)).ToList();
                    var dds = dl.QuerySelectorAll("dd").Where(dt => false == string.IsNullOrWhiteSpace(dt.InnerHtml)).ToList();
                    if (dts.Count != dds.Count)
                    {
                        Console.WriteLine("scaraped: failed in " + h2 + " of " + htmlPath);
                        continue;
                    }

                    for (int k = 0; k < dts.Count; k++)
                    {
                        var dt = dts[k];
                        var dd = dds[k];

                        Paper paper = new Paper();

                        var title = dt.QuerySelector("b");
                        var links = dt.QuerySelectorAll("a");
                        paper.Conference = conferenceName;
                        paper.Title = title.Text();
                        paper.OtherURLs = links.Select(lnk => lnk.GetAttribute("href")).ToArray();

                        paper.OfficialSiteURL = links.Length >= 1 ? links[0].GetAttribute("href") : "";
                        paper.Authors = dd.Text().Split(',').Select(s => s.Trim()).ToArray();

                        paperList.Add(paper);
                    }
                }

                return paperList;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString() + "\n" + ex.StackTrace);
                return null;
            }
        }
    }
}

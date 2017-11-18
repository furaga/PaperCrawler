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
using System.IO;

namespace PaperCrawlerLib
{
    public class CrawlKesen : CrawlBase
    {
        public override void Crawl(string saveDirectory, Action<string> log)
        {
            for (int year = 2005; year <= 2017; year++)
            {
                Download(
                    $"http://kesen.realtimerendering.com/sig{year}.html",
                    System.IO.Path.Combine(saveDirectory, $"SIGGRAPH_{year}.html"), log);
            }

            for (int year = 2008; year <= 2017; year++)
            {
                Download(
                    $"http://kesen.realtimerendering.com/siga{year}Papers.htm",
                   System.IO.Path.Combine(saveDirectory, $"SIGGRAPH_Asia_{year}.html"), log);
            }
        }

        List<string> scrapeHtmls(string htmlDirectory)
        {
            var htmls = System.IO.Directory.GetFiles(htmlDirectory).Where(path => Path.GetFileName(path).StartsWith("SIGGRAPH_")).ToList();
            return htmls;
        }

        public override Dictionary<string, List<Paper>> Scrape(string htmlDirectory, Action<string> log)
        {
            try
            {
                var result = new Dictionary<string, List<Paper>>();
                var htmls = scrapeHtmls(htmlDirectory);
                foreach (var path in htmls)
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string conferenceName = filename.Replace('_', ' ');

                    var parser = new HtmlParser();
                    var document = parser.Parse(System.IO.File.ReadAllText(path));

                    var h2s = document.QuerySelectorAll("h2").ToList();
                    var dls = document.QuerySelectorAll("dl").ToList();
                    System.Diagnostics.Debug.Assert(h2s.Count == dls.Count);


                    List<Paper> paperList = new List<Paper>();

                    for (int i = 0; i < h2s.Count; i++)
                    {
                        var h2 = h2s[i].Text();

                        var dl = dls[i];
                        var dts = dl.QuerySelectorAll("dt").Where(dt => false == string.IsNullOrWhiteSpace(dt.InnerHtml)).ToList();
                        var dds = dl.QuerySelectorAll("dd").Where(dt => false == string.IsNullOrWhiteSpace(dt.InnerHtml)).ToList();
                        if (dts.Count != dds.Count)
                        {
                            Console.WriteLine("scaraped: failed in " + h2 + " of " + path);
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

                    result[filename] = paperList;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString() + "\n" + ex.StackTrace);
                return null;
            }
        }
    }
}

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
using Newtonsoft.Json;

using JObject = Newtonsoft.Json.Linq.JObject;
using JValue = Newtonsoft.Json.Linq.JValue;
using JArray = Newtonsoft.Json.Linq.JArray;
using JProperty = Newtonsoft.Json.Linq.JProperty;
namespace PaperCrawlerLib
{
    public class CrawlUIST : CrawlBase
    {
        public override void Crawl(string saveDirectory, Action<string> log)
        {
            string listPath = System.IO.Path.Combine(saveDirectory, $"UIST_list.html");
            Download(
                @"https://dl.acm.org/event_series.cfm?id=RE172&_cf_containerId=pubs&_cf_nodebug=true&_cf_nocache=true&_cf_clientid=E23FE7AA9882926531C622695AF1AAEF&_cf_rc=1",
                listPath,
                log);

            var parser = new HtmlParser();
            var document = parser.Parse(File.ReadAllText(listPath));
            var aTags = document.QuerySelectorAll("a");
            foreach (var a in aTags)
            {
                string href = a.GetAttribute("href");
                string text = a.Text();
                if (text.StartsWith("UIST '9") || text.StartsWith("UIST '8") || text.Contains("Adjunct"))
                {
                    string url = @"https://dl.acm.org/" + href;

                    string conferenceName = text.Split(':')[0].Replace("'", "_").Replace(" ", "").Trim();
                    conferenceName = conferenceName.Replace("Adjunct", "").Replace("Proceedings", "");

                    string saveConfPath = System.IO.Path.Combine(saveDirectory, conferenceName + ".html");
                    Download(url, saveConfPath, log);

                    string confPage = File.ReadAllText(saveConfPath);
                    int start = confPage.IndexOf("['tab_about.cfm?");
                    start += 2;
                    int end = confPage.IndexOf("']", start + 1);

                    string contentHref = confPage.Substring(start, end - start);
                    string contentURL = @"https://dl.acm.org/" + contentHref;
                    string savePath = System.IO.Path.Combine(saveDirectory, conferenceName + "_contents.html");
                    Download(contentURL, savePath, log);
                }
            }

            //Download($"http://confer.csail.mit.edu/static/conf/uist2017/data/papers.json", System.IO.Path.Combine(saveDirectory, $"UIST_2017.json"), log);
            //Download($"https://dl.acm.org/tab_about.cfm?id=2984751&type=proceeding&sellOnline=1&parent_id=2984751&parent_type=proceeding&title=Proceedings%2520of%2520the%252029th%2520Annual%2520Symposium%2520on%2520User%2520Interface%2520Software%2520and%2520Technology&toctitle=The%252029th%2520Annual%2520ACM%2520Symposium%2520on%2520User%2520Interface%2520Software%2520and%2520Technology&tocissue_date=&notoc=0&usebody=tabbody&tocnext_id=&tocnext_str=&tocprev_id=&tocprev_str=&toctype=conference", System.IO.Path.Combine(saveDirectory, $"UIST_2016.html"), log);
        }

        List<string> selectJSONs(string htmlDirectory)
        {
            var jsons = System.IO.Directory.GetFiles(htmlDirectory).Where(path =>
                Path.GetFileName(path).StartsWith("UIST_") && Path.GetFileName(path).EndsWith(".json")).ToList();
            return jsons;
        }

        List<string> selectHtmls(string htmlDirectory)
        {
            var htmls = System.IO.Directory.GetFiles(htmlDirectory).Where(path =>
                Path.GetFileName(path).StartsWith("UIST_") && Path.GetFileName(path).EndsWith("_contents.html")).ToList();
            return htmls;
        }

        List<string> readAsAuthor(IElement td)
        {
            List<string> authors = new List<string>();
            if (td != null)
            {
                var aTags = td.QuerySelectorAll("a").ToList();
                foreach (var a in aTags)
                {
                    authors.Add(a.Text());
                }
            }
            return authors;
        }

        bool tryDescription(IElement td, out string description)
        {
            bool ok = false;
            description = "";
            if (td != null)
            {
                var spanTags = td.QuerySelectorAll("span").ToList();
                foreach (var span in spanTags)
                {
                    var id = span.GetAttribute("id");
                    if (id != null && id.StartsWith("toHide"))
                    {
                        var p = td.QuerySelector("p");
                        if (p != null)
                        {
                            description = p.Text();
                        }
                        ok = true;
                    }
                }
            }
            return ok;
        }

        bool tryReadAsAuthor(IElement td, List<string> authors)
        {
            bool ok = false;
            if (td != null)
            {
                var aTags = td.QuerySelectorAll("a").ToList();
                foreach (var a in aTags)
                {
                    var href = a.GetAttribute("href");
                    if (href.StartsWith("author"))
                    {
                        authors.Add(a.Text());
                        ok = true;
                    }
                }
            }
            return ok;
        }

        bool tryReadAsTitle(IElement td, out string title, out string officialSiteURL)
        {
            title = "";
            officialSiteURL = "";
            if (td != null)
            {
                if (td.GetAttribute("colspan") == "1")
                {
                    var a = td.QuerySelector("a");
                    if (a != null)
                    {
                        string href = a.GetAttribute("href");
                        if (href.StartsWith("author"))
                        {
                            return false;
                        }
                        title = a.Text();
                        officialSiteURL = "https://dl.acm.org/" + href;
                        return true;
                    }
                }
            }
            return false;
        }

        Dictionary<string, List<Paper>> parseACM(string htmlDirectory, Action<string> log)
        {
            try
            {

                var result = new Dictionary<string, List<Paper>>();
                var htmls = selectHtmls(htmlDirectory);
                foreach (var path in htmls)
                {
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string conferenceName = filename.Replace('_', ' ');

                    var parser = new HtmlParser();
                    var document = parser.Parse(System.IO.File.ReadAllText(path));

                    var tds = document.QuerySelectorAll("tbody > tr > td").ToList();

                    List<Paper> paperList = new List<Paper>();

                    string title, url;
                    List<string> authors = new List<string>();
                    string description;

                    var paper = new Paper()
                    {
                        Conference = conferenceName,
                        OtherURLs = new string[0],
                    };

                    for (int i = 0; i < tds.Count; i++)
                    {
                        var td = tds[i];

                        bool ok = tryReadAsTitle(td, out title, out url);
                        if (ok)
                        {
                            if (paper.Title != null)
                            {
                                paperList.Add(paper);
                                paper = new Paper()
                                {
                                    Conference = conferenceName,
                                    OtherURLs = new string[0],
                                };
                                authors = new List<string>();
                            }

                            paper.Title = title;
                            paper.OfficialSiteURL = url;
                        }
                        ok = tryReadAsAuthor(td, authors);
                        if (ok)
                        {
                            paper.Authors = authors.ToArray();
                       }
                        ok = tryDescription(td, out description);
                        if (ok)
                        {
                            paper.Description = description;
                        }
                    }

                    result[filename] = paperList;
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString() + "\n" + ex.StackTrace);
                return new Dictionary<string, List<Paper>>();
            }
        }

        public override Dictionary<string, List<Paper>> Scrape(string htmlDirectory, Action<string> log)
        {
            return parseACM(htmlDirectory, log);
        }

        Dictionary<string, List<Paper>> parseUIST2017(string htmlDirectory, Action<string> log)
        {
            try
            {
                var result = new Dictionary<string, List<Paper>>();
                var htmls = selectJSONs(htmlDirectory);

                foreach (var path in htmls)
                {
                    var json = File.ReadAllText(path);

                    var jarray = JsonConvert.DeserializeObject<JObject>(json.Substring("entities=".Length));

                    string filename = Path.GetFileNameWithoutExtension(path);
                    string conferenceName = filename.Replace('_', ' ');
                    List<Paper> paperList = new List<Paper>();

                    var cur = jarray.First;
                    while (cur != null)
                    {
                        var paper = new Paper();
                        paper.Title = cur.First.Value<string>("title");
                        paper.Description = cur.First.Value<string>("abstract");
                        paper.Conference = conferenceName;
                        paper.OfficialSiteURL = "http://confer.csail.mit.edu/uist2017/paper#!" + (cur as JProperty).Name;
                        paper.Authors = cur.First["authors"].Select(author =>
                        {
                            return (author as JObject).Value<string>("name");
                        }).ToArray();
                        paper.OtherURLs = new string[0];
                        paperList.Add(paper);

                        cur = cur.Next;
                    }

                    result[filename] = paperList;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString() + "\n" + ex.StackTrace);
                return new Dictionary<string, List<Paper>>();
            }
        }
    }
}

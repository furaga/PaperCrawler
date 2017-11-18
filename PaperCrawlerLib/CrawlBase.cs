using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperCrawlerLib
{
    abstract public class CrawlBase
    {
        protected void Download(string url, string savepath, Action<string> log)
        {
            try
            {
                var wc = new System.Net.WebClient();
                wc.DownloadFile(url, savepath);
                wc.Dispose();
                log?.Invoke($"Download: {url} -> {savepath}");
            }
            catch (Exception ex)
            {
                log?.Invoke(ex.ToString());
            }
        }

        public abstract void Crawl(string saveDirectory, Action<string> log);

        public abstract Dictionary<string, List<Paper>> Scrape(string htmlPath, Action<string> log);
    }
}

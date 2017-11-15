using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace PaperCrawler
{
    public partial class Form1 : Form
    {
        class Crawl
        {
            public string URL { get; set; }
            public string HTMLSavePath { get; set; }
        }


        string HTMLDirectory= "./data/html";
        string JSONDirectory = "./data/json";


        public Form1()
        {
            InitializeComponent();
        }

        private void crawlButton_Click(object sender, EventArgs e)
        {
            List<Crawl> crawls = new List<Crawl>();
            for (int year = 2005; year <= 2017; year++)
            {
                crawls.Add(new Crawl()
                {
                    URL = $"http://kesen.realtimerendering.com/sig{year}.html",
                    HTMLSavePath = System.IO.Path.Combine(HTMLDirectory, $"SIGGRAPH_{year}.html"),
                });
            }

            for (int year = 2008; year <= 2017; year++)
            {
                crawls.Add(new Crawl()
                {
                    URL = $"http://kesen.realtimerendering.com/siga{year}Papers.htm",
                    HTMLSavePath = System.IO.Path.Combine(HTMLDirectory, $"SIGGRAPH_Asia_{year}.html"),
                });
            }

            if (Directory.Exists(HTMLDirectory) == false)
            {
                Directory.CreateDirectory(HTMLDirectory);
            }

            foreach (var c in crawls)
            {
                richTextBox1.Text += ">>>> " + c.URL + " -> "+ c.HTMLSavePath + " >>>\n";
                richTextBox1.Invalidate();
                var siggraph = new PaperCrawlerLib.CrawlKesen();
                string result = siggraph.Crawl(c.URL, c.HTMLSavePath);
                richTextBox1.Text += result + "\n";
                richTextBox1.Invalidate();
            }
        }

        private void scrapeButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(JSONDirectory) == false)
            {
                Directory.CreateDirectory(JSONDirectory);
            }

            var htmls = System.IO.Directory.GetFiles(HTMLDirectory).Where(path => path.EndsWith("html")).ToList();
            foreach (var path in htmls)
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                string conferenceName = filename.Replace('_', ' ');

                var siggraph = new PaperCrawlerLib.CrawlKesen();
                var papers = siggraph.Scrape(path, conferenceName);

                if (papers != null)
                {
                    var json = JsonConvert.SerializeObject(papers, Formatting.Indented);
                    string jsPath = Path.Combine(JSONDirectory, filename + ".json");
                    System.IO.File.WriteAllText(jsPath, json);
                    richTextBox1.Text += $"scrape: {papers.Count} jsons -> {jsPath}\n";
                    richTextBox1.Invalidate();
                }
                else
                {
                    richTextBox1.Text += "scrape: failed. \n";
                }
            }
        }

        private void openHTMLOutputDirButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.IO.Path.GetFullPath(HTMLDirectory));
        }

        private void openJSONOutputDirButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.IO.Path.GetFullPath(JSONDirectory));
        }
    }
}

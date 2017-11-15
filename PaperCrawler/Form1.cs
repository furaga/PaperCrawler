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
            public string HTMLPath { get; set; }
        }

        class Scrape
        {
            public string HTMLPath { get; set; }
            public string JSPath { get; set; }
            public string ConferenceName { get; set; }
        }

        List<Crawl> crawls = new List<Crawl>();
        List<Scrape> scrapes = new List<Scrape>();

        string HTMLDirectory= "./data/html";
        string JSDirectory = "./data/js/";


        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int year = 2005; year <= 2017; year++)
            {
                crawls.Add(new Crawl()
                {
                    URL = $"http://kesen.realtimerendering.com/sig{year}.html",
                    HTMLPath = System.IO.Path.Combine(HTMLDirectory, $"SIGGRAPH_{year}.html"),
                });
            }

            for (int year = 2008; year <= 2017; year++)
            {
                crawls.Add(new Crawl()
                {
                    URL = $"http://kesen.realtimerendering.com/siga{year}Papers.html",
                    HTMLPath = System.IO.Path.Combine(HTMLDirectory, $"SIGGRAPH_Asia_{year}.html"),
                });
            }

            if (Directory.Exists(HTMLDirectory) == false)
            {
                Directory.CreateDirectory(HTMLDirectory);
            }

            foreach (var c in crawls)
            {
                var siggraph = new PaperCrawlerLib.SIGGRAPH();
                string result = siggraph.Crawl(c.URL, c.HTMLPath);
                richTextBox1.Text += result + "\n";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(JSDirectory) == false)
            {
                Directory.CreateDirectory(JSDirectory);
            }

            var htmls = System.IO.Directory.GetFiles(HTMLDirectory).Where(path => path.EndsWith("html")).ToList();
            foreach (var path in htmls)
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                string conferenceName = filename.Replace('_', ' ');

                var siggraph = new PaperCrawlerLib.SIGGRAPH();
                var papers = siggraph.Scrape(path, conferenceName);
                if (papers != null)
                {
                    var json = JsonConvert.SerializeObject(papers, Formatting.Indented);
                    string jsPath = Path.Combine(JSDirectory, filename + ".js");
                    System.IO.File.WriteAllText(jsPath, "export default " + json);
                    richTextBox1.Text += $"scrape: {papers.Count} jsons -> {jsPath}\n";
                }
                else
                {
                    richTextBox1.Text += "scrape: failed. \n";
                }
            }

        }

    }
}

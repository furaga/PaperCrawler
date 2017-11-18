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
        string HTMLDirectory= "./data/html";
        string JSONDirectory = "./data/json";

        void log(string text)
        {
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(text + "\n");
            });
        }

        void savePapers(Dictionary<string, List<PaperCrawlerLib.Paper>> papers, string directory)
        {
            foreach (var kv in papers)
            {
                if (kv.Value != null)
                {
                    var json = JsonConvert.SerializeObject(kv.Value, Formatting.Indented);
                    string jsPath = Path.Combine(directory, kv.Key + ".json");
                    System.IO.File.WriteAllText(jsPath, json);
                    log($"scrape: {kv.Value.Count} jsons -> {jsPath}");
                }
            }
        }

        //////////////////////////////////////////////////////////////

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.HideSelection = false;
        }

        private async void crawlButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(HTMLDirectory) == false)
                {
                    Directory.CreateDirectory(HTMLDirectory);
                }
                //            new PaperCrawlerLib.CrawlKesen().Crawl(HTMLDirectory, log);
                new PaperCrawlerLib.CrawlUIST().Crawl(HTMLDirectory, log);
            });
        }

        private async void scrapeButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(JSONDirectory) == false)
                {
                    Directory.CreateDirectory(JSONDirectory);
                }
                //      var kesenPapers = new PaperCrawlerLib.CrawlKesen().Scrape(HTMLDirectory, log);
                //      savePapers(kesenPapers, JSONDirectory);
                var uistPapers = new PaperCrawlerLib.CrawlUIST().Scrape(HTMLDirectory, log);
                savePapers(uistPapers, JSONDirectory);
            });
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

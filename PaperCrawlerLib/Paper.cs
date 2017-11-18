using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperCrawlerLib
{
    public class Paper
    {
        public string Conference { get; set; }
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public string OfficialSiteURL { get; set; }
        public string[] OtherURLs { get; set; }
        public string Description { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementsClassLibrary
{
    public class LiteratureReference
    {
        //[Article], [Source], [Vol], [Num], [DOI], [Page1], [Page2], [Year1], [Year2]
        public string Article { get; set; }
        public string Source { get; set; }
        public string Vol { get; set; }
        public string Num { get; set; }
        public string DOI { get; set; }
        public string Page1 { get; set; }
        public string Page2 { get; set; }
        public string Year1 { get; set; }
        public string Year2 { get; set; }
        public List<string> Authors { get; set; }

        public override string ToString()
        {
            string authors = (Authors == null ? "" : string.Join(" ", Authors));
            string article = (string.IsNullOrEmpty(Article) ? "" : Article);
            string source = (string.IsNullOrEmpty(Source) ? "" : $" / {Source}");
            string vol = (string.IsNullOrEmpty(Vol) ? "" : $", {Vol}");
            string num = (string.IsNullOrEmpty(Num) ? "" : $", {Num}");
            string doi = (string.IsNullOrEmpty(DOI) ? "" : $", {DOI}");
            string page1 = (string.IsNullOrEmpty(Page1) ? "" : $", {Page1}");
            string page2 = (string.IsNullOrEmpty(Page2) ? "" : $"-{Page2}");
            string year1 = (string.IsNullOrEmpty(Year1) ? "" : $", {Year1}");
            string year2 = (string.IsNullOrEmpty(Year2) ? "" : $"-{Year2}");

            return $"{authors}\n{article}{source}{vol}{num}{doi}{page1}{page2}{year1}{year2}";
        }
    }
}

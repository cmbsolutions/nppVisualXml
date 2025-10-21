using System.Collections.Generic;

namespace nppVisualXml.Storage.Models
{
    public class SettingsModel
    {
        public string Appname { get; set; }
        public string Appversion { get; set; }
        public Toolstrip1 ToolStrip1 { get; set; }
    }

    public class Toolstrip1
    {
        public bool TsbCaseSensitive { get; set; }
        public bool TsbRegex { get; set; }
        public Tscbosearch TscboSearch { get; set; }
    }

    public class Tscbosearch
    {
        public List<History> History { get; set; }
    }

    public class History
    {
        public bool CaseSensitive { get; set; }
        public bool Regex { get; set; }
        public string SearchText { get; set; }

        public override string ToString()
        {
            return SearchText;
        }
    }

}



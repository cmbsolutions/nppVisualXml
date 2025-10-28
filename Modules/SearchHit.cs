using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    public sealed class SearchHit
    {
        public XObject XObj;          // element / attribute / text / comment / cdata / PI
        public string Path;           // breadcrumb without namespaces
        public string Preview;        // short value/name preview
        public int Line;              // 1-based (if available)
        public int Col;

        public SearchHit(XObject x)
        {
            XObj = x;
            var li = x as IXmlLineInfo;
            if (li != null && li.HasLineInfo())
            {
                Line = li.LineNumber; Col = li.LinePosition;
            }

            Path = BuildPath(x);
            Preview = BuildPreview(x);
        }

        private static string BuildPath(XObject x)
        {
            var parts = new System.Collections.Generic.List<string>();
            XElement el =
                x as XElement ??
                (x as XAttribute)?.Parent ??
                (x as XText)?.Parent ??
                (x as XContainer)?.Parent;

            for (var cur = el; cur != null; cur = cur.Parent)
            {
                // 1-based sibling index among same local-name elements
                int idx = 1;
                if (cur.Parent != null)
                {
                    foreach (var sib in cur.Parent.Elements(cur.Name))
                    {
                        if (sib == cur) break;
                        idx++;
                    }
                }
                parts.Add($"{cur.Name.LocalName}[{idx}]");
            }
            parts.Reverse();

            // refine last segment for specific node types
            if (x is XAttribute xa) parts.Add("@" + xa.Name.LocalName);
            else if (x is XText) parts.Add("#text");
            else if (x is XCData) parts.Add("#cdata");
            else if (x is XComment) parts.Add("#comment");
            else if (x is XProcessingInstruction pi) parts.Add("?" + pi.Target);

            return string.Join("/", parts);
        }

        private static string BuildPreview(XObject x)
        {
            const int Max = 80;
            string s = "";
            if (x is XElement xe) s = $"<{xe.Name.LocalName}>";
            else if (x is XAttribute xa) s = $"@{xa.Name.LocalName}={xa.Value}";
            else if (x is XText xt) s = xt.Value;
            else if (x is XCData cd) s = cd.Value;
            else if (x is XComment cm) s = cm.Value;
            else if (x is XProcessingInstruction pi) s = $"{pi.Target} {pi.Data}";
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= Max ? s : s.Substring(0, Max) + "…";
        }
    }
}

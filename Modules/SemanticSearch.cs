using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    internal static class SemanticSearch
    {
        public static StructuredQuery ParseStructuredQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            // Prefer '==' over '='
            int idx = query.IndexOf("==", StringComparison.Ordinal);
            bool exact = false;
            string op = null;

            if (idx >= 0) { exact = true; op = "=="; }
            else
            {
                idx = query.IndexOf('='); // single '='
                if (idx >= 0) op = "=";
            }

            if (op == null) return null;

            string pathPart = query.Substring(0, idx).Trim();
            string valuePart = query.Substring(idx + op.Length).Trim();

            if (pathPart.Length == 0 || valuePart.Length == 0) return null;

            var segments = pathPart.Split('.')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            if (segments.Count == 0) return null;

            return new StructuredQuery(segments, exact, valuePart);
        }


        private static bool LocalEq(XName n, string s) => n.LocalName.Equals(s, StringComparison.OrdinalIgnoreCase);

        public static IEnumerable<XObject> StructuredSearch(XDocument xdoc, StructuredQuery q)
        {
            if (xdoc?.Root == null || q == null) yield break;

            // Start from anywhere matching the FIRST segment
            IEnumerable<XElement> current = xdoc
                .Descendants()
                .Where(e => LocalEq(e.Name, q.Path[0]));

            // Walk intermediate segments (second .. second-to-last), direct child each step
            for (int i = 1; i < q.Path.Count - 1; i++)
            {
                string seg = q.Path[i];
                current = current.SelectMany(e => e.Elements()
                                        .Where(ch => LocalEq(ch.Name, seg)));
            }

            // Last segment: target leaf elements whose text we test
            string leaf = q.Path[q.Path.Count - 1];

            foreach (var parent in current)
            {
                var leaves = parent.Elements().Where(e => LocalEq(e.Name, leaf));
                foreach (var leafEl in leaves)
                {
                    string text = (leafEl.Value ?? string.Empty).Trim();
                    if (q.Exact)
                    {
                        if (text.Equals(q.Value, StringComparison.OrdinalIgnoreCase))
                            yield return leafEl;
                    }
                    else
                    {
                        if (text.IndexOf(q.Value, StringComparison.OrdinalIgnoreCase) >= 0)
                            yield return leafEl;
                    }
                }
            }
        }
    }
}
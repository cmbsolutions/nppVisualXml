using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    internal class XmlTreeFiller
    {
        public static void LoadXmlIntoTree(TreeView tree, string xmlText)
        {
            tree.BeginUpdate();
            try
            {
                tree.Nodes.Clear();
                if (string.IsNullOrWhiteSpace(xmlText)) return;

                var xdoc = XDocument.Parse(
                    xmlText,
                    LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo
                );

                if (xdoc.Root == null) return;

                var rootNode = CreateElementNode(xdoc.Root);
                tree.Nodes.Add(rootNode);
                rootNode.Expand();
            }
            catch (Exception ex)
            {
                tree.Nodes.Clear();
                tree.Nodes.Add(new TreeNode($"⚠ Not valid XML: {ex.Message}"));
            }
            finally
            {
                tree.EndUpdate();
            }
        }

        private static TreeNode CreateElementNode(XElement el)
        {
            var tn = new TreeNode(GetElementDisplay(el)) { Tag = el };

            // Attributes first
            foreach (var attr in el.Attributes())
                tn.Nodes.Add(new TreeNode($"@{attr.Name} = \"{Truncate(attr.Value, 80)}\"") { Tag = attr });

            // Child content
            foreach (var n in el.Nodes())
            {
                switch (n.NodeType)
                {
                    case XmlNodeType.Element:
                        tn.Nodes.Add(CreateElementNode((XElement)n));
                        break;

                    case XmlNodeType.Text:
                        var t = (XText)n;
                        var v = t.Value;
                        if (!string.IsNullOrWhiteSpace(v))
                            tn.Nodes.Add(new TreeNode($"text: {Truncate(v, 120)}") { Tag = t });
                        break;

                    case XmlNodeType.CDATA:
                        var cdata = (XCData)n;
                        tn.Nodes.Add(new TreeNode($"<![CDATA[{Truncate(cdata.Value, 120)}]]>") { Tag = cdata });
                        break;

                    case XmlNodeType.Comment:
                        var com = (XComment)n;
                        tn.Nodes.Add(new TreeNode($"<!-- {Truncate(com.Value, 120)} -->") { Tag = com });
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        var pi = (XProcessingInstruction)n;
                        tn.Nodes.Add(new TreeNode($"<?{pi.Target} {Truncate(pi.Data, 80)}?>") { Tag = pi });
                        break;

                    default:
                        // ignore whitespace, etc.
                        break;
                }
            }

            return tn;
        }

        // Call this instead of CreateElementNode when building the tree
        private static TreeNode CreateElementNodeLazy(XElement el)
        {
            var tn = new TreeNode(GetElementDisplay(el)) { Tag = el };
            // attributes now
            foreach (var attr in el.Attributes())
                tn.Nodes.Add(new TreeNode($"@{attr.Name} = \"{Truncate(attr.Value, 80)}\"") { Tag = attr });

            // add a placeholder if there are element children or interesting nodes
            bool hasChildren = el.Nodes().Any(n =>
                n.NodeType == XmlNodeType.Element ||
                n.NodeType == XmlNodeType.Text ||
                n.NodeType == XmlNodeType.CDATA ||
                n.NodeType == XmlNodeType.Comment ||
                n.NodeType == XmlNodeType.ProcessingInstruction);

            if (hasChildren)
                tn.Nodes.Add(new TreeNode("…")); // placeholder

            return tn;
        }

        private static string GetElementDisplay(XElement el)
        {
            var name = el.Name.ToString();
            var attrsPreview = el.Attributes().Take(3)
                .Select(a => $"{a.Name}=\"{Truncate(a.Value, 20)}\"");
            var attrsStr = string.Join(" ", attrsPreview);

            string pos = "";
            if (el is IXmlLineInfo li && li.HasLineInfo())
                pos = $"  (line {li.LineNumber}, col {li.LinePosition})";

            return attrsStr.Length > 0 ? $"<{name} {attrsStr}>{pos}" : $"<{name}>{pos}";
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "…";
        }
    }
}

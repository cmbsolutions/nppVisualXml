using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    internal static class XmlTreeFiller
    {
        private const bool ShowNamespaces = false;

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
            var li = GetElementDisplay(el);
            var tn = new TreeNode(li.Name)
            {
                Tag = el,
                Name = $"{li.LineNo}:{li.LinePosition}"
            };

            // Attributes first
            foreach (var attr in el.Attributes())
            {
                if (attr.IsNamespaceDeclaration) continue;
                tn.Nodes.Add(new TreeNode($"@{QName(attr.Name)} = \"{Truncate(attr.Value, 80)}\"") { Tag = attr });
            }
                

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

        private static XmlLineInfo GetElementDisplay(XElement el)
        {
            XmlLineInfo lineInfo = new XmlLineInfo();
            var name = QName(el.Name);
            var attrsPreview = el.Attributes()
                .Where(a => !a.IsNamespaceDeclaration)
                .Take(3)
                .Select(a => $"{QName(a.Name)}=\"{Truncate(a.Value, 20)}\"");
            var attrsStr = string.Join(" ", attrsPreview);

            lineInfo.Name = name;

            if (el is IXmlLineInfo li && li.HasLineInfo())
            {
                lineInfo.LineNo = li.LineNumber;
                lineInfo.LinePosition = li.LinePosition;
            }

            return lineInfo;
        }

        private static string QName(XName name)
        => ShowNamespaces ? name.ToString() : name.LocalName;

        private static string Truncate(string s, int maxLen)
               => string.IsNullOrEmpty(s) || s.Length <= maxLen ? s : s.Substring(0, maxLen) + "…";
    }
}

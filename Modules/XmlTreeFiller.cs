using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace nppVisualXml.Modules
{
    internal static class XmlTreeFiller
    {
        private const bool ShowNamespaces = false;
        public static readonly string PlaceholderText = "…";
        private const int ChunkSize = 200;          // nodes per UI batch
        private const int MaxPreviewLen = 80;       // text/cdata preview length

        private static XDocument _xdoc;

        public static void LoadXmlIntoTree(TreeView tree, string xmlText)
        {
            tree.BeginUpdate();
            try
            {
                tree.Nodes.Clear();
                if (string.IsNullOrWhiteSpace(xmlText)) return;

                _xdoc = XDocument.Parse(
                    xmlText,
                    LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo
                );

                if (_xdoc.Root == null) return;

                var rootNode = CreateElementNodeLazy(_xdoc.Root);
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

        private static TreeNode CreateElementNodeLazy(XElement el)
        {
            var tn = new TreeNode(QName(el.Name)) { Tag = el };
            TreeNodeIndex.Register(tn, el);

            // (Optional) group attributes under a single node for fewer items
            var attrs = el.Attributes().Where(a => !a.IsNamespaceDeclaration).ToList();
            if (attrs.Count > 0)
            {
                var attrsNode = new TreeNode($"@attributes ({attrs.Count})") { Tag = attrs };
                MakeExpandable(attrsNode); // lazy too
                tn.Nodes.Add(attrsNode);
            }

            // If element *might* have content, add placeholder to lazy load
            if (HasInterestingChildren(el))
                MakeExpandable(tn);

            return tn;
        }

        private static bool HasInterestingChildren(XElement el)
        {
            foreach (var n in el.Nodes())
            {
                switch (n.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                        return true;
                }
            }
            return false;
        }

        public static async Task PopulateAttributesAsync(TreeNode parent, List<XAttribute> attrs)
        {
            // Build TreeNode objects off the UI thread (cheap, no handles yet)
            var built = await Task.Run(() =>
            {
                var list = new List<TreeNode>(attrs.Count);
                foreach (var a in attrs)
                {
                    list.Add(new TreeNode($"@{a.Name.LocalName} = \"{Truncate(a.Value, MaxPreviewLen)}\"") { Tag = a });
                }

                foreach (var n in list) TreeNodeIndex.Register(n, (XObject)n.Tag);

                return list;
            });

            AddInChunks(parent, built);
        }

        public static async Task PopulateElementChildrenAsync(TreeNode parent, XElement el)
        {
            var built = await Task.Run(() =>
            {
                var list = new List<TreeNode>();

                foreach (var n in el.Nodes())
                {
                    switch (n.NodeType)
                    {
                        case XmlNodeType.Element:
                            list.Add(CreateElementNodeLazy((XElement)n));
                            break;

                        case XmlNodeType.Text:
                            var t = (XText)n;
                            if (!string.IsNullOrWhiteSpace(t.Value))
                                list.Add(new TreeNode($"text: {Truncate(t.Value, MaxPreviewLen)}") { Tag = t });
                            break;

                        case XmlNodeType.CDATA:
                            var c = (XCData)n;
                            list.Add(new TreeNode($"<![CDATA[{Truncate(c.Value, MaxPreviewLen)}]]>") { Tag = c });
                            break;

                        case XmlNodeType.Comment:
                            var com = (XComment)n;
                            list.Add(new TreeNode($"<!-- {Truncate(com.Value, MaxPreviewLen)} -->") { Tag = com });
                            break;

                        case XmlNodeType.ProcessingInstruction:
                            var pi = (XProcessingInstruction)n;
                            list.Add(new TreeNode($"<?{pi.Target} {Truncate(pi.Data, MaxPreviewLen)}?>") { Tag = pi });
                            break;
                    }
                }
                foreach (var n in list) TreeNodeIndex.Register(n, (XObject)n.Tag);
                return list;
            });

            AddInChunks(parent, built);
        }

        public static void AddInChunks(TreeNode parent, List<TreeNode> built)
        {
            var tree = parent.TreeView;
            if (built.Count == 0) return;

            // Turn off redraw to avoid flicker and speed up
            SetRedraw(tree, false);
            try
            {
                for (int i = 0; i < built.Count; i += ChunkSize)
                {
                    int take = Math.Min(ChunkSize, built.Count - i);
                    var slice = built.GetRange(i, take).ToArray();

                    tree.BeginUpdate();
                    parent.Nodes.AddRange(slice);
                    tree.EndUpdate();
                }
            }
            finally
            {
                SetRedraw(tree, true);
            }
        }

        private const string PlaceholderKey = "__npp_xml_placeholder__";

        private static void MakeExpandable(TreeNode parent)
        {
            parent.Nodes.Add(new TreeNode { Name = PlaceholderKey, Text = "..." }); // invisible-ish
        }

        public static bool NeedsPopulate(TreeNode node)
        {
            return node.Nodes.Count == 1 && node.Nodes[0].Name == PlaceholderKey;
        }

        public static TreeNode FindPlaceholderChild(TreeNode node)
        {
            foreach (TreeNode c in node.Nodes)
                if (c.Name == PlaceholderKey) return c;
            return null;
        }

        public static IEnumerable<XObject> FindMatchesInXDoc(string query, bool caseSensitive, bool useRegex)
        {
            if (_xdoc?.Root == null || string.IsNullOrEmpty(query)) yield break;

            // Prepare matchers
            Func<string, bool> isMatch;
            if (useRegex)
            {
                var opts = caseSensitive ? RegexOptions.CultureInvariant : (RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var re = new Regex(query, opts);
                isMatch = s => re.IsMatch(s ?? string.Empty);
            }
            else
            {
                var cmp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                isMatch = s => (s ?? string.Empty).IndexOf(query, cmp) >= 0;
            }

            // Walk elements
            foreach (var el in _xdoc.Descendants())
            {
                // element name
                if (isMatch(el.Name.LocalName)) yield return el;

                // attributes (name or value)
                foreach (var a in el.Attributes().Where(a => !a.IsNamespaceDeclaration))
                {
                    if (isMatch(a.Name.LocalName) || isMatch(a.Value)) yield return a;
                }

                // text nodes (skip pure whitespace)
                foreach (var t in el.Nodes().OfType<XText>())
                {
                    var v = t.Value;
                    if (!string.IsNullOrWhiteSpace(v) && isMatch(v)) yield return t;
                }

                // comments / cdata / PI if you wish
                foreach (var c in el.Nodes().OfType<XComment>())
                    if (isMatch(c.Value)) yield return c;

                foreach (var cd in el.Nodes().OfType<XCData>())
                    if (isMatch(cd.Value)) yield return cd;

                foreach (var pi in el.Nodes().OfType<XProcessingInstruction>())
                    if (isMatch(pi.Target) || isMatch(pi.Data)) yield return pi;
            }
        }



        // Small native helper to suspend/resume redraw
        private const int WM_SETREDRAW = 0x000B;
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private static void SetRedraw(Control c, bool enable)
        {
            if (!c.IsHandleCreated) return;
            SendMessage(c.Handle, WM_SETREDRAW, (IntPtr)(enable ? 1 : 0), IntPtr.Zero);
            if (enable) c.Invalidate();
        }

        private static string QName(XName name)
        => ShowNamespaces ? name.ToString() : name.LocalName;

        private static string Truncate(string s, int maxLen)
               => string.IsNullOrEmpty(s) || s.Length <= maxLen ? s : s.Substring(0, maxLen) + "…";
    }
}

using Kbg.NppPluginNET.PluginInfrastructure;
using nppVisualXml.Modules;
using nppVisualXml.Storage;
using nppVisualXml.Storage.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Kbg.NppPluginNET
{
    public partial class XmlViewer : Form
    {
        private IScintillaGateway Editor;
        private INotepadPPGateway Notepad;

        public Settings settings { get; set; }


        public XmlViewer()
        {
            InitializeComponent();
            this.Editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            this.Notepad = new NotepadPPGateway();
            tvXml.BeginUpdate();
            tvXml.EndUpdate();
            tvXml.NodeMouseClick += TvXml_NodeMouseClick;
            tvXml.BeforeExpand += TvXml_BeforeExpand;
            TreeSearchOwnerDraw.Attach(tvXml);
        }

        public void LoadSettings()
        {
            tsbCaseSensitive.Checked = settings.settings.ToolStrip1.TsbCaseSensitive;
            tsbRegex.Checked = settings.settings.ToolStrip1.TsbRegex;

            for (int i = 0; i < settings.settings.ToolStrip1.TscboSearch.History.Count; i++)
            {
                tscboSearch.Items.Add(settings.settings.ToolStrip1.TscboSearch.History[i]);
            }
        }

        private void SaveSettings()
        {
            settings.settings.ToolStrip1.TsbCaseSensitive = tsbCaseSensitive.Checked;
            settings.settings.ToolStrip1.TsbRegex = tsbRegex.Checked;

            settings.Save();
        }

        private async void TvXml_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var ph = XmlTreeFiller.FindPlaceholderChild(e.Node);
            if (ph == null) return; // already populated for this node

            // clear placeholder
            e.Node.Nodes.Remove(ph);

            if (e.Node.Tag is List<XAttribute> attrs)
            {
                await XmlTreeFiller.PopulateAttributesAsync(e.Node, attrs);
            }
            else if (e.Node.Tag is XElement el)
            {
                await XmlTreeFiller.PopulateElementChildrenAsync(e.Node, el);
            }

            if (e.Node.Nodes.Count > 0 && !e.Node.IsExpanded)
            {
                e.Node.Expand();
            }
        }

        private void XmlViewer_Load(object sender, EventArgs e)
        {
            RefreshFromActiveDoc();
        }

        public void RefreshFromActiveDoc()
        {
            var xml = NppText.GetActiveDocumentText();
            XmlTreeFiller.LoadXmlIntoTree(tvXml, xml);
        }

        private void tsbCollapse_Click(object sender, EventArgs e)
        {
            if (tvXml.Nodes.Count > 0)
            {
                tvXml.CollapseAll();
            }
        }

        private void tsbExpand_Click(object sender, EventArgs e)
        {
            if (tvXml.Nodes.Count > 0)
            {
                tvXml.ExpandAll();
            }
        }

        private void tsbReload_Click(object sender, EventArgs e)
        {
            RefreshFromActiveDoc();
        }

        private void TvXml_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!TryGetLineColFromTag(e.Node, out int line1, out int col1))
                return;

            JumpToLineCol(line1, col1);
        }

        private static bool TryGetLineColFromTag(TreeNode node, out int line1, out int col1)
        {
            line1 = 0; col1 = 0;
            if (node.Tag is System.Xml.IXmlLineInfo li && li.HasLineInfo())
            {
                line1 = li.LineNumber;
                col1 = li.LinePosition;
                return line1 > 0 && col1 > 0;
            }
            return false;
        }

        private static void JumpToLineCol(int line1, int col1)
        {
            // Notepad++ / Scintilla are 0-based for lines and byte-based for positions.
            int line0 = Math.Max(0, line1 - 1);
            int colChars = Math.Max(0, col1 - 1);

            var editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());

            // Start of the target line (byte position)
            int lineStartPos = editor.PositionFromLine(line0);

            // Get the line text to convert character column -> UTF-8 byte offset
            string lineText = editor.GetLine(line0) ?? string.Empty;

            // Bound the character column to the line length (in characters)
            if (colChars > lineText.Length) colChars = lineText.Length;

            // Compute byte offset for the first 'colChars' characters in UTF-8
            int byteOffset = Encoding.UTF8.GetByteCount(lineText.Substring(0, colChars));

            int targetPos = lineStartPos + byteOffset;

            // Move caret, collapse selection, and scroll into view
            editor.SetSel(targetPos, targetPos);
            editor.ScrollCaret();
        }

        private void tsbSearch_Click(object sender, EventArgs e)
        {
            var qText = tscboSearch.Text.Trim();
            bool caseSensitive = tsbCaseSensitive.Checked;
            bool useRegex = tsbRegex.Checked;
            
            if (useRegex)
            {
                try
                {
                    var options = caseSensitive ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                    var re = new System.Text.RegularExpressions.Regex(qText, options | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Invalid regular expression:\n{ex.Message}", "Regex Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var sq = SemanticSearch.ParseStructuredQuery(qText);
            var hits = (sq != null)
                ? SemanticSearch.StructuredSearch(XmlTreeFiller.GetXmlDocument(), sq).ToList()
                : XmlTreeFiller.FindMatchesInXDoc(qText, caseSensitive, useRegex).ToList();

            NewSearchHistoryItem(qText, caseSensitive, useRegex);
            
            int visibleCount = 0;
            tvXml.BeginUpdate();
            try
            {
                foreach (var x in hits)
                {
                    var node = EnsureVisibleNodeFor(x);
                    if (node != null)
                    {
                        // Make sure the node is visible
                        node.EnsureVisible();
                        visibleCount++;
                    }
                }
            }
            finally { tvXml.EndUpdate(); }

            string highlightTerm = (sq != null) ? sq.Value : qText;
            int matchCount = TreeSearchOwnerDraw.SearchAndHighlight(tvXml, highlightTerm, caseSensitive, useRegex);

            tXPath.Text = matchCount == 1 ? "1 match" : $"{matchCount} matches";
        }

        private TreeNode EnsureVisibleNodeFor(XObject target)
        {
            if (target == null) return null;

            // Figure out the element that should contain the node we want to see
            XElement targetElement = target as XElement
                ?? (target as XAttribute)?.Parent
                ?? (target as XText)?.Parent
                ?? (target as XContainer)?.Parent
                ?? null;

            if (targetElement == null) return null;

            // Build ancestor chain from root -> targetElement
            var chain = new Stack<XElement>();
            for (var cur = targetElement; cur != null; cur = cur.Parent)
                chain.Push(cur);

            // Start at TreeView root
            if (tvXml.Nodes.Count == 0) return null;
            TreeNode curNode = tvXml.Nodes[0];

            // If root doesn't match, bail
            if (!(curNode.Tag is XElement rootEl) || rootEl != chain.Peek())
                return null;

            // Walk down the chain, expanding as needed
            while (chain.Count > 0)
            {
                var want = chain.Pop();

                // curNode corresponds to 'want' already — ensure its children are populated
                ExpandNodeIfPlaceholder(curNode);

                if (chain.Count == 0) break; // we are at targetElement, stop descending

                // Find child element node matching the next XElement in chain
                var nextEl = chain.Peek();


                // If we’ve already materialized it, use the index
                if (TreeNodeIndex.TryGetNode(nextEl, out TreeNode nextNode))
                {
                    curNode = nextNode;
                    continue;
                }

                // Otherwise, search children (after lazy expansion)
                foreach (TreeNode child in curNode.Nodes)
                {
                    if (child.Tag is XElement x && x == nextEl)
                    {
                        nextNode = child;
                        break;
                    }
                }

                if (nextNode == null) return null; // should not happen
                curNode = nextNode;
            }

            // At targetElement node; now locate the actual target node:
            if (target is XElement)
            {
                return curNode; // already at element node
            }
            else if (target is XAttribute xa)
            {
                // find @attributes group, expand, then find the attribute node
                TreeNode attrsGroup = null;
                foreach (TreeNode child in curNode.Nodes)
                    if (child.Tag is List<XAttribute>) { attrsGroup = child; break; }

                if (attrsGroup == null) return null;

                ExpandNodeIfPlaceholder(attrsGroup);

                foreach (TreeNode child in attrsGroup.Nodes)
                    if (child.Tag is XAttribute a && a == xa)
                        return child;

                return null;
            }
            else if (target is XText xt)
            {
                // expand current element and find text node
                ExpandNodeIfPlaceholder(curNode);
                foreach (TreeNode child in curNode.Nodes)
                    if (child.Tag is XText t && t == xt)
                        return child;
                return null;
            }
            else if (target is XCData cd)
            {
                ExpandNodeIfPlaceholder(curNode);
                foreach (TreeNode child in curNode.Nodes)
                    if (child.Tag is XCData c && c == cd)
                        return child;
                return null;
            }
            else if (target is XComment com)
            {
                ExpandNodeIfPlaceholder(curNode);
                foreach (TreeNode child in curNode.Nodes)
                    if (child.Tag is XComment c && c == com)
                        return child;
                return null;
            }
            else if (target is XProcessingInstruction pi)
            {
                ExpandNodeIfPlaceholder(curNode);
                foreach (TreeNode child in curNode.Nodes)
                    if (child.Tag is XProcessingInstruction p && p == pi)
                        return child;
                return null;
            }

            return null;
        }

        private void ExpandNodeIfPlaceholder(TreeNode node)
        {
            var ph = XmlTreeFiller.FindPlaceholderChild(node);
            if (ph == null) return; // already populated

            // Remove just the sentinel
            node.Nodes.Remove(ph);

            // Populate synchronously here (no async/await)
            if (node.Tag is List<XAttribute> attrs)
            {
                XmlTreeFiller.PopulateAttributes(node, attrs);
            }
            else if (node.Tag is XElement el)
            {
                XmlTreeFiller.PopulateElementChildren(node, el);
            }

            // Now the real children exist; open it
            if (!node.IsExpanded) node.Expand();
        }

        private void NewSearchHistoryItem(string text, bool cc, bool regex)
        {
            History tmp = new History();
            tmp.SearchText = text;
            tmp.CaseSensitive = cc;
            tmp.Regex = regex;
            settings.settings.ToolStrip1.TscboSearch.History.RemoveAll(h => h.SearchText == text && h.CaseSensitive == cc && h.Regex == regex);
            settings.settings.ToolStrip1.TscboSearch.History.Insert(0,tmp);
            settings.settings.ToolStrip1.TscboSearch.History = settings.settings.ToolStrip1.TscboSearch.History.Take(10).ToList();
            tscboSearch.Items.Clear();
            tscboSearch.Items.AddRange(settings.settings.ToolStrip1.TscboSearch.History.ToArray());
            settings.Save();
        }

        private void tscboSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            History history = (History)tscboSearch.SelectedItem;
            tsbCaseSensitive.Checked = history.CaseSensitive;
            tsbRegex.Checked = history.Regex;
        }
    }
}

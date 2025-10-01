using Kbg.NppPluginNET.PluginInfrastructure;
using nppVisualXml.Modules;
using nppVisualXml.Storage;
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
            foreach (nppVisualXml.Storage.Models.ConfigItem configitem in settings.settings.ConfigItems)
            {
                if (configitem == null || configitem.Name.StartsWith("Quick")) { continue; }   

                Control ctrl = this.Controls.Find(configitem.Name, true).FirstOrDefault();

                if (ctrl != null && ctrl.Name.StartsWith("NumericUpDown"))
                {
                    NumericUpDown nupdown = ctrl as NumericUpDown;
                    nupdown.Value = Math.Min(Convert.ToDecimal(configitem.Value), nupdown.Maximum);
                }
                if (ctrl != null && ctrl.Name.StartsWith("Checkbox"))
                {
                    System.Windows.Forms.CheckBox check = ctrl as System.Windows.Forms.CheckBox;
                    check.Checked = Convert.ToBoolean(configitem.Value);
                    //TriggerCheckBoxChangeEvent(check);
                }
                if (ctrl != null && ctrl.Name.StartsWith("Textbox"))
                {
                    TextBox txt = ctrl as TextBox;
                    txt.Text = configitem.Value;
                }
                if (ctrl != null && ctrl.Name.StartsWith("Radio"))
                {
                    System.Windows.Forms.RadioButton radio = ctrl as System.Windows.Forms.RadioButton;
                    radio.Checked = Convert.ToBoolean(configitem.Value);
                }
                if (ctrl != null && ctrl.Name.StartsWith("TabControl"))
                {
                    System.Windows.Forms.TabControl tab = ctrl as System.Windows.Forms.TabControl;
                    tab.SelectedTab = tab.TabPages[Convert.ToInt32(configitem.Value)];
                }
                if (ctrl != null && ctrl.Name.StartsWith("ComboBox"))
                {
                    ComboBox combo = ctrl as ComboBox;
                    combo.Text = configitem.Value;
                }
            }
        }

        private void SaveSettings()
        {
            foreach (nppVisualXml.Storage.Models.ConfigItem configitem in settings.settings.ConfigItems)
            {
                if (configitem.Name.StartsWith("Quick")) { continue; }

                Control ctrl = this.Controls.Find(configitem.Name, true).FirstOrDefault();

                if (ctrl != null && ctrl.Name.StartsWith("NumericUpDown"))
                {
                    NumericUpDown nupdown = ctrl as NumericUpDown;
                    configitem.Value = nupdown.Value.ToString();
                }
                if (ctrl != null && ctrl.Name.StartsWith("Checkbox"))
                {
                    System.Windows.Forms.CheckBox check = ctrl as System.Windows.Forms.CheckBox;
                    configitem.Value = (check.Checked ? "true" : "false");
                }
                if (ctrl != null && ctrl.Name.StartsWith("Textbox"))
                {
                    TextBox txt = ctrl as TextBox;
                    configitem.Value = txt.Text;
                }
                if (ctrl != null && ctrl.Name.StartsWith("Radio"))
                {
                    System.Windows.Forms.RadioButton radio = ctrl as System.Windows.Forms.RadioButton;
                    configitem.Value = (radio.Checked ? "true" : "false");
                }
                if (ctrl != null && ctrl.Name.StartsWith("TabControl"))
                {
                    System.Windows.Forms.TabControl tab = ctrl as System.Windows.Forms.TabControl;
                    configitem.Value = tab.SelectedIndex.ToString();
                }
                if (ctrl != null && ctrl.Name.StartsWith("ComboBox"))
                {
                    ComboBox combo = ctrl as ComboBox;
                    configitem.Value = combo.Text;
                }
            }

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

            // Optional: flash/select a token width for visibility (e.g., 1 char)
            // int nextByte = byteOffset + (colChars < lineText.Length ? Encoding.UTF8.GetByteCount(lineText.AsSpan(colChars, 1)) : 0);
            // editor.SetSel(targetPos, lineStartPos + nextByte);
            // editor.ScrollCaret();
        }

        private void tsbSearch_Click(object sender, EventArgs e)
        {
            var q = tstSearch.Text;
            bool caseSensitive = tsbCaseSensitive.Checked; // if you have this
            bool useRegex = tsbRegex.Checked;              // if you have this

            var hits = XmlTreeFiller.FindMatchesInXDoc(q, caseSensitive, useRegex).ToList();
            
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

            // 3) Now run your substring highlighter over the whole tree.
            //    This computes correct ranges based on the actual node.Text.
            int matchCount = TreeSearchOwnerDraw.SearchAndHighlight(tvXml, q, caseSensitive, useRegex);

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

                TreeNode nextNode = null;

                // If we’ve already materialized it, use the index
                if (TreeNodeIndex.TryGetNode(nextEl, out nextNode))
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
            if (ph != null)
            {
                // trigger your lazy populate synchronously
                tvXml.BeginUpdate();
                try
                {
                    node.Expand(); // your BeforeExpand removes placeholder and fills real children
                }
                finally { tvXml.EndUpdate(); }
            }
        }
    }
}

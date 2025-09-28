using Kbg.NppPluginNET.PluginInfrastructure;
using nppVisualXml.Modules;
using nppVisualXml.Storage;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

            int hits = TreeSearchOwnerDraw.SearchAndHighlight(tvXml, q, caseSensitive, useRegex);
            tXPath.Text = hits == 1 ? "1 match" : $"{hits} matches";
        }
    }
}

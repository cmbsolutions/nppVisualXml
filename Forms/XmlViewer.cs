using Kbg.NppPluginNET.PluginInfrastructure;
using nppVisualXml.Modules;
using nppVisualXml.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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

    }
}

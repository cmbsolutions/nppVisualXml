namespace Kbg.NppPluginNET
{
    partial class XmlViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlViewer));
            this.tvXml = new System.Windows.Forms.TreeView();
            this.tlpFrames = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbCollapse = new System.Windows.Forms.ToolStripButton();
            this.tsbExpand = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tscboSearch = new System.Windows.Forms.ToolStripComboBox();
            this.tsbSearch = new System.Windows.Forms.ToolStripButton();
            this.tsbCaseSensitive = new System.Windows.Forms.ToolStripButton();
            this.tsbRegex = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbReload = new System.Windows.Forms.ToolStripButton();
            this.tXPath = new System.Windows.Forms.TextBox();
            this.lvResults = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tlpFrames.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvXml
            // 
            this.tvXml.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvXml.Location = new System.Drawing.Point(3, 28);
            this.tvXml.Name = "tvXml";
            this.tvXml.ShowNodeToolTips = true;
            this.tvXml.Size = new System.Drawing.Size(388, 349);
            this.tvXml.TabIndex = 0;
            // 
            // tlpFrames
            // 
            this.tlpFrames.ColumnCount = 1;
            this.tlpFrames.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFrames.Controls.Add(this.tvXml, 0, 1);
            this.tlpFrames.Controls.Add(this.toolStrip1, 0, 0);
            this.tlpFrames.Controls.Add(this.tXPath, 0, 3);
            this.tlpFrames.Controls.Add(this.lvResults, 0, 2);
            this.tlpFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFrames.Location = new System.Drawing.Point(0, 0);
            this.tlpFrames.Name = "tlpFrames";
            this.tlpFrames.RowCount = 4;
            this.tlpFrames.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpFrames.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpFrames.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpFrames.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpFrames.Size = new System.Drawing.Size(394, 561);
            this.tlpFrames.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbCollapse,
            this.tsbExpand,
            this.toolStripSeparator1,
            this.tscboSearch,
            this.tsbSearch,
            this.tsbCaseSensitive,
            this.tsbRegex,
            this.toolStripSeparator2,
            this.tsbReload});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(394, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbCollapse
            // 
            this.tsbCollapse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCollapse.Image = global::nppVisualXml.Properties.Resources.button_blue_remove;
            this.tsbCollapse.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCollapse.Name = "tsbCollapse";
            this.tsbCollapse.Size = new System.Drawing.Size(23, 22);
            this.tsbCollapse.Text = "Collapse";
            this.tsbCollapse.Click += new System.EventHandler(this.tsbCollapse_Click);
            // 
            // tsbExpand
            // 
            this.tsbExpand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbExpand.Image = global::nppVisualXml.Properties.Resources.button_blue_add;
            this.tsbExpand.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExpand.Name = "tsbExpand";
            this.tsbExpand.Size = new System.Drawing.Size(23, 22);
            this.tsbExpand.Text = "Expand";
            this.tsbExpand.Click += new System.EventHandler(this.tsbExpand_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tscboSearch
            // 
            this.tscboSearch.Name = "tscboSearch";
            this.tscboSearch.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.tscboSearch.Size = new System.Drawing.Size(200, 25);
            this.tscboSearch.SelectedIndexChanged += new System.EventHandler(this.tscboSearch_SelectedIndexChanged);
            // 
            // tsbSearch
            // 
            this.tsbSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSearch.Image = global::nppVisualXml.Properties.Resources.find;
            this.tsbSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSearch.Name = "tsbSearch";
            this.tsbSearch.Size = new System.Drawing.Size(23, 22);
            this.tsbSearch.Text = "Search";
            this.tsbSearch.Click += new System.EventHandler(this.tsbSearch_Click);
            // 
            // tsbCaseSensitive
            // 
            this.tsbCaseSensitive.BackColor = System.Drawing.SystemColors.Control;
            this.tsbCaseSensitive.Checked = true;
            this.tsbCaseSensitive.CheckOnClick = true;
            this.tsbCaseSensitive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsbCaseSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbCaseSensitive.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsbCaseSensitive.Image = ((System.Drawing.Image)(resources.GetObject("tsbCaseSensitive.Image")));
            this.tsbCaseSensitive.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCaseSensitive.Name = "tsbCaseSensitive";
            this.tsbCaseSensitive.Size = new System.Drawing.Size(23, 22);
            this.tsbCaseSensitive.Text = "Cc";
            // 
            // tsbRegex
            // 
            this.tsbRegex.Checked = true;
            this.tsbRegex.CheckOnClick = true;
            this.tsbRegex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsbRegex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbRegex.Image = ((System.Drawing.Image)(resources.GetObject("tsbRegex.Image")));
            this.tsbRegex.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRegex.Name = "tsbRegex";
            this.tsbRegex.Size = new System.Drawing.Size(23, 22);
            this.tsbRegex.Text = "*.";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbReload
            // 
            this.tsbReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbReload.Image = global::nppVisualXml.Properties.Resources.refresh;
            this.tsbReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReload.Name = "tsbReload";
            this.tsbReload.Size = new System.Drawing.Size(23, 22);
            this.tsbReload.Text = "Reload";
            this.tsbReload.Click += new System.EventHandler(this.tsbReload_Click);
            // 
            // tXPath
            // 
            this.tXPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tXPath.Location = new System.Drawing.Point(3, 535);
            this.tXPath.Name = "tXPath";
            this.tXPath.ReadOnly = true;
            this.tXPath.Size = new System.Drawing.Size(388, 22);
            this.tXPath.TabIndex = 2;
            this.tXPath.WordWrap = false;
            // 
            // lvResults
            // 
            this.lvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvResults.FullRowSelect = true;
            this.lvResults.GridLines = true;
            this.lvResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(0, 380);
            this.lvResults.Margin = new System.Windows.Forms.Padding(0);
            this.lvResults.MultiSelect = false;
            this.lvResults.Name = "lvResults";
            this.lvResults.ShowItemToolTips = true;
            this.lvResults.Size = new System.Drawing.Size(394, 152);
            this.lvResults.TabIndex = 3;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.VirtualMode = true;
            this.lvResults.ItemActivate += new System.EventHandler(this.LvResults_ItemActivate);
            this.lvResults.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LvResults_RetrieveVirtualItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Path";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Preview";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Line:Col";
            this.columnHeader3.Width = 100;
            // 
            // XmlViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(394, 561);
            this.Controls.Add(this.tlpFrames);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "XmlViewer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "VisualXML";
            this.Load += new System.EventHandler(this.XmlViewer_Load);
            this.tlpFrames.ResumeLayout(false);
            this.tlpFrames.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvXml;
        private System.Windows.Forms.TableLayoutPanel tlpFrames;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbCollapse;
        private System.Windows.Forms.ToolStripButton tsbExpand;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbReload;
        private System.Windows.Forms.TextBox tXPath;
        private System.Windows.Forms.ToolStripButton tsbCaseSensitive;
        private System.Windows.Forms.ToolStripButton tsbRegex;
        private System.Windows.Forms.ToolStripComboBox tscboSearch;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}
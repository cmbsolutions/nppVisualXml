using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nppVisualXml.Forms
{
    public partial class SearchResultsWidget : UserControl
    {
        public SearchResultsWidget()
        {
            InitializeComponent();
        }

        private void SearchResultsWidget_SizeChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                // Adjust the 3 columns to 50%, 40% and 10% of the total width
                lvResults.Columns[0].Width = (int)(lvResults.ClientSize.Width * 0.5);
                lvResults.Columns[1].Width = (int)(lvResults.ClientSize.Width * 0.4);
                lvResults.Columns[2].Width = (int)(lvResults.ClientSize.Width * 0.1);
            }
        }
    }
}

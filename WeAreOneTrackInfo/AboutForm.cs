using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WeAreOneTrackInfo
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            linkLabel1.Click += (sender, e) => Process.Start(linkLabel1.Text);
            linkLabel2.Click += (sender, e) => Process.Start(linkLabel2.Text);
            linkLabel3.Click += (sender, e) => Process.Start(linkLabel3.Text);
        }
    }
}

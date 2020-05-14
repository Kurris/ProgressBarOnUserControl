using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProgressForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public bool IsStop
        {
            get; set;
        }
        public string TipMessage { get; internal set; }

        internal void ReportProgress(int progressPercentage, object userState)
        {
            throw new NotImplementedException();
        }
    }
}

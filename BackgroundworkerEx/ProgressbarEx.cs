using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ligy
{
    public partial class ProgressbarEx : Form
    {
        public ProgressbarEx()
        {
            InitializeComponent();

            MainProgressBar.Style = ProgressBarStyle.Marquee;
            MainProgressBar.MarqueeAnimationSpeed = 50;

            this.ShowInTaskbar = false;

            PicStop.Click += (s, eve) =>
            {
                IsStop = true;
            };

            this.MouseDown += CusProgressForm_MouseDown;
            this.MouseUp += CusProgressForm_MouseUp;
            this.MouseMove += CusProgressForm_MouseMove;
        }

        /// <summary>
        /// Need Stop ?
        /// </summary>
        public bool IsStop { get; private set; } = false;

        /// <summary>
        /// TipMessage
        /// </summary>
        public string TipMessage
        {
            get
            {
                return lblTips.Text;
            }
            set
            {

                lblTips.Text = value;
            }
        }

        /// <summary>
        /// Set ProgressBar value ,which makes ProgressBar smooth
        /// </summary>
        /// <param name="value"></param>
        public void SetProgressValue(int value)
        {

            if (MainProgressBar.Value == 100) MainProgressBar.Value = 0;

            MainProgressBar.Value = value;
        }



        #region NoWrap drag

        private bool _mbisDown = false;
        private Point _mpoint;

        private void CusProgressForm_MouseUp(object sender, MouseEventArgs e)
        {
            _mbisDown = false;
        }

        private void CusProgressForm_MouseDown(object sender, MouseEventArgs e)
        {
            _mbisDown = true;
            _mpoint = e.Location;
        }
        private void CusProgressForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mbisDown)
            {
                Point p = e.Location;
                int x = p.X - _mpoint.X;
                int y = p.Y - _mpoint.Y;
                this.Location = new Point(this.Location.X + x, this.Location.Y + y);
            }
        }
        #endregion


    }
}

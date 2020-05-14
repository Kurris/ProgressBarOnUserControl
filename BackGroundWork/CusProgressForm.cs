using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unitl
{
    public partial class CusProgressForm : Form
    {
        public CusProgressForm()
        {
            InitializeComponent();

            InitStyle();
        }

        /// <summary>
        /// 是否需要停止
        /// </summary>
        public bool IsStop { get; private set; } = false;

        /// <summary>
        /// 提示内容
        /// </summary>
        public string TipMessage { get; set; }

        /// <summary>
        /// 样式
        /// </summary>
        private void InitStyle()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 50;
            this.FormClosed += (s , e) =>
            {
                this.DialogResult = DialogResult.OK;
            };
            this.ShowInTaskbar = false;

            PicStop.Click += (s , e) =>
            {
                IsStop = true;
            };

            this.MouseDown += CusProgressForm_MouseDown;
            this.MouseUp += CusProgressForm_MouseUp;
            this.MouseMove += CusProgressForm_MouseMove;
        }

        #region 无边框拖拽

        private bool _isDown = false;
        private Point _point;
        private void CusProgressForm_MouseUp(object sender , MouseEventArgs e)
        {
            _isDown = false;
        }

        private void CusProgressForm_MouseDown(object sender , MouseEventArgs e)
        {
            _isDown = true;
            _point = e.Location;
        }
        private void CusProgressForm_MouseMove(object sender , MouseEventArgs e)
        {
            if( _isDown )
            {
                Point p = e.Location;
                int x = p.X - _point.X;
                int y = p.Y - _point.Y;
                this.Location = new Point(this.Location.X + x , this.Location.Y + y);
            }
        }
        #endregion



        public void SetTip()
        {
            lblTips.Text = TipMessage;
        }

        internal void ReportProgress(int progressPercentage , object userState)
        {
            if( progressBar1.Value==progressBar1.Maximum )
            {
                progressBar1.Value = 0;
            }
            progressBar1.Value = progressPercentage ;
        }
    }
}

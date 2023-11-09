namespace Kurisu
{
    partial class ProgressbarNew
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressbarNew));
            this.MainProgressBar = new System.Windows.Forms.ProgressBar();
            this.lblTips = new System.Windows.Forms.Label();
            this.PicStop = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PicStop)).BeginInit();
            this.SuspendLayout();
            // 
            // MainProgressBar
            // 
            this.MainProgressBar.Location = new System.Drawing.Point(13, 35);
            this.MainProgressBar.Name = "MainProgressBar";
            this.MainProgressBar.Size = new System.Drawing.Size(626, 12);
            this.MainProgressBar.TabIndex = 0;
            // 
            // lblTips
            // 
            this.lblTips.Location = new System.Drawing.Point(11, 9);
            this.lblTips.Name = "lblTips";
            this.lblTips.Size = new System.Drawing.Size(295, 18);
            this.lblTips.TabIndex = 1;
            this.lblTips.Text = "等待时间";
            this.lblTips.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PicStop
            // 
            this.PicStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PicStop.Image = ((System.Drawing.Image)(resources.GetObject("PicStop.Image")));
            this.PicStop.Location = new System.Drawing.Point(641, 9);
            this.PicStop.Name = "PicStop";
            this.PicStop.Size = new System.Drawing.Size(21, 18);
            this.PicStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicStop.TabIndex = 2;
            this.PicStop.TabStop = false;
            // 
            // ProgressbarEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 69);
            this.Controls.Add(this.PicStop);
            this.Controls.Add(this.lblTips);
            this.Controls.Add(this.MainProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ProgressbarEx";
            ((System.ComponentModel.ISupportInitialize)(this.PicStop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar MainProgressBar;
        private System.Windows.Forms.Label lblTips;
        private System.Windows.Forms.PictureBox PicStop;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unitl;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnStop.Click += BtnStop_Click;
            btnBegin.Click += BtnBegin_Click;

            btnStop.Enabled = false;
        }

        CusBackgroundWorkHelper<CusArg> workHelper = null;


        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBegin_Click(object sender , EventArgs e)
        {
            btnBegin.Enabled = false;
            btnStop.Enabled = true;

            try
            {
                if( workHelper != null )
                {
                    workHelper = null;
                }
                if( workHelper == null )
                {
                    workHelper = new CusBackgroundWorkHelper<CusArg>();
                }

                //文本显示
                //workHelper.Mode = CusBackgroundWorkHelper<CusArg>.ShowMode.Text;
                //workHelper.ProgressTip = "正在读取!";
                //workHelper.ShowContanier = label1;

                //进度条显示
                workHelper.Mode = CusBackgroundWorkHelper<CusArg>.ShowMode.Progress;
                workHelper.AllowDialog = true;
                workHelper.ShowContanier = this;

                workHelper.DoWork += (eve) =>
                {
                    //System.Threading.Thread.Sleep(5000);
                    CusArg cus = eve.Argument;
                    try
                    {
                        //测试暂停/完成的数据
                        for( int i = 0 ; i < 200 ; i++ )
                        {
                            System.Threading.Thread.Sleep(100);
                            if( i == 50 )
                            {
                                cus.Msg += "1";
                            }
                            else if( i == 100 )
                            {
                                cus.Msg += "1";
                            }
                            else if( i == 150 )
                            {
                                cus.Msg += "1";
                            }
                            else
                            {
                                cus.Msg += "1";
                            }

                        }
                    }
                    catch( Exception ) { }
                    finally
                    {
                        eve.Result = cus;
                    }

                };
                workHelper.RunWorkCompleted += (eve) =>
                {
                    CusArg ca = eve.Result as CusArg;
                    MessageBox.Show("finish" + "    " + ca.Msg);
                    btnBegin.Enabled = true;
                    btnStop.Enabled = false;
                };

                workHelper.WorkStoped += (eve) =>
                {
                    CusArg ca = eve.Argument as CusArg;
                    MessageBox.Show("stop" + "    " + ca.Msg);
                    btnBegin.Enabled = true;
                    btnStop.Enabled = false;
                };

                //参数
                CusArg arg = new CusArg()
                {
                    Msg = "Msg"
                };

                workHelper.AsyncStart(arg);
            }
            catch( Exception ex)
            {
                btnBegin.Enabled = true;
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStop_Click(object sender , EventArgs e)
        {
            workHelper.Stop();
        }
        /// <summary>
        /// 参数类
        /// </summary>
        public class CusArg
        {
            public string Msg { get; set; }
        }
    }
}


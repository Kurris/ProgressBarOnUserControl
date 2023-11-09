using System;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using Kurisu;

namespace Ligy
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnBegin.Click += BtnBegin_Click;
        }

        KurisuBackgroundWorker<ParaArg> workHelper = null;

        /// <summary>
        /// Begin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBegin_Click(object sender, EventArgs e)
        {
            btnBegin.Enabled = false;

            try
            {
                if (workHelper != null || (workHelper != null && workHelper.IsBusy))
                {
                    workHelper.Dispose();
                    workHelper = null;
                }
                if (workHelper == null)
                {
                    workHelper = new KurisuBackgroundWorker<ParaArg>();
                }

                workHelper.Executing += (eve) =>
                {
                    ParaArg args = eve.Argument;

                    try
                    {
                        //ToDo  like Thread.Sleep(20000);
                        Thread.Sleep(10000);
                        args.Msg = "...this is bussiness code result";
                        throw new Exception("");
                    }
                    catch (Exception ex)
                    {
                        args.Ex = ex;
                    }
                    finally
                    {
                        eve.Result = args;
                    }

                };
                workHelper.Completed += (eve) =>
                {
                    if (eve.Error != null)
                    {
                        //get .net backgroundworker exception;
                        //handle this exception;
                        //return ?
                    }

                    //get your para result
                    ParaArg x = eve.Result;

                    if (x.Ex != null)
                    {
                        //get your bussiness exception;
                        //handle this exception;
                        //return ?
                    }

                    //finially get your need;
                    //MayBe to do some UI hanlde and bussiness logical
                    string sReusltMsg = x.Msg;

                    btnBegin.Enabled = true;
                };

                workHelper.Stoped += (eve) =>
                {
                    //if stoped ! it means no error;
                    //just get what you want; 
                    ParaArg x = eve.Result as ParaArg;

                    btnBegin.Enabled = true;
                };

                //参数
                ParaArg arg = new ParaArg()
                {
                    Msg = "Msg"
                };

                workHelper.Start(arg);

            }
            catch (Exception ex)
            {
                btnBegin.Enabled = true;
                MessageBox.Show(ex.Message);
            }
        }
    }

    /// <summary>
    /// Para Class
    /// </summary>
    public class ParaArg
    {
        public DataTable Data { get; set; }
        public string Msg { get; set; }
        public Exception Ex { get; set; }
    }
}


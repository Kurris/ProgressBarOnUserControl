using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Unitl
{
    public class CusBackgroundWorkHelper<T> : IWorkerReportProgress, IDisposable
    {
        public delegate void DoWorkEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// 异步工作开发
        /// </summary>
        public event DoWorkEventHandler DoWork;

        public delegate void StopEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// 异步工作被停止
        /// </summary>
        public event StopEventHandler WorkStoped;

        public delegate void RunWorkCompletedEventHandler(RunWorkerCompletedEventArgs Argument);
        /// <summary>
        /// 异步工作完成
        /// </summary>
        public event RunWorkCompletedEventHandler RunWorkCompleted;

        public delegate void ProgressChangedEventHandler(MProgressChangedEventArgs Argument);
        /// <summary>
        /// 进度报告改变
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        private BackgroundWorker Worker = null;

        private T WorkArg { get; set; }

        private Timer timer;

        /// <summary>
        /// 工作线程
        /// </summary>
        private Thread WorkerThread;

        /// <summary>
        /// 进度条文字颜色
        /// </summary>
        public Brush ProgressBackColor { get; set; } = Brushes.Blue;

        /// <summary>
        /// 进度条提示 默认： 正在加载数据，请稍后[{0}]{1}
        /// </summary>
        public string ProgressTip { get; set; } = "正在加载数据,请稍等[{0}]{1}";
        private int _WorkerStartDateSecond = 0;
        private int _ShowProgressCount = 0;

        /// <summary>
        /// 是否需要停止,默认为False
        /// </summary>
        public bool IsStop { get; private set; } = false;

        /// <summary>
        /// 使用进度条/文本, 默认为ShowMode.Text
        /// </summary>
        public ShowMode Mode { get; set; } = ShowMode.Text;


        public bool AllowDialog { get; set; } = true;


        /// <summary>
        /// 显示模式
        /// </summary>
        public enum ShowMode
        {
            Progress = 0,
            Text = 1
        }

        /// <summary>
        /// 表示Worker组件是否在忙
        /// </summary>
        public bool IsBusy
        {
            get
            {
                if( Worker != null )
                {
                    return Worker.IsBusy;
                }

                return false;
            }
        }

        /// <summary>
        /// 显示的容器
        /// </summary>
        public Control ShowContanier { set; private get; }

        /// <summary>
        /// 容器的窗体
        /// </summary>
        private Form _ParentForm = null;

        /// <summary>
        /// 进度条窗体
        /// </summary>
        private CusProgressForm _progressForm = null;

        /// <summary>
        /// 父窗口
        /// </summary>
        public Form ParentForm
        {
            get
            {
                if( _ParentForm != null )
                {
                    return _ParentForm;
                }

                if( ShowContanier != null )
                {
                    return ShowContanier.FindForm();
                }

                return null;
            }
        }

        /// <summary>
        /// 异步任务开始
        /// </summary>
        /// <param name="Para"></param>
        public void AsyncStart(T Para)
        {
            if( DoWork == null )
            {
                return;
            }

            _WorkerStartDateSecond = 0;
            _ShowProgressCount = 0;

            StopTimer();

            Worker = new BackgroundWorker();

            //允许展示进度条   初始化
            if( Mode == ShowMode.Progress )
            {
                if( _progressForm != null )
                {
                    _progressForm.Close();
                    _progressForm = null;
                }
                _progressForm = new CusProgressForm();
            }

            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkCompleted;
            Worker.ProgressChanged += Worker_ProgressChanged;

            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            WorkArg = Para;
            StartTimer();

            Worker.RunWorkerAsync(Para);

            if( Mode == ShowMode.Progress )
            {
                _progressForm.StartPosition = FormStartPosition.CenterParent;
                if( AllowDialog )
                {
                    _progressForm.ShowDialog();
                }
                else
                {
         
                    _progressForm.Show();
                }

            }
        }

        /// <summary>
        /// 异步工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender , DoWorkEventArgs e)
        {
            if( DoWork != null )
            {
                DoWorkEventArgs<T> Argument = new DoWorkEventArgs<T>(e.Argument);

                WorkerThread = new Thread((o) =>
                {
                    try
                    {
                        DoWork?.Invoke(Argument);
                    }
                    catch( ThreadAbortException ) { }
                    catch( Exception ) { }

                });

                WorkerThread.IsBackground = true;
                WorkerThread.Start(Argument);

                while( WorkerThread.IsAlive )
                {
                    Thread.Sleep(10);
                }

                e.Result = Argument.Result;
            }
            else
            {
                e.Cancel = true;
            }
        }


        private void Worker_ProgressChanged(object sender , ProgressChangedEventArgs e)
        {
            try
            {
                if( Mode == ShowMode.Progress && _progressForm != null )
                {
                    _progressForm.ReportProgress(e.ProgressPercentage , e.UserState);

                    ProgressChanged?.Invoke(new MProgressChangedEventArgs(e.ProgressPercentage , e.UserState));
                }
            }
            catch( Exception ) { }

        }

        public void Worker_RunWorkCompleted(object sender , RunWorkerCompletedEventArgs e)
        {
            try
            {
                if( Mode == ShowMode.Progress )
                {
                    CloseProgressForm();
                }

                if( WorkerThread != null && WorkerThread.ThreadState == ThreadState.Aborted )
                {
                    WorkStoped?.Invoke(new DoWorkEventArgs<T>(WorkArg));
                }
                else
                {
                    RunWorkCompleted?.Invoke(new RunWorkerCompletedEventArgs<T>(e.Result , e.Error , e.Cancelled));
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                StopWorker();
                StopTimer();
            }
        }


        /// <summary>
        /// 定时器
        /// </summary>
        private void StartTimer()
        {
            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center ,
                LineAlignment = StringAlignment.Center
            };
            Font f = new Font(ShowContanier.Font , FontStyle.Bold);
            Graphics g = ShowContanier.CreateGraphics();

            if( !ProgressTip.Contains("{0}") )
            {
                ProgressTip += "..用时{0}{1}";
            }

            if( timer == null )
            {
                timer = new Timer(1000);
                timer.Elapsed += (s , e) =>
                {
                    if( ( _progressForm != null && _progressForm.IsStop ) || IsStop )
                    {
                        if( Worker != null )
                        {
                            try
                            {
                                if( WorkerThread != null && WorkerThread.IsAlive )
                                {
                                    StopTimer();
                                    WorkerThread.Abort();
                                }
                            }
                            catch( ThreadAbortException ) { }
                            catch( Exception ) { }
                        }
                    }

                    if( ShowContanier == null ) return;

                    //回调容器
                    ShowContanier.Invoke(new Action<DateTime>((datetime) =>
                    {
                        DateTime sTime = datetime;

                        //工作时间
                        _WorkerStartDateSecond++;

                        //...的个数
                        _ShowProgressCount++;

                        if( _ShowProgressCount > 6 )
                        {
                            _ShowProgressCount = 1;
                        }

                        string[] strs = new string[_ShowProgressCount];

                        string ProgressStr = string.Join("." , strs);

                        string ProgressText = string.Format(ProgressTip , _WorkerStartDateSecond , ProgressStr);

                        if( _progressForm != null )
                        {
                            _progressForm.ReportProgress(_WorkerStartDateSecond , null);

                            _progressForm.TipMessage = ProgressText;
                            _progressForm.SetTip();
                        }
                        //必须在DrawString之前
                        ShowContanier.Refresh();

                        if( Mode == ShowMode.Text )
                        {
                            g.DrawString(ProgressText , f , ProgressBackColor , ShowContanier.ClientRectangle , sf);
                        }

                        

                    }) , e.SignalTime);
                };

                if( !timer.Enabled )
                {
                    timer.Start();
                }
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void Stop()
        {
            IsStop = true;
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        private void StopTimer()
        {
            if( timer != null && timer.Enabled )
            {
                timer.Stop();
            }
            //最后一次刷新容器
            if( ShowContanier != null )
            {
                if( !ShowContanier.IsDisposed && ShowContanier.Created )
                {
                    ShowContanier.Invoke(new Action(() =>
                    {
                        ShowContanier.Refresh();
                    }));
                }
            }
        }

        /// <summary>
        /// 停止工作
        /// </summary>
        private void StopWorker()
        {
            if( Worker != null )
            {
                Worker.DoWork -= Worker_DoWork;
                Worker.RunWorkerCompleted -= Worker_RunWorkCompleted;
                Worker.ProgressChanged -= Worker_ProgressChanged;

                try
                {
                    if( WorkerThread != null && WorkerThread.IsAlive )
                    {
                        WorkerThread.Abort();
                    }
                }
                catch( Exception ) { }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if( !disposing )
            {
                try
                {
                    DoWork = null;
                    RunWorkCompleted = null;
                    ProgressChanged = null;
                    WorkStoped = null;
                    WorkerThread = null;
                    Worker.Dispose();
                    Worker = null;
                }
                catch( Exception ) { }
            }
        }



        /// <summary>
        /// 关闭进度条
        /// </summary>
        private void CloseProgressForm()
        {
            if( _progressForm != null )
            {
                _progressForm.Close();
                _progressForm.Dispose();
            }
        }

        public void ReportProgress(int precentProgress , object userState)
        {
            if( Worker != null )
            {
                Worker.ReportProgress(precentProgress , userState);
            }
        }

        public void Dispose()
        {
            try
            {
                DoWork = null;
                RunWorkCompleted = null;
                ProgressChanged = null;
                WorkStoped = null;
                WorkerThread = null;
                Worker.Dispose();
                Worker = null;
            }
            catch( Exception ) { }
        }
    }

    public class DoWorkEventArgs<T> : DoWorkEventArgs
    {
        public new T Argument { get; set; }

        public new T Result { get; set; }

        public DoWorkEventArgs(object argument) : base(argument)
        {
            Argument = (T) argument;
        }
    }


    public class RunWorkerCompletedEventArgs<T> : RunWorkerCompletedEventArgs
    {
        public new T Result { get; set; }

        public RunWorkerCompletedEventArgs(object result , Exception error , bool cancelled) : base(result , error , cancelled)
        {
            Result = (T) result;
        }
    }

    public class MProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public MProgressChangedEventArgs(int progressPercentage , object userState) : base(progressPercentage , userState)
        {

        }

    }
    public interface IWorkerReportProgress
    {
        void ReportProgress(int percentProgress , object userState);
    }
}

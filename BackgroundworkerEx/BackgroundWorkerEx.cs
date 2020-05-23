using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Timer = System.Timers.Timer;

namespace Ligy
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BackgroundWorkerEx<T> : IWorkerReportProgress, IDisposable
    {
        public delegate void DoWorkEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// StartAsync
        /// </summary>
        public event DoWorkEventHandler DoWork;

        public delegate void StopEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// StopAsync
        /// </summary>
        public event StopEventHandler WorkStoped;

        public delegate void RunWorkCompletedEventHandler(RunWorkerCompletedEventArgs Argument);
        /// <summary>
        /// FinishAsync
        /// </summary>
        public event RunWorkCompletedEventHandler RunWorkCompleted;

        public delegate void ProgressChangedEventHandler(MProgressChangedEventArgs Argument);
        /// <summary>
        /// ReportProgress
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// .Net  BackgroundWorker
        /// </summary>
        private BackgroundWorker Worker = null;

        /// <summary>
        /// Whole Para
        /// </summary>
        private T WorkArg { get; set; }

        /// <summary>
        /// Timer
        /// </summary>
        private Timer timer;

        /// <summary>
        /// WorkingThread
        /// </summary>
        private Thread WorkerThread;

        /// <summary>
        /// 进度条提示 默认： 正在加载数据，请稍后[{0}]{1}
        /// </summary>
        public string ProgressTip { get; set; } = "Elapsed Time[{0}]{1}";

        /// <summary>
        /// Async time sec
        /// </summary>
        private int _miWorkerStartDateSecond = 0;

        /// <summary>
        /// Async time dot
        /// </summary>
        private int _miShowProgressCount = 0;

        /// <summary>
        /// Stop flag
        /// </summary>
        private bool _mbIsStop = false;


        /// <summary>
        /// Express Busy
        /// </summary>
        public bool IsBusy
        {
            get
            {
                if (Worker != null)
                {
                    return Worker.IsBusy;
                }
                return false;
            }
        }

        /// <summary>
        /// ProgressbarEx
        /// </summary
        private ProgressbarEx _mfrmProgressForm = null;

        /// <summary>
        /// Start AsyncWorl
        /// </summary>
        /// <param name="Para"></param>
        public void AsyncStart(T Para)
        {
            //if workeven is  null ,express user do not regist event
            if (DoWork == null)
            {
                return;
            }

            _miWorkerStartDateSecond = 0;
            _miShowProgressCount = 0;

            //init
            if (Worker != null && Worker.IsBusy)
            {
                Worker.CancelAsync();
                Worker = null;
            }

            Worker = new BackgroundWorker();

            //create progressbar
            _mfrmProgressForm = new ProgressbarEx();

            //add event
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkCompleted;
            Worker.ProgressChanged += Worker_ProgressChanged;

            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            //Set Whole Para
            WorkArg = Para;

            //Start timer
            StartTimer();

            Worker.RunWorkerAsync(Para);

            _mfrmProgressForm.StartPosition = FormStartPosition.CenterParent;
            _mfrmProgressForm.ShowDialog();
        }

        /// <summary>
        /// Working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (DoWork == null)
            {
                e.Cancel = true;
                return;
            }

            DoWorkEventArgs<T> Argument = new DoWorkEventArgs<T>(e.Argument);

            WorkerThread = new Thread(o =>
            {
                try
                {
                    DoWork?.Invoke(Argument);
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception)
                {

                }

            });

            WorkerThread.IsBackground = true;
            WorkerThread.Start(Argument);

            //Maybe cpu do not start thread
            Thread.Sleep(20);

            while (WorkerThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            e.Result = Argument.Result;
        }

        /// <summary>
        /// Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_RunWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (_mfrmProgressForm != null)
                {
                    _mfrmProgressForm.Close();
                    _mfrmProgressForm.Dispose();
                    _mfrmProgressForm = null;
                }

                //In timer, When stop progress will make thread throw AbortException
                if (WorkerThread != null && WorkerThread.ThreadState == ThreadState.Aborted)
                {
                    WorkStoped?.Invoke(new DoWorkEventArgs<T>(WorkArg));
                }
                else
                {
                    RunWorkCompleted?.Invoke(new RunWorkerCompletedEventArgs<T>(e.Result, e.Error, e.Cancelled));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                StopWorkerAndThread();
                StopTimer();
            }
        }

        /// <summary>
        /// Report Progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (_mfrmProgressForm != null)
                {
                    _mfrmProgressForm.ReportProgress(e.ProgressPercentage, e.UserState);

                    ProgressChanged?.Invoke(new MProgressChangedEventArgs(e.ProgressPercentage, e.UserState));
                }
            }
            catch (Exception) { }

        }




        /// <summary>
        /// Timer Start 
        /// </summary>
        private void StartTimer()
        {
            //Check user ProgressTip
            if (!ProgressTip.Contains("{0}"))
            {
                ProgressTip += "...Elapsed Time{0}{1}";
            }

            if (timer != null) return;

            //On one sec 
            timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                //progress and it's stop flag (picture stop)||  this stop flag
                if ((_mfrmProgressForm != null && _mfrmProgressForm.IsStop) || _mbIsStop)
                {
                    if (Worker != null)
                    {
                        try
                        {
                            if (WorkerThread != null && WorkerThread.IsAlive)
                            {
                                StopTimer();
                                WorkerThread.Abort();
                            }
                        }
                        catch (ThreadAbortException)
                        {

                        }
                        catch (Exception)
                        {

                        }
                    }
                }

                if (_mfrmProgressForm!=null)
                {
                    //Callback 
                    _mfrmProgressForm.Invoke(new Action<DateTime>(elapsedtime =>
                    {
                        DateTime sTime = elapsedtime;

                        //worked time
                        _miWorkerStartDateSecond++;

                        //.....count
                        _miShowProgressCount++;

                        if (_miShowProgressCount > 6)
                        {
                            _miShowProgressCount = 1;
                        }

                        string[] strs = new string[_miShowProgressCount];

                        string ProgressStr = string.Join(".", strs);

                        string ProgressText = string.Format(ProgressTip, _miWorkerStartDateSecond, ProgressStr);

                        if (_mfrmProgressForm != null)
                        {
                            _mfrmProgressForm.ReportProgress(_miWorkerStartDateSecond, null);
                            _mfrmProgressForm.TipMessage = ProgressText;
                            _mfrmProgressForm.SetTip();
                        }
                    }), e.SignalTime);
                }
            };

            if (!timer.Enabled)
            {
                timer.Start();
            }
        }

        /// <summary>
        /// Stop Async
        /// </summary>
        public void Stop()
        {
            _mbIsStop = true;
        }

        /// <summary>
        /// Stop Timer
        /// </summary>
        private void StopTimer()
        {
            if (timer != null && timer.Enabled)
            {
                timer.Stop();
            }

            //refresh contanier
            if (_mfrmProgressForm != null)
            {
                if (!_mfrmProgressForm.IsDisposed && _mfrmProgressForm.Created)
                {
                    _mfrmProgressForm.Close();
                    _mfrmProgressForm.Dispose();
                    _mfrmProgressForm = null;
                }
            }
        }

        /// <summary>
        /// Stop the all objects
        /// </summary>
        private void StopWorkerAndThread()
        {
            if (Worker != null)
            {
                Worker.DoWork -= Worker_DoWork;
                Worker.RunWorkerCompleted -= Worker_RunWorkCompleted;
                Worker.ProgressChanged -= Worker_ProgressChanged;

                try
                {
                    if (WorkerThread != null && WorkerThread.IsAlive)
                    {
                        WorkerThread.Abort();
                    }
                }
                catch (Exception) { }
            }
        }



        public void ReportProgress(int precentProgress, object userState)
        {
            if (Worker != null)
            {
                Worker.ReportProgress(precentProgress, userState);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            try
            {
                DoWork = null;
                RunWorkCompleted = null;
                WorkStoped = null;
                ProgressChanged = null;

                WorkerThread = null;
                Worker.Dispose();
                Worker = null;
                timer = null;
            }
            catch (Exception) { }
        }
    }

    public class DoWorkEventArgs<T> : DoWorkEventArgs
    {
        public new T Argument { get; set; }

        public new T Result { get; set; }

        public DoWorkEventArgs(object argument) : base(argument)
        {
            Argument = (T)argument;
        }
    }


    public class RunWorkerCompletedEventArgs<T> : RunWorkerCompletedEventArgs
    {
        public new T Result { get; set; }

        public RunWorkerCompletedEventArgs(object result, Exception error, bool cancelled) : base(result, error, cancelled)
        {
            Result = (T)result;
        }
    }

    public class MProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public MProgressChangedEventArgs(int progressPercentage, object userState) : base(progressPercentage, userState)
        {

        }

    }
    public interface IWorkerReportProgress
    {
        void ReportProgress(int percentProgress, object userState);
    }
}

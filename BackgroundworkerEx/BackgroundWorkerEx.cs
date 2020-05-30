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
    public class BackgroundWorkerEx<T> : IDisposable
    {
        #region Event

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

        public delegate void RunWorkCompletedEventHandler(RunWorkerCompletedEventArgs<T> Argument);
        /// <summary>
        /// FinishAsync
        /// </summary>
        public event RunWorkCompletedEventHandler RunWorkCompleted;


        #endregion

        /// <summary>
        /// .Net  BackgroundWorker
        /// </summary>
        private BackgroundWorker _mWorker = null;

        /// <summary>
        /// Whole Para
        /// </summary>
        private T _mWorkArg = default(T);

        /// <summary>
        /// Timer
        /// </summary>
        private Timer _mTimer = null;

        /// <summary>
        /// WorkingThread
        /// </summary>
        private Thread _mWorkerThread = null;

        /// <summary>
        /// Async time sec
        /// </summary>
        private int _miWorkerStartDateSecond = 0;

        /// <summary>
        /// Async time dot
        /// </summary>
        private int _miShowProgressCount = 0;

        /// <summary>
        /// ProgressbarEx
        /// </summary
        private ProgressbarEx _mfrmProgressForm = null;



        /// <summary>
        /// Express Busy
        /// </summary>
        public bool IsBusy
        {
            get
            {
                if (_mWorker != null)
                {
                    return _mWorker.IsBusy;
                }
                return false;
            }
        }

        /// <summary>
        /// 进度条提示 默认： 正在加载数据，请稍后[{0}]{1}
        /// </summary>
        public string ProgressTip { get; set; } = "Elapsed Time[{0}]{1}";



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
            if (_mWorker != null && _mWorker.IsBusy)
            {
                _mWorker.CancelAsync();
                _mWorker = null;
            }

            _mWorker = new BackgroundWorker();

            //create progressbar
            _mfrmProgressForm = new ProgressbarEx();

            //add event
            _mWorker.DoWork += Worker_DoWork;
            _mWorker.RunWorkerCompleted += Worker_RunWorkCompleted;

            _mWorker.WorkerReportsProgress = true;
            _mWorker.WorkerSupportsCancellation = true;

            //Set Whole Para
            _mWorkArg = Para;

            _mWorker.RunWorkerAsync(Para);
            //Start timer
            StartTimer();

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

            try
            {
                if (_mWorkerThread != null && _mWorkerThread.IsAlive)
                {
                    _mWorkerThread.Abort();
                }
            }
            catch (Exception)
            {
                Thread.Sleep(50);
            }

            _mWorkerThread = new Thread(a =>
            {
                try
                {
                    DoWork?.Invoke(a as DoWorkEventArgs<T>);
                }
                catch (Exception)
                {

                }
            });

            _mWorkerThread.IsBackground = true;
            _mWorkerThread.Start(Argument);

            //Maybe cpu do not start thread
            Thread.Sleep(20);

            //Wait.....
            while (_mWorkerThread.IsAlive)
            {
                Thread.Sleep(50);
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

                if (_mWorker != null)
                {
                    _mWorker.DoWork -= Worker_DoWork;
                    _mWorker.RunWorkerCompleted -= Worker_RunWorkCompleted;

                    try
                    {
                        if (_mWorkerThread != null && _mWorkerThread.IsAlive)
                        {
                            _mWorkerThread.Abort();
                        }
                    }
                    catch (Exception) { }
                }

                //In timer, When stop progress will make thread throw AbortException
                if (_mWorkerThread != null && _mWorkerThread.ThreadState == ThreadState.Aborted)
                {
                    WorkStoped?.Invoke(new DoWorkEventArgs<T>(_mWorkArg));
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

            if (_mTimer != null) return;

            //On one sec 
            _mTimer = new Timer(1000);
            _mTimer.Elapsed += (s, e) =>
            {
                //progress and it's stop flag (picture stop)||  this stop flag
                if (_mfrmProgressForm != null && _mfrmProgressForm.IsStop)
                {
                    if (_mWorker != null)
                    {
                        try
                        {
                            if (_mWorkerThread != null && _mWorkerThread.IsAlive)
                            {
                                if (_mTimer != null && _mTimer.Enabled)
                                {
                                    _mTimer.Stop();
                                    _mTimer = null;
                                }
                                _mWorkerThread.Abort();
                            }
                        }
                        catch (Exception) { }
                    }
                }

                if (_mfrmProgressForm != null)
                {
                    //Callback 
                    _mfrmProgressForm.Invoke(new Action<DateTime>(elapsedtime =>
                    {
                        DateTime sTime = elapsedtime;

                        //worked time
                        _miWorkerStartDateSecond++;
                        if (_mfrmProgressForm != null)
                        {
                            _mfrmProgressForm.SetProgressValue(_miWorkerStartDateSecond);
                        }

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
                            _mfrmProgressForm.TipMessage = ProgressText;
                        }
                    }), e.SignalTime);
                }
            };

            if (!_mTimer.Enabled)
            {
                _mTimer.Start();
            }
        }


        #region Dispose

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

                _mWorkerThread = null;
                _mWorker.Dispose();
                _mWorker = null;
                _mTimer = null;
            }
            catch (Exception)
            {

            }
        }

        #endregion
    }



    #region Parameters Class

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

    #endregion
}

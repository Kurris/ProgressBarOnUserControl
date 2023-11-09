using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

using Timer = System.Timers.Timer;

namespace Kurisu
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KurisuBackgroundWorker<T> : IDisposable
    {
        #region Event

        public delegate void ExecutingEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// StartAsync
        /// </summary>
        public event ExecutingEventHandler Executing;

        public delegate void StopedEventHandler(DoWorkEventArgs<T> Argument);
        /// <summary>
        /// StopAsync
        /// </summary>
        public event StopedEventHandler Stoped;

        public delegate void CompletedEventHandler(RunWorkerCompletedEventArgs<T> Argument);
        /// <summary>
        /// FinishAsync
        /// </summary>
        public event CompletedEventHandler Completed;

        #endregion

        /// <summary>
        /// .Net  BackgroundWorker
        /// </summary>
        private BackgroundWorker _worker = null;

        /// <summary>
        /// Whole Para
        /// </summary>
        private T _workArg = default;

        /// <summary>
        /// Timer
        /// </summary>
        private Timer _timer = null;

        /// <summary>
        /// WorkingThread
        /// </summary>
        private Thread _workerThread = null;

        /// <summary>
        /// Async time sec
        /// </summary>
        private double _workerStartDateSecond = 0.0;

        /// <summary>
        /// Async time dot
        /// </summary>
        private double _showProgressCount = 0.0;

        /// <summary>
        /// ProgressbarEx
        /// </summary
        private ProgressbarNew _progressForm = null;



        /// <summary>
        /// Express Busy
        /// </summary>
        public bool IsBusy
        {
            get
            {
                if (_worker != null)
                {
                    return _worker.IsBusy;
                }
                return false;
            }
        }

        /// <summary>
        /// 进度条提示 默认： 正在加载数据，请稍后[{0}]{1}
        /// </summary>
        public string ProgressTip { get; set; } = "等待时间[{0}]{1}";


        public CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Start 
        /// </summary>
        /// <param name="Para"></param>
        public void Start(T Para)
        {
            //if workeven is  null ,express user do not regist event
            if (Executing == null)
            {
                return;
            }

            _workerStartDateSecond = 0.0;
            _showProgressCount = 0.0;

            //init
            if (_worker != null && _worker.IsBusy)
            {
                _worker.CancelAsync();
                _worker = null;
            }

            _worker = new BackgroundWorker();

            //create progressbar
            _progressForm = new ProgressbarNew();

            //add event
            _worker.DoWork += Worker_DoWork;
            _worker.RunWorkerCompleted += Worker_RunWorkCompleted;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            //Set Whole Para
            _workArg = Para;

            //Start timer
            StartTimer();
            _worker.RunWorkerAsync(Para);

            _progressForm.StartPosition = FormStartPosition.CenterParent;
            _progressForm.ShowDialog();
        }

        /// <summary>
        /// Working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Executing == null)
            {
                e.Cancel = true;
                return;
            }

            DoWorkEventArgs<T> Argument = new DoWorkEventArgs<T>(e.Argument);

            try
            {
                if (_workerThread != null && _workerThread.IsAlive)
                {
                    _workerThread.Abort();
                }
            }
            catch (Exception)
            {
                Thread.Sleep(50);
            }

            _workerThread = new Thread(p =>
            {
                Executing?.Invoke(p as DoWorkEventArgs<T>);
            })
            {
                IsBackground = true
            };

            _workerThread.Start(Argument);

            //Maybe cpu do not start thread
            Thread.Sleep(20);

            //Wait.....
            while (_workerThread.IsAlive)
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
                if (_progressForm != null)
                {
                    _progressForm.Close();
                    _progressForm.Dispose();
                    _progressForm = null;
                }

                if (_worker != null)
                {
                    _worker.DoWork -= Worker_DoWork;
                    _worker.RunWorkerCompleted -= Worker_RunWorkCompleted;

                    try
                    {
                        if (_workerThread != null && _workerThread.IsAlive)
                        {
                            _workerThread.Abort();
                        }
                    }
                    catch (Exception) { }
                }

                if (_timer != null)
                {
                    Completed?.Invoke(new RunWorkerCompletedEventArgs<T>(e.Result, e.Error, e.Cancelled));
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
            if (_timer != null) return;

            //On one sec 
            _timer = new Timer(200);
            _timer.Elapsed += (s, e) =>
            {
                if (_progressForm != null && _progressForm.IsStop && _worker != null)
                {
                    if (_workerThread != null && _workerThread.IsAlive)
                    {
                        if (_timer != null && _timer.Enabled)
                        {
                            _timer.Stop();
                            _timer = null;
                        }
                        _workerThread.Abort();
                        _progressForm.Invoke(new Action(() =>
                        {
                            Stoped?.Invoke(new DoWorkEventArgs<T>(_workArg));
                            _progressForm.Close();
                        }));
                    }
                }

                //callback 
                _progressForm?.Invoke(new Action<DateTime>(elapsedtime =>
                {
                   
                    try
                    {
                        DateTime sTime = elapsedtime;

                        //worked time
                        _workerStartDateSecond += 0.2;
                        _progressForm?.SetProgressValue((int)_workerStartDateSecond);

                        //.....count
                        _showProgressCount += 0.2;

                        if (_showProgressCount > 6)
                        {
                            _showProgressCount = 1;
                        }

                        string[] strs = new string[(int)_showProgressCount];

                        string ProgressStr = string.Join(".", strs);
                        string ProgressText = string.Format(ProgressTip, (int)_workerStartDateSecond, ProgressStr);

                        _progressForm.TipMessage = ProgressText;
                    }
                    catch (Exception)
                    {

                    }

                }), e.SignalTime);
            };

            if (!_timer.Enabled)
            {
                _timer.Start();
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
                Executing = null;
                Completed = null;
                Stoped = null;

                _workerThread = null;
                _worker.Dispose();
                _worker = null;
                _timer = null;
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

# BackgroundWorkerEx
基于BackgroundWorker封装,自定义一个进度条显示当前的进度
It's base on BackgroundWorker encapsulation ,cutoming a ProgressBar Showing dialog

>使用Backgroundworker异步模型和ProgressBar进度条控件的二次封装,使用方式对于Backgroundworker来说是保持一致的,但是BackgroundWorkerEx封装成泛型类,所以调用的时候,必须声明一个参数类进行数据在异步中传递!

>BackgroundWorkerEx is using Backgroundworker Aysnc model and ProgressBar control to be encapsulated,that is same as  Backgroundworker about how to use it;
But BackgroundWorkerEx is a Generic class,so it is necessarily to define a para class to transfer data in Aysnc working Method when start async;

---

### 使用方式/How to use it
* 定义一个参数类 /Define a para class 
```C#
 public class ParaArg
 {
 public string Msg { get; set; }
 }
```

* 在全局中定义异步帮助类 /Define a whole Async helper
```C#
BackgroundWorkerEx<ParaArg> workHelper = null;
```
* 添加异步工作,完成,暂停事件/Add Async WorkEvent,CompletedEvent,StopedEvent
* 以下的代码是在按钮中调用/The follow code is called in a button event
``` C#
  if (workHelper != null || (workHelper != null && workHelper.IsBusy))
                {
                    workHelper.Dispose();
                    workHelper = null;
                }
                if (workHelper == null)
                {
                    workHelper = new BackgroundWorkerEx<ParaArg>();
                }

                workHelper.DoWork += (eve) =>
                {
                    ParaArg args = eve.Argument;

                    try
                    {
                        //ToDo  like Thread.Sleep(200000000000);

                        args.Msg = "...this is bussiness code result";
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
                workHelper.RunWorkCompleted += (eve) =>
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

                workHelper.WorkStoped += (eve) =>
                { 
                    //if stoped ! it means no error;
                    //just get what you want; 
                    ParaArg x = eve.Result;

                    btnBegin.Enabled = true;
                };

                //para
                ParaArg arg = new ParaArg()
                {
                    Msg = "Msg"
                };

                workHelper.AsyncStart(arg);
```


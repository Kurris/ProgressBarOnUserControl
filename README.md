# Custom-BackgroudWorker
基于BackgroundWorker封装,自定义一个进度条/文本 显示当前的进度

>这个是基于winform的程序,使用Backgroundworker异步模型和ProgressBar进度条控件的二次封装,使用方式对于Backgroundworker来说是保持一致的,但是Custom-BackgroudWorker封装成泛型类,所以调用的时候,必须声明一个参数类进行数据在异步中传递!
>

---

### 调用方式有俩种

* 使用进度条
	- [X] 模式窗体
	>顶层显示进度条控件,并且不能做任务操作,可以自主停止!可控
	- [ ] 非模式窗体
	>功能尚缺,未能做到进度条控件首次出现在调用窗体中.....待续
```C#
workerHelper.ShowMode.....=.....Progress;
workerHelper.AllowDialog=true;//模式窗体
workerHelper.TipMessage=""//默认存在,使用进度条的情况下不建议指定
workerHeper.ShowContanier=容器;
```
* 文本显示
	- [X] 完成
	>建议使用Label控件,并且将控件Text设置为空,这样可以让自定义文本显示
	>
```C#
workerHelper.ShowMode.....=.....Text;
workerHelper.TipMessage="显示内容"
workerHeper.ShowContanier=容器;
```


using System;
using System.Threading.Tasks;
using MainApp.Models.EventArgs;

namespace MainApp.Service.Abstract.Interface;

/// <summary>
///     表示日志接收功能的接口定义。
/// </summary>
public interface ILogReceived
{
    /// <summary>
    ///     表示一个事件，用于通知日志接收操作完成时触发。
    /// </summary>
    event EventHandler<LogReceivedEventArgs> OnLogReceived;

    /// <summary>
    ///     准备接收日志的方法。
    /// </summary>
    /// <returns>表示准备操作是否成功的布尔值。</returns>
    Task PrepareReceiveAsync();

    /// <summary>
    ///     开始接收日志的异步方法。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    Task StartReceiveAsync();

    /// <summary>
    ///     停止接收日志的异步方法。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    Task StopReceiveAsync();

    /// <summary>
    ///     准备接收日志的同步方法。
    /// </summary>
    void PrepareReceive();

    /// <summary>
    ///     开始接收日志的同步方法。
    /// </summary>
    void StartReceive();

    /// <summary>
    ///     停止接收日志的同步方法。
    /// </summary>
    void StopReceive();
}
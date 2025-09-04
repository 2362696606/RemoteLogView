using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using MainApp.Models.EventArgs;
using MainApp.Service.Abstract.Interface;

namespace MainApp.Service.Impl;

/// <summary>
/// 表示通过UDP接收日志的实现类。
/// </summary>
public class UdpLogReceived : ILogReceived
{
    /// <summary>
    /// 表示UDP客户端实例，用于通过UDP协议接收数据包。
    /// </summary>
    /// <remarks>
    /// 该字段是一个可空的 <see cref="UdpClient"/> 类型实例，在运行时动态创建和销毁，
    /// 主要用于处理日志的网络接收功能。
    /// - 在调用 <see cref="PrepareReceive"/> 或 <see cref="PrepareReceiveAsync"/> 方法时实例化。
    /// - 在调用 <see cref="StopReceive"/> 或 <see cref="StopReceiveAsync"/> 方法时释放资源并置空。
    /// </remarks>
    private UdpClient? _udpClient;

    /// <summary>
    /// 表示接收日志过程的运行状态。
    /// </summary>
    /// <remarks>
    /// 当值为 true 时，表示日志接收器处于运行状态；
    /// 当值为 false 时，表示日志接收器停止运行。
    /// 用于控制日志接收器的运行周期和状态管理。
    /// </remarks>
    private bool _isRunning;

    /// <summary>
    /// 表示默认的日志接收端口号。
    /// </summary>
    /// <remarks>
    /// 此常量用于指定应用程序监听日志数据的默认UDP端口号。
    /// 在<see cref="UdpLogReceived"/>类中，该端口用于初始化并监听日志消息的接收。
    /// </remarks>
    private const int DefaultPort = 8085;

    /// <summary>
    /// UdpLogReceived 是一个基于UDP的日志接收实现类，用于接收日志消息并提供相关的生命周期管理方法。
    /// </summary>
    public UdpLogReceived()
    {
        _isRunning = false;
        LocalPort = DefaultPort;
    }

    /// <summary>
    /// Gets or sets the local port number for receiving UDP logs.
    /// </summary>
    /// <remarks>
    /// This property allows external configuration of the UDP port used for log reception.
    /// The default value is set to <see cref="DefaultPort"/>.
    /// The port cannot be modified while the service is running.
    /// </remarks>
    private int _localPort;

    /// <summary>
    /// 表示用于接收UDP日志的本地端口号。
    /// </summary>
    /// <remarks>
    /// 该属性支持配置用于UDP日志接收的本地端口号，默认值为 <see cref="DefaultPort"/>。
    /// 当服务正在运行时，无法修改该端口号，尝试修改会引发 <see cref="InvalidOperationException"/>。
    /// </remarks>
    public int LocalPort
    {
        get => _localPort;
        set
        {
            if (_isRunning)
                throw new InvalidOperationException("Cannot modify port while service is running.");
            _localPort = value;
        }
    }

    /// <summary>
    /// 表示当接收到日志时触发的事件。
    /// </summary>
    /// <remarks>
    /// 该事件携带 <see cref="LogReceivedEventArgs"/> 类型的参数，
    /// 包含日志的等级（Level）、时间戳（Time）以及日志消息（Message）等详细信息。
    /// </remarks>
    public event EventHandler<LogReceivedEventArgs>? OnLogReceived;

    /// <summary>
    /// 异步准备进行日志接收的操作。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task PrepareReceiveAsync()
    {
        await Task.Run(PrepareReceive);
    }

    /// <summary>
    /// 开始接收日志的异步方法。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public Task StartReceiveAsync()
    {
        StartReceive();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 停止接收日志的异步方法。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public Task StopReceiveAsync()
    {
        StopReceive();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步处理接收到的消息数据，将其解析为日志事件并触发日志接收事件。
    /// </summary>
    /// <param name="endPoint">消息来源的远程终结点。</param>
    /// <param name="message">接收到的消息数据的字节数组。</param>
    /// <returns>表示异步操作的任务。</returns>
    private Task HandleReceivedMessageAsync(IPEndPoint endPoint, byte[] message)
    {
        try
        {
            var jsonString = System.Text.Encoding.UTF8.GetString(message);

            var logEvent = JsonSerializer.Deserialize<LogReceivedEventArgs>(jsonString);
            if (logEvent != null)
            {
                var udpLogEvent = new UdpLogReceivedEventArgs
                {
                    RemoteAddress = endPoint.Address, RemotePort = endPoint.Port,
                    Level = logEvent.Level, Time = logEvent.Time, Message = logEvent.Message
                };
                OnLogReceived?.Invoke(this, udpLogEvent);
            }
        }
        catch (JsonException)
        {
            // Invalid JSON format, ignore the message
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 准备接收日志的同步方法。
    /// </summary>
    /// <returns>如果初始化成功则返回 true，否则返回 false。</returns>
    public bool PrepareReceive()
    {
        try
        {
            _udpClient = new UdpClient(LocalPort);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 开始接收日志的同步方法。
    /// </summary>
    /// <remarks>
    /// 此方法启动一个异步任务，用于持续接收日志数据。
    /// 调用此方法前应确保已调用 <see cref="ILogReceived.PrepareReceive"/> 方法进行初始化。
    /// 如果未初始化或出现异常，日志接收操作将无法正常进行。
    /// </remarks>
    public void StartReceive()
    {
        if (_udpClient == null) return;
        _isRunning = true;
        Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    await HandleReceivedMessageAsync(result.RemoteEndPoint,result.Buffer);
                }
                catch
                {
                    if (_isRunning)
                    {
                        _isRunning = false;
                    }
                }
            }
        });
    }

    /// <summary>
    /// 停止接收日志的同步方法。
    /// </summary>
    /// <remarks>
    /// 此方法用于停止日志接收任务，同时释放相关的资源。
    /// 调用此方法后，日志接收将立即停止，并关闭当前使用的 UDP 客户端实例。
    /// </remarks>
    public void StopReceive()
    {
        _isRunning = false;
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;
    }
}

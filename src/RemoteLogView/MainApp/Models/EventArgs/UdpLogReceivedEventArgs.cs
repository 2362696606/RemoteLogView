using System.Net;

namespace MainApp.Models.EventArgs;

/// <summary>
///     表示接收到的UDP日志信息事件的相关参数。
/// </summary>
public class UdpLogReceivedEventArgs : LogReceivedEventArgs
{
    /// <summary>
    ///     获取或设置远程发送日志数据的IP地址。
    ///     表示生成此事件的主机地址信息，用于区分日志来源。
    /// </summary>
    public IPAddress? RemoteAddress { get; set; }

    /// <summary>
    ///     获取或设置远程终端发送日志的端口号。
    /// </summary>
    public int RemotePort { get; set; }
}
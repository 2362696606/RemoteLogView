using System;

namespace LogViewService.Models.EventArgs;

/// <summary>
/// 表示日志事件的参数类，封装了日志相关信息。
/// </summary>
public class LogReceivedEventArgs:System.EventArgs
{
    /// <summary>
    /// 表示日志事件的日志级别。
    /// </summary>
    /// <remarks>
    /// 日志级别用于标识日志的重要性或严重性级别，例如调试信息(Debug)、普通信息(Info)、警告(Warn)、错误(Error)、致命错误(Fatal)等。
    /// </remarks>
    public LogLevel Level { get; set; }

    /// <summary>
    /// 表示日志事件的发生时间。
    /// </summary>
    /// <remarks>
    /// 此属性存储了一个 <see cref="DateTime"/> 对象，表示日志记录生成的具体时间点。
    /// 可用于排序日志条目或记录日志生成的时间信息。
    /// </remarks>
    public DateTime Time { get; set; }

    /// <summary>
    /// 获取或设置日志消息的内容。
    /// 此属性表示日志事件中记录的文本信息。
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

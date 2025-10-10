using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MainApp.CustomControl;
using MainApp.Models.EventArgs;
using MainApp.Service.Abstract.Interface;

namespace MainApp.ViewModels;

/// <summary>
///     HomeViewModel 是一个负责处理日常日志接收操作及相关逻辑的视图模型。
/// </summary>
/// <remarks>
///     此类主要功能包括过滤日志数据、支持正则表达式过滤选项、处理日志接收事件，并通过命令实现相关操作如刷新与清除。
/// </remarks>
public class HomeViewModel : ObservableObject
{
    /// <summary>
    ///     表示一个日志接收服务实例，该实例用于处理日志接收功能。
    /// </summary>
    /// <remarks>
    ///     此变量用于存储实现了 <see cref="ILogReceived" /> 接口的对象实例。
    ///     主要负责通过订阅 <see cref="ILogReceived.OnLogReceived" /> 事件以实时处理日志数据。
    ///     在类中，该实例通常用于初始化日志处理逻辑，例如绑定事件和接收日志流数据。
    /// </remarks>
    private readonly ILogReceived _logReceived;

    /// <summary>
    ///     表示一个用于存储日志数据的可观察集合。
    /// </summary>
    /// <remarks>
    ///     此变量存储类型为 <see cref="LogReceivedEventArgs" /> 的对象集合，
    ///     主要用于管理从日志接收服务中收到的日志数据。每个集合项代表一条日志记录，
    ///     包含日志级别、时间戳及消息内容等信息。该集合通常与 UI 绑定，用于展示或更新日志条目。
    ///     _logs 会在日志事件接收器 <see cref="ILogReceived.OnLogReceived" /> 中动态更新，
    ///     并以先进先出的方式维护集合大小（当日志条目超过约束数量时，会移除旧日志）。
    /// </remarks>
    private readonly ObservableCollection<LogReceivedEventArgs> _logs = new();

    /// <summary>
    ///     用于存储筛选字符串的字段。
    /// </summary>
    /// <remarks>
    ///     此字段用作日志列表的过滤依据。通过设置筛选字符串，可以对日志集合中的条目进行筛选操作。
    ///     如果为空字符串，则禁用过滤功能；否则，将根据提供的关键字或正则表达式筛选符合条件的日志记录。
    ///     该字段的值通过 <c>FilterString</c> 属性进行访问和修改。
    /// </remarks>
    private string _filterString = string.Empty;

    /// <summary>
    ///     表示一个布尔值，指示是否启用正则表达式筛选功能。
    /// </summary>
    /// <remarks>
    ///     当该变量为 true 时，日志筛选功能将以正则表达式作为匹配规则，对日志信息进行筛选。
    ///     如果为 false，则使用普通字符串匹配方式进行筛选。
    ///     此变量的更改会影响日志过滤逻辑的具体执行方式。
    /// </remarks>
    private bool _regexEnable;

    /// <summary>
    ///     输入逻辑的视图模型，用于处理日志接收和过滤操作。
    /// </summary>
    /// <remarks>
    ///     该类与日志接收接口 <see cref="ILogReceived" /> 配合使用，支持日志数据的实时处理、过滤和显示功能。
    ///     它通过数据绑定与用户界面交互，为用户提供高效的日志查看和管理解决方案。
    /// </remarks>
    public HomeViewModel(ILogReceived logReceived)
    {
        _logReceived = logReceived;
        _logReceived.OnLogReceived += OnLogReceived;
        LogsView = new DataGridCollectionView(_logs)
        {
            Filter = FilterLog
        };
    }

    /// <summary>
    ///     表示用户输入的筛选字符串，用于过滤日志列表。
    /// </summary>
    /// <remarks>
    ///     此属性用于存储和获取筛选条件，支持关键字匹配或正则表达式匹配功能。
    ///     当设置一个新值时，日志过滤逻辑会根据新地筛选字符串实时更新显示的日志集合。
    ///     配合 <c>RegexEnable</c> 属性，可选择使用普通文本匹配或正则表达式匹配方式。
    /// </remarks>
    public string FilterString
    {
        get => _filterString;
        set => SetProperty(ref _filterString, value);
    }

    /// <summary>
    ///     表示一个布尔值，指示日志筛选功能是否启用正则表达式匹配模式。
    /// </summary>
    /// <remarks>
    ///     当此属性设置为 true 时，日志筛选功能将使用正则表达式对日志信息进行匹配；
    ///     当设置为 false 时，将使用普通的字符串匹配方式。此属性主要用于切换筛选逻辑，
    /// </remarks>
    public bool RegexEnable
    {
        get => _regexEnable;
        set => SetProperty(ref _regexEnable, value);
    }

    /// <summary>
    ///     表示用于展示日志数据的视图集合。
    /// </summary>
    /// <remarks>
    ///     LogsView 是一个只读属性，其类型为 <see cref="DataGridCollectionView" />，用于绑定和展示日志数据集合。
    ///     日志数据通过 <see cref="ILogReceived.OnLogReceived" /> 事件动态更新，并与用户界面绑定以实现实时显示和过滤功能。
    ///     此属性默认情况下包含一个自定义过滤逻辑，该逻辑可通过用户提供的 <see cref="FilterString" /> 或 <see cref="RegexEnable" /> 进行动态调整。
    ///     结合 UI 元素，如 <see cref="AutoScrollListBox" />，可以实现日志的自动滚动展示。
    /// </remarks>
    public DataGridCollectionView LogsView { get; }

    /// <summary>
    ///     表示用于刷新日志视图的异步命令。
    /// </summary>
    /// <remarks>
    ///     <para>此命令绑定到视图模型中的刷新逻辑，例如在日志过滤条件改变或需要手动刷新视图时调用。</para>
    ///     <para>通过与 <see cref="AsyncRelayCommand" /> 结合，提供了异步执行支持，从而避免阻塞主线程。</para>
    ///     <para>在视图中可通过绑定机制使用，例如配合输入控件触发刷新操作。</para>
    /// </remarks>
    public AsyncRelayCommand RefreshCommand => new(DoRefresh);

    /// <summary>
    ///     表示用于清空日志列表的命令。
    /// </summary>
    /// <remarks>
    ///     ClearCommand 是一个异步命令，用于清空当前的日志集合 <c>_logs</c>，释放内存并重置用户界面的日志显示内容。
    ///     此命令绑定于视图中的“清空”按钮，当用户点击按钮时执行 <see cref="DoClear" /> 方法完成清空操作。
    ///     使用该命令可以快速清除日志记录，解决日志堆积影响性能的问题。
    /// </remarks>
    public AsyncRelayCommand ClearCommand => new(DoClear);

    /// <summary>
    ///     根据指定的过滤条件筛选日志的逻辑实现方法。
    /// </summary>
    /// <param name="arg">需要筛选的对象，通常为 <see cref="LogReceivedEventArgs" /> 类型。</param>
    /// <returns>如果对象满足当前的过滤条件，则返回 true；否则返回 false。</returns>
    private bool FilterLog(object arg)
    {
        if (string.IsNullOrEmpty(FilterString)) return true;

        if (arg is LogReceivedEventArgs log)
        {
            if (RegexEnable)
            {
                var regex = new Regex(FilterString);
                return regex.IsMatch(log.Message);
            }

            return log.Message.Contains(FilterString);
        }

        return false;
    }

    /// <summary>
    ///     清空日志集合的方法。
    /// </summary>
    /// <returns>
    ///     表示异步操作的任务对象。
    /// </returns>
    private Task DoClear()
    {
        _logs.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     刷新日志视图的异步方法，用于重新加载和更新当前日志显示。
    /// </summary>
    /// <returns> 返回一个已完成的任务，表示日志刷新操作已成功执行。</returns>
    private Task DoRefresh()
    {
        LogsView.Refresh();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     当接收到新的日志时触发的事件处理方法。
    /// </summary>
    /// <param name="sender">事件的发送者对象。</param>
    /// <param name="e">包含日志信息的 <see cref="LogReceivedEventArgs" /> 实例。</param>
    private void OnLogReceived(object? sender, LogReceivedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_logs.Count > 1000) _logs.RemoveAt(0);

            _logs.Add(e);
        });
    }
}
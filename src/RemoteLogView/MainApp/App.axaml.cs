using Avalonia;
using Avalonia.Markup.Xaml;
using MainApp.Service.Abstract.Interface;
using MainApp.Service.Impl;
using MainApp.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Navigation.Regions;

namespace MainApp;

public class App : PrismApplication
{
    /// <summary>
    /// 初始化应用程序的核心功能。
    /// </summary>
    /// <remarks>
    /// 在初始化过程中加载应用程序的 XAML 资源，确保应用程序界面正确渲染。
    /// 此方法需要调用基类的 Initialize 方法，以确保框架的基本功能完整性。
    /// </remarks>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Required when overriding Initialize
        base.Initialize();
    }

    /// <summary>
    /// 创建主框架窗口的实例。
    /// </summary>
    /// <returns>返回主窗口的实例，作为应用程序的主视图。</returns>
    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <summary>
    /// 重写的OnInitialized方法，用于在应用程序初始化完成后执行自定义逻辑。
    /// </summary>
    /// <remarks>
    /// 此方法在调用基础类的初始化逻辑后执行。可以通过在此方法中注册区域或执行其他需要在应用程序初始化后启动的操作。
    /// </remarks>
    protected override void OnInitialized()
    {
        IRegionManager regionManager = Container.Resolve<IRegionManager>();
        RegisterRegion(regionManager);
        base.OnInitialized();
    }

    /// <summary>
    /// 注册区域到区域管理器。
    /// </summary>
    /// <param name="regionManager">用于管理视图区域的区域管理器实例。</param>
    private void RegisterRegion(IRegionManager regionManager)
    {
        regionManager.RegisterViewWithRegion<HomeView>("HomeRegion");
    }

    /// <summary>
    /// 注册应用程序服务、视图和对话框的依赖关系容器。
    /// </summary>
    /// <param name="containerRegistry">容器注册器，用于注册应用程序的类型和服务。</param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register you Services, Views, Dialogs, etc.
        
        containerRegistry.RegisterSingleton<ILogReceived>(() =>
        {
            var udpLogReceived = new UdpLogReceived();
            udpLogReceived.PrepareReceive();
            udpLogReceived.StartReceive();
            return udpLogReceived;
        });
    }
}
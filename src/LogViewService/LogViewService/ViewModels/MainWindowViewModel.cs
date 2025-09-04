using DryIoc;
using Prism.Commands;
using Prism.Dialogs;

namespace LogViewService.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private string _returnedResult = string.Empty;

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;

        Title = "日志服务";
    }
}

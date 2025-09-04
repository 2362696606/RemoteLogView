using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;

namespace LogViewService.CustomControl;

public class AutoScrollListBox : ListBox
{
    public static readonly StyledProperty<bool> AutoScrollEnabledProperty =
        AvaloniaProperty.Register<AutoScrollListBox, bool>(
            nameof(AutoScrollEnabled),
            defaultValue: false);

    protected override Type StyleKeyOverride => typeof(ListBox);

    public bool AutoScrollEnabled
    {
        get => GetValue(AutoScrollEnabledProperty);
        set => SetValue(AutoScrollEnabledProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AutoScrollEnabledProperty)
        {
            if (change.NewValue is true)
            {
                Items.CollectionChanged += Items_CollectionChanged;
            }
            else
            {
                Items.CollectionChanged -= Items_CollectionChanged;
            }
        }
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // if (AutoScrollEnabled && e.Action == NotifyCollectionChangedAction.Add)
        // {
        //
        // }
        ScrollIntoView(Items.Count - 1);
    }
}

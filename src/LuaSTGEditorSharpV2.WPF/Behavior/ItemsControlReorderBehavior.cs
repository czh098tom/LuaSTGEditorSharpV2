using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using PropertyTools.Wpf;

namespace LuaSTGEditorSharpV2.WPF.Behavior;

public sealed record ItemsControlReorderRequest(object Item, int InsertionIndex);

public sealed class ItemsControlReorderBehavior : Behavior<ItemsControl>
{
    private const string DragDataFormat = "LuaSTGEditorSharpV2.ItemsControlReorder";

    public static readonly DependencyProperty ReorderCommandProperty =
        DependencyProperty.Register(
            nameof(ReorderCommand),
            typeof(ICommand),
            typeof(ItemsControlReorderBehavior));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(ItemsControlReorderBehavior),
            new PropertyMetadata(System.Windows.Controls.Orientation.Vertical));

    private bool _originalAllowDrop;
    private Point? _dragStart;
    private object? _draggedItem;
    private FrameworkElement? _targetContainer;
    private InsertionAdorner? _insertionAdorner;

    public ICommand? ReorderCommand
    {
        get => (ICommand?)GetValue(ReorderCommandProperty);
        set => SetValue(ReorderCommandProperty, value);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        _originalAllowDrop = AssociatedObject.AllowDrop;
        AssociatedObject.SetCurrentValue(UIElement.AllowDropProperty, true);
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.PreviewDragOver += OnPreviewDragOver;
        AssociatedObject.PreviewDragLeave += OnPreviewDragLeave;
        AssociatedObject.PreviewDrop += OnPreviewDrop;
    }

    protected override void OnDetaching()
    {
        RemoveInsertionAdorner();
        ResetDrag();
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.PreviewDragOver -= OnPreviewDragOver;
        AssociatedObject.PreviewDragLeave -= OnPreviewDragLeave;
        AssociatedObject.PreviewDrop -= OnPreviewDrop;
        AssociatedObject.SetCurrentValue(UIElement.AllowDropProperty, _originalAllowDrop);
        base.OnDetaching();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(AssociatedObject);
        _draggedItem = GetItem(e.OriginalSource as DependencyObject);
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        => ResetDrag();

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            ResetDrag();
            return;
        }

        if (_dragStart is not { } dragStart
            || _draggedItem is null
            || ReorderCommand is null)
        {
            return;
        }

        var currentPosition = e.GetPosition(AssociatedObject);
        if (Math.Abs(currentPosition.X - dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(currentPosition.Y - dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var payload = new DragPayload(AssociatedObject, _draggedItem);
        var data = new DataObject(DragDataFormat, payload);
        try
        {
            DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);
        }
        finally
        {
            RemoveInsertionAdorner();
            ResetDrag();
        }
    }

    private void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        if (!TryGetPayload(e.Data, out var payload))
        {
            return;
        }

        e.Handled = true;
        if (!ReferenceEquals(payload.Owner, AssociatedObject)
            || !TryUpdateDropTarget(e, payload.Item, out var request)
            || ReorderCommand?.CanExecute(request) != true)
        {
            RemoveInsertionAdorner();
            e.Effects = DragDropEffects.None;
            return;
        }

        e.Effects = DragDropEffects.Move;
    }

    private void OnPreviewDragLeave(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DragDataFormat))
        {
            return;
        }

        RemoveInsertionAdorner();
        e.Handled = true;
    }

    private void OnPreviewDrop(object sender, DragEventArgs e)
    {
        if (!TryGetPayload(e.Data, out var payload))
        {
            return;
        }

        e.Handled = true;
        if (ReferenceEquals(payload.Owner, AssociatedObject)
            && TryUpdateDropTarget(e, payload.Item, out var request)
            && ReorderCommand?.CanExecute(request) == true)
        {
            ReorderCommand.Execute(request);
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        RemoveInsertionAdorner();
    }

    private bool TryUpdateDropTarget(
        DragEventArgs e,
        object item,
        out ItemsControlReorderRequest request)
    {
        var container = GetContainer(e.OriginalSource as DependencyObject);
        if (container is null && AssociatedObject.Items.Count > 0)
        {
            container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(
                AssociatedObject.Items.Count - 1) as FrameworkElement;
        }

        if (container is null)
        {
            request = null!;
            return false;
        }

        var position = e.GetPosition(container);
        var insertBefore = Orientation == System.Windows.Controls.Orientation.Vertical
            ? position.Y < container.ActualHeight / 2
            : position.X < container.ActualWidth / 2;
        var targetIndex = AssociatedObject.ItemContainerGenerator.IndexFromContainer(container);
        if (targetIndex < 0)
        {
            request = null!;
            return false;
        }

        request = new ItemsControlReorderRequest(
            item,
            targetIndex + (insertBefore ? 0 : 1));
        UpdateInsertionAdorner(container, insertBefore);
        return true;
    }

    private FrameworkElement? GetContainer(DependencyObject? source)
    {
        if (source is null)
        {
            return null;
        }

        return ItemsControl.ContainerFromElement(AssociatedObject, source) as FrameworkElement;
    }

    private object? GetItem(DependencyObject? source)
    {
        var container = GetContainer(source);
        if (container is null)
        {
            return null;
        }

        var item = AssociatedObject.ItemContainerGenerator.ItemFromContainer(container);
        return ReferenceEquals(item, DependencyProperty.UnsetValue) ? null : item;
    }

    private void UpdateInsertionAdorner(FrameworkElement container, bool insertBefore)
    {
        if (ReferenceEquals(_targetContainer, container) && _insertionAdorner is not null)
        {
            _insertionAdorner.IsInFirstHalf = insertBefore;
            _insertionAdorner.InvalidateVisual();
            return;
        }

        RemoveInsertionAdorner();
        var adornerLayer = AdornerLayer.GetAdornerLayer(container);
        if (adornerLayer is null)
        {
            return;
        }

        _targetContainer = container;
        _insertionAdorner = new InsertionAdorner(
            Orientation == System.Windows.Controls.Orientation.Vertical,
            insertBefore,
            container,
            adornerLayer);
    }

    private void RemoveInsertionAdorner()
    {
        _insertionAdorner?.Detach();
        _insertionAdorner = null;
        _targetContainer = null;
    }

    private void ResetDrag()
    {
        _dragStart = null;
        _draggedItem = null;
    }

    private static bool TryGetPayload(IDataObject data, out DragPayload payload)
    {
        var value = data.GetDataPresent(DragDataFormat)
            ? data.GetData(DragDataFormat) as DragPayload
            : null;
        if (value is null)
        {
            payload = null!;
            return false;
        }

        payload = value;
        return true;
    }

    private sealed record DragPayload(ItemsControl Owner, object Item);
}

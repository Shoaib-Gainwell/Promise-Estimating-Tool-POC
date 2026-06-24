using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC;

public partial class MainWindow : Window
{
    // Undo stack for deleted items
    private readonly Stack<UndoAction> _undoStack = new();
    private bool _sidebarVisible = true;

    public MainWindow()
    {
        InitializeComponent();

        // When weighted values are changed in Settings, refresh all component base hours
        WeightedValues.ValuesChanged += () =>
        {
            if (DataContext is MainViewModel vm)
            {
                foreach (var c in vm.Components)
                    c.UpdateBaseHours();
            }
        };

        // Auto-focus Req # cell when a new component is added
        if (DataContext is MainViewModel mainVm)
        {
            mainVm.Components.CollectionChanged += (_, args) =>
            {
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        var lastIndex = mainVm.Components.Count - 1;
                        ComponentsGrid.ScrollIntoView(mainVm.Components[lastIndex]);
                        ComponentsGrid.CurrentCell = new DataGridCellInfo(
                            mainVm.Components[lastIndex],
                            ComponentsGrid.Columns[1]); // Column 1 = Req #
                        ComponentsGrid.BeginEdit();
                    });
                }
            };

            mainVm.CollaborationItems.CollectionChanged += (_, args) =>
            {
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        var lastIndex = mainVm.CollaborationItems.Count - 1;
                        CollaborationGrid.ScrollIntoView(mainVm.CollaborationItems[lastIndex]);
                        CollaborationGrid.CurrentCell = new DataGridCellInfo(
                            mainVm.CollaborationItems[lastIndex],
                            CollaborationGrid.Columns[1]); // Column 1 = Task Name
                        CollaborationGrid.BeginEdit();
                    });
                }
            };
        }
    }

    // ═══════════ KEYBOARD SHORTCUTS ═══════════

    private void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e) => PerformSave();
    private void OnNewComponentExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Context-aware: add item to whichever tab is active
            if (MainTabControl.SelectedIndex == 1) // Collaboration tab
                vm.AddCollaborationItemCommand.Execute(null);
            else
                vm.AddComponentCommand.Execute(null);
        }
    }

    private void OnUndoExecuted(object sender, ExecutedRoutedEventArgs e) => PerformUndo();
    private void OnCanUndo(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _undoStack.Count > 0;

    private void OnDeleteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        // Context-aware: delete from whichever grid is active
        if (MainTabControl.SelectedIndex == 1 && CollaborationGrid.CurrentItem is CollaborationRowViewModel collabItem)
        {
            _undoStack.Push(new UndoAction(UndoType.CollaborationDelete, null, collabItem));
            vm.RemoveCollaborationItemCommand.Execute(collabItem);
            ShowToast("Item removed (Ctrl+Z to undo)", false);
        }
        else if (ComponentsGrid.CurrentItem is ComponentRowViewModel component)
        {
            PushUndoComponent(component);
            vm.RemoveComponentCommand.Execute(component);
            ShowToast("Component removed (Ctrl+Z to undo)", false);
        }
    }

    // ═══════════ SIDEBAR TOGGLE ═══════════

    private void OnToggleSidebar(object sender, RoutedEventArgs e)
    {
        _sidebarVisible = !_sidebarVisible;
        var column = SidebarColumn;

        if (_sidebarVisible)
        {
            column.Width = new GridLength(330);
            SidebarToggleText.Text = "◀";
            SidebarToggleButton.ToolTip = "Hide sidebar (summary panel)";
        }
        else
        {
            column.Width = new GridLength(0);
            SidebarToggleText.Text = "▶";
            SidebarToggleButton.ToolTip = "Show sidebar (summary panel)";
        }
    }

    // ═══════════ SAVE ═══════════

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => PerformSave();

    private void PerformSave()
    {
        if (DataContext is MainViewModel vm)
        {
            var result = vm.SaveProject();
            if (result != null)
                ShowToast(result, true);
            else
                ShowToast("Project saved successfully!", false);
        }
    }

    private void OnHistoryClick(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow { Owner = this };
        if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.LoadProject(historyWindow.SelectedProject);
                ShowToast("Project loaded", false);
            }
        }
    }

    // ═══════════ CLEAR ALL WITH CONFIRMATION ═══════════

    private void OnClearAllClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (vm.Components.Count == 0 && vm.CollaborationItems.Count == 0) return;

        // Show inline confirmation
        ConfirmOverlay.Visibility = Visibility.Visible;
    }

    private void OnConfirmClearYes(object sender, RoutedEventArgs e)
    {
        ConfirmOverlay.Visibility = Visibility.Collapsed;
        if (DataContext is MainViewModel vm)
        {
            // Push all components and collab items to undo
            foreach (var c in vm.Components.ToList())
                _undoStack.Push(new UndoAction(UndoType.ComponentDelete, c, null));
            foreach (var c in vm.CollaborationItems.ToList())
                _undoStack.Push(new UndoAction(UndoType.CollaborationDelete, null, c));

            vm.ClearAllCommand.Execute(null);
            ShowToast("All items cleared (Ctrl+Z to undo)", false);
        }
    }

    private void OnConfirmClearNo(object sender, RoutedEventArgs e)
    {
        ConfirmOverlay.Visibility = Visibility.Collapsed;
    }

    // ═══════════ DELETE WITH UNDO ═══════════

    private void OnDeleteComponentClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ComponentRowViewModel component)
        {
            if (DataContext is MainViewModel vm)
            {
                PushUndoComponent(component);
                vm.RemoveComponentCommand.Execute(component);
                ShowToast("Component removed (Ctrl+Z to undo)", false);
            }
        }
    }

    private void OnDeleteCollaborationClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is CollaborationRowViewModel item)
        {
            if (DataContext is MainViewModel vm)
            {
                _undoStack.Push(new UndoAction(UndoType.CollaborationDelete, null, item));
                vm.RemoveCollaborationItemCommand.Execute(item);
                ShowToast("Item removed (Ctrl+Z to undo)", false);
            }
        }
    }

    // ═══════════ UNDO ═══════════

    private void PushUndoComponent(ComponentRowViewModel component)
    {
        _undoStack.Push(new UndoAction(UndoType.ComponentDelete, component, null));
    }

    private void PerformUndo()
    {
        if (_undoStack.Count == 0) return;
        if (DataContext is not MainViewModel vm) return;

        var action = _undoStack.Pop();
        switch (action.Type)
        {
            case UndoType.ComponentDelete when action.Component != null:
                action.Component.PropertyChanged += (_, _) => { }; // reconnect
                vm.Components.Add(action.Component);
                ShowToast("Undo: component restored", false);
                break;
            case UndoType.CollaborationDelete when action.CollaborationItem != null:
                vm.CollaborationItems.Add(action.CollaborationItem);
                ShowToast("Undo: item restored", false);
                break;
        }
    }

    // ═══════════ TOAST NOTIFICATIONS ═══════════

    private System.Timers.Timer? _toastTimer;

    private void ShowToast(string message, bool isError)
    {
        Dispatcher.Invoke(() =>
        {
            ToastText.Text = message;
            ToastPanel.Background = new SolidColorBrush(
                isError ? (Color)ColorConverter.ConvertFromString("#FEF2F2")!
                        : (Color)ColorConverter.ConvertFromString("#F0FDF4")!);
            ToastPanel.BorderBrush = new SolidColorBrush(
                isError ? (Color)ColorConverter.ConvertFromString("#FECACA")!
                        : (Color)ColorConverter.ConvertFromString("#BBF7D0")!);
            ToastIcon.Text = isError ? "⚠" : "✓";
            ToastIcon.Foreground = new SolidColorBrush(
                isError ? (Color)ColorConverter.ConvertFromString("#DC2626")!
                        : (Color)ColorConverter.ConvertFromString("#16A34A")!);

            ToastPanel.Visibility = Visibility.Visible;

            // Fade in
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            ToastPanel.BeginAnimation(OpacityProperty, fadeIn);
        });

        // Auto-dismiss after 3 seconds
        _toastTimer?.Stop();
        _toastTimer = new System.Timers.Timer(3000) { AutoReset = false };
        _toastTimer.Elapsed += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                fadeOut.Completed += (_, _) => ToastPanel.Visibility = Visibility.Collapsed;
                ToastPanel.BeginAnimation(OpacityProperty, fadeOut);
            });
        };
        _toastTimer.Start();
    }

    // ═══════════ SINGLE-CLICK EDITING ═══════════

    private void OnUndoClick(object sender, RoutedEventArgs e) => PerformUndo();

    private void ComponentsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SingleClickEdit(ComponentsGrid, e);
    }

    private void CollaborationGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SingleClickEdit(CollaborationGrid, e);
    }

    private void SingleClickEdit(DataGrid grid, MouseButtonEventArgs e)
    {
        var originalSource = e.OriginalSource as DependencyObject;
        if (originalSource == null) return;

        DataGridCell? cell = null;
        var current = originalSource;
        while (current != null && current is not DataGrid)
        {
            if (current is DataGridCell foundCell)
            {
                cell = foundCell;
                break;
            }
            current = VisualTreeHelper.GetParent(current);
        }

        if (cell == null || cell.IsEditing || cell.IsReadOnly) return;

        if (!cell.IsFocused)
            cell.Focus();

        grid.CurrentCell = new DataGridCellInfo(cell);

        if (!cell.IsEditing)
            grid.BeginEdit(e);
    }
}

// ═══════════ UNDO TYPES ═══════════

public enum UndoType { ComponentDelete, CollaborationDelete }

public record UndoAction(
    UndoType Type,
    ComponentRowViewModel? Component,
    CollaborationRowViewModel? CollaborationItem);

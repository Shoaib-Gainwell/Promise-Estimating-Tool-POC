using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC;

public partial class MainWindow : Window
{
    // Undo stack for all actions
    private readonly Stack<UndoAction> _undoStack = new();
    private bool _sidebarVisible = true;
    private bool _suppressUndo; // prevents undo recording during undo/redo operations

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
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && args.NewItems != null)
                {
                    foreach (ComponentRowViewModel row in args.NewItems)
                        SubscribeComponentForUndo(row);

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
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && args.OldItems != null)
                {
                    foreach (ComponentRowViewModel row in args.OldItems)
                        UnsubscribeComponentForUndo(row);
                }
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    // Clear all subscriptions tracked in _subscribedComponents
                    foreach (var row in _subscribedComponents.ToList())
                        UnsubscribeComponentForUndo(row);
                }
            };

            mainVm.CollaborationItems.CollectionChanged += (_, args) =>
            {
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && args.NewItems != null)
                {
                    foreach (CollaborationRowViewModel row in args.NewItems)
                        SubscribeCollaborationForUndo(row);

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
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && args.OldItems != null)
                {
                    foreach (CollaborationRowViewModel row in args.OldItems)
                        UnsubscribeCollaborationForUndo(row);
                }
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    foreach (var row in _subscribedCollaborations.ToList())
                        UnsubscribeCollaborationForUndo(row);
                }
            };

            // Subscribe existing items (e.g., default collaboration rows)
            foreach (var row in mainVm.CollaborationItems)
                SubscribeCollaborationForUndo(row);
        }
    }

    // ═══════════ UNDO PROPERTY TRACKING ═══════════

    private readonly HashSet<ComponentRowViewModel> _subscribedComponents = new();
    private readonly HashSet<CollaborationRowViewModel> _subscribedCollaborations = new();

    // Stores "about to change" values captured by PropertyChanging
    private readonly Dictionary<object, (string Property, object? Value)> _pendingChanges = new();

    private void SubscribeComponentForUndo(ComponentRowViewModel row)
    {
        if (_subscribedComponents.Add(row))
        {
            row.PropertyChanging += OnComponentPropertyChanging;
            row.PropertyChanged += OnComponentPropertyChangedForUndo;
        }
    }

    private void UnsubscribeComponentForUndo(ComponentRowViewModel row)
    {
        if (_subscribedComponents.Remove(row))
        {
            row.PropertyChanging -= OnComponentPropertyChanging;
            row.PropertyChanged -= OnComponentPropertyChangedForUndo;
        }
    }

    private void SubscribeCollaborationForUndo(CollaborationRowViewModel row)
    {
        if (_subscribedCollaborations.Add(row))
        {
            row.PropertyChanging += OnCollaborationPropertyChanging;
            row.PropertyChanged += OnCollaborationPropertyChangedForUndo;
        }
    }

    private void UnsubscribeCollaborationForUndo(CollaborationRowViewModel row)
    {
        if (_subscribedCollaborations.Remove(row))
        {
            row.PropertyChanging -= OnCollaborationPropertyChanging;
            row.PropertyChanged -= OnCollaborationPropertyChangedForUndo;
        }
    }

    private static readonly HashSet<string> _trackedComponentProps = new()
    {
        nameof(ComponentRowViewModel.RequirementId),
        nameof(ComponentRowViewModel.ComponentType),
        nameof(ComponentRowViewModel.Description),
        nameof(ComponentRowViewModel.ChangeType),
        nameof(ComponentRowViewModel.Size),
        nameof(ComponentRowViewModel.Count),
        nameof(ComponentRowViewModel.Notes)
    };

    private static readonly HashSet<string> _trackedCollabProps = new()
    {
        nameof(CollaborationRowViewModel.TaskName),
        nameof(CollaborationRowViewModel.CollabType),
        nameof(CollaborationRowViewModel.NumberOfMeetings),
        nameof(CollaborationRowViewModel.MeetingDurationMinutes),
        nameof(CollaborationRowViewModel.NumberOfParticipants),
        nameof(CollaborationRowViewModel.ParticipantPrepTimeMinutes),
        nameof(CollaborationRowViewModel.Notes)
    };

    private void OnComponentPropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (_suppressUndo || sender is not ComponentRowViewModel row || e.PropertyName == null) return;
        if (!_trackedComponentProps.Contains(e.PropertyName)) return;

        object? oldValue = e.PropertyName switch
        {
            nameof(ComponentRowViewModel.RequirementId) => row.RequirementId,
            nameof(ComponentRowViewModel.ComponentType) => row.ComponentType,
            nameof(ComponentRowViewModel.Description) => row.Description,
            nameof(ComponentRowViewModel.ChangeType) => row.ChangeType,
            nameof(ComponentRowViewModel.Size) => row.Size,
            nameof(ComponentRowViewModel.Count) => row.Count,
            nameof(ComponentRowViewModel.Notes) => row.Notes,
            _ => null
        };
        _pendingChanges[row] = (e.PropertyName, oldValue);
    }

    private void OnComponentPropertyChangedForUndo(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_suppressUndo || sender is not ComponentRowViewModel row || e.PropertyName == null) return;
        if (!_pendingChanges.TryGetValue(row, out var pending)) return;
        if (pending.Property != e.PropertyName) return;

        _pendingChanges.Remove(row);
        _undoStack.Push(new UndoAction(UndoType.ComponentPropertyChange, row, null, e.PropertyName, pending.Value));
    }

    private void OnCollaborationPropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (_suppressUndo || sender is not CollaborationRowViewModel row || e.PropertyName == null) return;
        if (!_trackedCollabProps.Contains(e.PropertyName)) return;

        object? oldValue = e.PropertyName switch
        {
            nameof(CollaborationRowViewModel.TaskName) => row.TaskName,
            nameof(CollaborationRowViewModel.CollabType) => row.CollabType,
            nameof(CollaborationRowViewModel.NumberOfMeetings) => row.NumberOfMeetings,
            nameof(CollaborationRowViewModel.MeetingDurationMinutes) => row.MeetingDurationMinutes,
            nameof(CollaborationRowViewModel.NumberOfParticipants) => row.NumberOfParticipants,
            nameof(CollaborationRowViewModel.ParticipantPrepTimeMinutes) => row.ParticipantPrepTimeMinutes,
            nameof(CollaborationRowViewModel.Notes) => row.Notes,
            _ => null
        };
        _pendingChanges[row] = (e.PropertyName, oldValue);
    }

    private void OnCollaborationPropertyChangedForUndo(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_suppressUndo || sender is not CollaborationRowViewModel row || e.PropertyName == null) return;
        if (!_pendingChanges.TryGetValue(row, out var pending)) return;
        if (pending.Property != e.PropertyName) return;

        _pendingChanges.Remove(row);
        _undoStack.Push(new UndoAction(UndoType.CollaborationPropertyChange, null, row, e.PropertyName, pending.Value));
    }

    // ═══════════ KEYBOARD SHORTCUTS ═══════════

    private void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e) => PerformSave();
    private void OnNewComponentExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Context-aware: add item to whichever tab is active
            if (MainTabControl.SelectedIndex == 1) // Collaboration tab
            {
                vm.AddCollaborationItemCommand.Execute(null);
                var added = vm.CollaborationItems[^1];
                _undoStack.Push(new UndoAction(UndoType.CollaborationAdd, null, added));
            }
            else
            {
                vm.AddComponentCommand.Execute(null);
                var added = vm.Components[^1];
                _undoStack.Push(new UndoAction(UndoType.ComponentAdd, added, null));
            }
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
            int index = vm.CollaborationItems.IndexOf(collabItem);
            _undoStack.Push(new UndoAction(UndoType.CollaborationDelete, null, collabItem, InsertIndex: index));
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

    private void OnAddComponentClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.AddComponentCommand.Execute(null);
            var added = vm.Components[^1];
            _undoStack.Push(new UndoAction(UndoType.ComponentAdd, added, null));
        }
    }

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
                int index = vm.CollaborationItems.IndexOf(item);
                _undoStack.Push(new UndoAction(UndoType.CollaborationDelete, null, item, InsertIndex: index));
                vm.RemoveCollaborationItemCommand.Execute(item);
                ShowToast("Item removed (Ctrl+Z to undo)", false);
            }
        }
    }

    // ═══════════ UNDO ═══════════

    private void PushUndoComponent(ComponentRowViewModel component)
    {
        if (DataContext is MainViewModel vm)
        {
            int index = vm.Components.IndexOf(component);
            _undoStack.Push(new UndoAction(UndoType.ComponentDelete, component, null, InsertIndex: index));
        }
    }

    private void PerformUndo()
    {
        if (_undoStack.Count == 0) return;
        if (DataContext is not MainViewModel vm) return;

        _suppressUndo = true;
        try
        {
            var action = _undoStack.Pop();
            switch (action.Type)
            {
                case UndoType.ComponentDelete when action.Component != null:
                    var idx = action.InsertIndex >= 0 && action.InsertIndex <= vm.Components.Count
                        ? action.InsertIndex : vm.Components.Count;
                    vm.Components.Insert(idx, action.Component);
                    action.Component.UpdateBaseHours();
                    ShowToast("Undo: component restored", false);
                    break;

                case UndoType.ComponentAdd when action.Component != null:
                    vm.Components.Remove(action.Component);
                    ShowToast("Undo: component add reverted", false);
                    break;

                case UndoType.ComponentPropertyChange when action.Component != null && action.PropertyName != null:
                    SetComponentProperty(action.Component, action.PropertyName, action.OldValue);
                    ShowToast("Undo: change reverted", false);
                    break;

                case UndoType.CollaborationDelete when action.CollaborationItem != null:
                    var cIdx = action.InsertIndex >= 0 && action.InsertIndex <= vm.CollaborationItems.Count
                        ? action.InsertIndex : vm.CollaborationItems.Count;
                    vm.CollaborationItems.Insert(cIdx, action.CollaborationItem);
                    ShowToast("Undo: item restored", false);
                    break;

                case UndoType.CollaborationAdd when action.CollaborationItem != null:
                    vm.CollaborationItems.Remove(action.CollaborationItem);
                    ShowToast("Undo: collaboration add reverted", false);
                    break;

                case UndoType.CollaborationPropertyChange when action.CollaborationItem != null && action.PropertyName != null:
                    SetCollaborationProperty(action.CollaborationItem, action.PropertyName, action.OldValue);
                    ShowToast("Undo: change reverted", false);
                    break;
            }
        }
        finally
        {
            _suppressUndo = false;
        }
    }

    private static void SetComponentProperty(ComponentRowViewModel row, string prop, object? value)
    {
        switch (prop)
        {
            case nameof(ComponentRowViewModel.RequirementId): row.RequirementId = (string)(value ?? string.Empty); break;
            case nameof(ComponentRowViewModel.ComponentType): row.ComponentType = (ComponentType)(value ?? ComponentType.None); break;
            case nameof(ComponentRowViewModel.Description): row.Description = (string)(value ?? string.Empty); break;
            case nameof(ComponentRowViewModel.ChangeType): row.ChangeType = (ChangeType)(value ?? ChangeType.None); break;
            case nameof(ComponentRowViewModel.Size): row.Size = (ComponentSize)(value ?? ComponentSize.None); break;
            case nameof(ComponentRowViewModel.Count): row.Count = (int)(value ?? 0); break;
            case nameof(ComponentRowViewModel.Notes): row.Notes = (string)(value ?? string.Empty); break;
        }
    }

    private static void SetCollaborationProperty(CollaborationRowViewModel row, string prop, object? value)
    {
        switch (prop)
        {
            case nameof(CollaborationRowViewModel.TaskName): row.TaskName = (string)(value ?? string.Empty); break;
            case nameof(CollaborationRowViewModel.CollabType): row.CollabType = (CollaborationType)(value ?? CollaborationType.WPRs); break;
            case nameof(CollaborationRowViewModel.NumberOfMeetings): row.NumberOfMeetings = (int)(value ?? 0); break;
            case nameof(CollaborationRowViewModel.MeetingDurationMinutes): row.MeetingDurationMinutes = (int)(value ?? 0); break;
            case nameof(CollaborationRowViewModel.NumberOfParticipants): row.NumberOfParticipants = (int)(value ?? 0); break;
            case nameof(CollaborationRowViewModel.ParticipantPrepTimeMinutes): row.ParticipantPrepTimeMinutes = (int)(value ?? 0); break;
            case nameof(CollaborationRowViewModel.Notes): row.Notes = (string)(value ?? string.Empty); break;
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

    private void CollaborationGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        var combo = FindVisualChild<ComboBox>(e.EditingElement);
        if (combo == null) return;

        // When Tab is pressed while the dropdown is open, close the dropdown and let Tab commit+move
        // When Enter is pressed, treat it like Tab — commit the cell and move to the next editable column
        combo.PreviewKeyDown += (s, args) =>
        {
            if (args.Key == Key.Tab && combo.IsDropDownOpen)
            {
                combo.IsDropDownOpen = false;
                // Do not mark Handled — let Tab propagate to DataGrid for normal navigation
            }
            else if (args.Key == Key.Enter)
            {
                if (combo.IsDropDownOpen)
                    combo.IsDropDownOpen = false;

                // Prevent the DataGrid's default Enter behavior (move to next row)
                args.Handled = true;

                // Capture current cell info before committing
                var currentCell = CollaborationGrid.CurrentCell;

                Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                {
                    CollaborationGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);

                    if (!currentCell.IsValid) return;

                    // Find the next editable column after the current one
                    int currentColIdx = CollaborationGrid.Columns.IndexOf(currentCell.Column);
                    for (int i = currentColIdx + 1; i < CollaborationGrid.Columns.Count; i++)
                    {
                        if (!CollaborationGrid.Columns[i].IsReadOnly)
                        {
                            CollaborationGrid.CurrentCell = new DataGridCellInfo(currentCell.Item, CollaborationGrid.Columns[i]);
                            CollaborationGrid.BeginEdit();
                            break;
                        }
                    }
                });
            }
        };
    }

    private void CollaborationGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SingleClickEdit(CollaborationGrid, e);
    }

    private void CollaborationGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (e.Row.Item is not InitialEstimatePOC.ViewModels.CollaborationRowViewModel row) return;

        // Find the editing ComboBox inside the cell
        var combo = FindVisualChild<ComboBox>(e.EditingElement);
        if (combo == null) return;

        var text = combo.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(text, out int entered)) { e.Cancel = true; return; }

        string colHeader = (e.Column.Header as string) ?? string.Empty;

        bool valid = colHeader switch
        {
            var h when h.StartsWith("Number of Meetings") => entered >= 0 && entered <= 20,
            var h when h.StartsWith("Meeting Duration")   => new[] { 0, 15, 30, 45, 60 }.Contains(entered),
            var h when h.StartsWith("Number of Participants") => entered >= 0 && entered <= 20,
            var h when h.StartsWith("Participant Prep")   => entered >= 0 && entered <= 180 && entered % 15 == 0,
            _ => true
        };

        if (!valid)
        {
            e.Cancel = true;
            var cell = e.Column.GetCellContent(e.Row)?.Parent as DataGridCell;
            MessageBox.Show(
                colHeader switch
                {
                    var h when h.StartsWith("Meeting Duration") =>
                        "Invalid value. Meeting Duration must be one of: 0, 15, 30, 45, or 60 minutes.",
                    var h when h.StartsWith("Participant Prep") =>
                        "Invalid value. Prep Time must be a multiple of 15 between 0 and 180 minutes.",
                    var h when h.StartsWith("Number of Meetings") =>
                        "Invalid value. Number of Meetings must be between 0 and 20.",
                    _ =>
                        "Invalid value. Number of Participants must be between 0 and 20."
                },
                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Delay focus so DataGrid's own post-handler navigation is overridden
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                CollaborationGrid.CurrentCell = new DataGridCellInfo(e.Row.Item, e.Column);
                CollaborationGrid.BeginEdit();
                var editingCombo = FindVisualChild<ComboBox>(e.Column.GetCellContent(e.Row)?.Parent as DependencyObject);
                editingCombo?.Focus();
            });
        }
    }

    // ─── PM Effort ComboBox validation ───────────────────────────────────────

    private void PmEffortComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ValidatePmEffortComboBox();
    }

    private void PmEffortComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Move focus away — this triggers LostFocus which runs the validation
            e.Handled = true;
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }

    private void ValidatePmEffortComboBox()
    {
        if (DataContext is not MainViewModel vm) return;

        var text = PmEffortComboBox.Text?.Trim() ?? string.Empty;
        bool valid = int.TryParse(text, out int entered) && vm.PmEffortOptions.Contains((decimal)entered);

        if (!valid)
        {
            MessageBox.Show(
                $"Invalid value '{text}'. PM Effort must be a whole number between {(int)vm.PmEffortOptions.First()} and {(int)vm.PmEffortOptions.Last()}%.",
                "Invalid PM Effort", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Revert to the current valid ViewModel value (keeps VM and UI in sync)
            PmEffortComboBox.Text = vm.PmEffortPercentage.ToString();
        }
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
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

public enum UndoType { ComponentDelete, ComponentAdd, ComponentPropertyChange, CollaborationDelete, CollaborationAdd, CollaborationPropertyChange }

public record UndoAction(
    UndoType Type,
    ComponentRowViewModel? Component,
    CollaborationRowViewModel? CollaborationItem,
    string? PropertyName = null,
    object? OldValue = null,
    int InsertIndex = -1);

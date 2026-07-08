using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC;

public partial class HistoryWindow : Window
{
    public ProjectEntity? SelectedProject { get; private set; }

    private List<ProjectEntity> _allProjects = new();

    public HistoryWindow()
    {
        InitializeComponent();
        LoadProjects();
    }

    private void LoadProjects()
    {
        _allProjects = MainViewModel.GetAllProjects();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var term = SearchBox?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(term))
        {
            ProjectsGrid.ItemsSource = _allProjects;
        }
        else
        {
            ProjectsGrid.ItemsSource = _allProjects
                .Where(p =>
                    (p.ProjectName ?? string.Empty).Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    (p.ChangeOrderId ?? string.Empty).Contains(term, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ClearSearchButton.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
        ApplyFilter();
    }

    private void OnClearSearchClick(object sender, RoutedEventArgs e)
    {
        SearchBox.Clear();
        SearchBox.Focus();
    }

    private void OnProjectDoubleClick(object sender, MouseButtonEventArgs e)
    {
        LoadSelectedAndClose();
    }

    private void OnLoadClick(object sender, RoutedEventArgs e)
    {
        LoadSelectedAndClose();
    }

    private void LoadSelectedAndClose()
    {
        if (ProjectsGrid.SelectedItem is ProjectEntity project)
        {
            SelectedProject = project;
            DialogResult = true;
            Close();
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (ProjectsGrid.SelectedItem is not ProjectEntity project) return;

        var result = MessageBox.Show(
            $"Delete project \"{project.ProjectName}\"?\nThis cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        using var db = new EstimateDbContext();
        var toDelete = db.Projects.Include(p => p.Components).FirstOrDefault(p => p.ProjectId == project.ProjectId);
        if (toDelete != null)
        {
            db.Projects.Remove(toDelete);
            db.SaveChanges();
        }

        LoadProjects();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

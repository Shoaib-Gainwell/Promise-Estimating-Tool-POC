using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC;

public partial class HistoryWindow : Window
{
    public ProjectEntity? SelectedProject { get; private set; }

    public HistoryWindow()
    {
        InitializeComponent();
        LoadProjects();
    }

    private void LoadProjects()
    {
        var projects = MainViewModel.GetAllProjects();
        ProjectsGrid.ItemsSource = projects;
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

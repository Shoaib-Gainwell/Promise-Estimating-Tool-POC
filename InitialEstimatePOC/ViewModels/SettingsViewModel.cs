using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using Microsoft.EntityFrameworkCore;

namespace InitialEstimatePOC.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasChanges;

    public ObservableCollection<WeightedValueRow> WeightedValueRows { get; } = new();

    public SettingsViewModel()
    {
        LoadValues();
    }

    private void LoadValues()
    {
        WeightedValueRows.Clear();
        using var db = new EstimateDbContext();
        var entities = db.WeightedValues.OrderBy(v => v.ComponentType).ThenBy(v => v.Size).ThenBy(v => v.ChangeType).ToList();

        foreach (var e in entities)
        {
            var row = new WeightedValueRow
            {
                Id = e.Id,
                ComponentType = e.ComponentType,
                Size = e.Size,
                ChangeType = e.ChangeType,
                BaseHours = e.BaseHours,
                OriginalBaseHours = e.BaseHours,
                DisplayName = WeightedValues.GetDisplayName(e.ComponentType),
                LastModified = e.LastModified,
                ModifiedBy = e.ModifiedBy
            };
            row.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(WeightedValueRow.BaseHours))
                    HasChanges = true;
            };
            WeightedValueRows.Add(row);
        }
    }

    [RelayCommand]
    private void SaveAll()
    {
        using var db = new EstimateDbContext();
        int changed = 0;

        foreach (var row in WeightedValueRows)
        {
            if (row.BaseHours != row.OriginalBaseHours)
            {
                WeightedValues.UpdateValue(db, row.ComponentType, row.Size, row.ChangeType, row.BaseHours, "Manager");
                row.OriginalBaseHours = row.BaseHours;
                row.LastModified = DateTime.UtcNow;
                row.ModifiedBy = "Manager";
                changed++;
            }
        }

        HasChanges = false;
        StatusMessage = changed > 0
            ? $"✓ Saved {changed} value(s) successfully"
            : "No changes to save";

        if (changed > 0)
            WeightedValues.NotifyValuesChanged();
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        using var db = new EstimateDbContext();
        db.WeightedValues.RemoveRange(db.WeightedValues);
        db.SaveChanges();

        InitialEstimatePOC.Data.DatabaseSeeder.Initialize(db);
        WeightedValues.LoadFromDatabase(db);
        LoadValues();
        HasChanges = false;
        StatusMessage = "✓ Reset all values to defaults";
        WeightedValues.NotifyValuesChanged();
    }
}

public partial class WeightedValueRow : ObservableObject
{
    public int Id { get; set; }
    public ComponentType ComponentType { get; set; }
    public ComponentSize Size { get; set; }
    public ChangeType ChangeType { get; set; }

    [ObservableProperty]
    private decimal _baseHours;

    public decimal OriginalBaseHours { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    private DateTime _lastModified;

    [ObservableProperty]
    private string _modifiedBy = string.Empty;

    public bool IsModified => BaseHours != OriginalBaseHours;
}

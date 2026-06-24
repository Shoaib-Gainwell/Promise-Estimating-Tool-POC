using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for SettingsViewModel — covers LoadValues, SaveAll, ResetToDefaults,
/// change tracking, and status messages.
/// Uses the real app DB (seeded on app startup).
/// </summary>
public class SettingsViewModelTests
{
    private SettingsViewModel CreateVm()
    {
        // Ensure DB is seeded so SettingsViewModel can load values
        using var db = new EstimateDbContext();
        db.Database.EnsureCreated();
        DatabaseSeeder.Initialize(db);
        WeightedValues.LoadFromDatabase(db);
        return new SettingsViewModel();
    }

    [Fact]
    public void Constructor_LoadsAllValues()
    {
        var vm = CreateVm();

        Assert.Equal(66, vm.WeightedValueRows.Count);
    }

    [Fact]
    public void Constructor_DefaultStatusMessage_Empty()
    {
        var vm = CreateVm();

        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    [Fact]
    public void Constructor_HasChanges_InitiallyFalse()
    {
        var vm = CreateVm();

        Assert.False(vm.HasChanges);
    }

    [Fact]
    public void LoadValues_RowsHaveCorrectDisplayName()
    {
        var vm = CreateVm();

        var pbRow = vm.WeightedValueRows.First(r => r.ComponentType == ComponentType.PowerBuilderWindows);
        Assert.Equal("PowerBuilder Windows", pbRow.DisplayName);
    }

    [Fact]
    public void LoadValues_RowsHaveCorrectBaseHours()
    {
        var vm = CreateVm();

        var row = vm.WeightedValueRows.First(r =>
            r.ComponentType == ComponentType.MISC && r.Size == ComponentSize.Large && r.ChangeType == ChangeType.New);
        Assert.Equal(100.00m, row.BaseHours);
    }

    [Fact]
    public void LoadValues_OriginalBaseHoursMatchBaseHours()
    {
        var vm = CreateVm();

        Assert.All(vm.WeightedValueRows, r => Assert.Equal(r.OriginalBaseHours, r.BaseHours));
    }

    [Fact]
    public void ModifyRow_SetsHasChangesToTrue()
    {
        var vm = CreateVm();

        vm.WeightedValueRows[0].BaseHours = 999m;

        Assert.True(vm.HasChanges);
    }

    [Fact]
    public void SaveAllCommand_NoChanges_SetsNoChangesMessage()
    {
        var vm = CreateVm();

        vm.SaveAllCommand.Execute(null);

        Assert.Equal("No changes to save", vm.StatusMessage);
    }

    [Fact]
    public void SaveAllCommand_WithChanges_SavesAndSetsMessage()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 999m;

        vm.SaveAllCommand.Execute(null);

        Assert.Contains("Saved 1 value(s) successfully", vm.StatusMessage);

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }

    [Fact]
    public void SaveAllCommand_ResetsHasChanges()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 999m;
        Assert.True(vm.HasChanges);

        vm.SaveAllCommand.Execute(null);

        Assert.False(vm.HasChanges);

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }

    [Fact]
    public void SaveAllCommand_UpdatesOriginalBaseHours()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 777m;

        vm.SaveAllCommand.Execute(null);

        Assert.Equal(777m, vm.WeightedValueRows[0].OriginalBaseHours);

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }

    [Fact]
    public void SaveAllCommand_MultipleChanges_CountsCorrectly()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 111m;
        vm.WeightedValueRows[1].BaseHours = 222m;
        vm.WeightedValueRows[2].BaseHours = 333m;

        vm.SaveAllCommand.Execute(null);

        Assert.Contains("Saved 3 value(s) successfully", vm.StatusMessage);

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }

    [Fact]
    public void SaveAllCommand_FiresValuesChangedEvent()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 999m;
        bool eventFired = false;
        void handler() => eventFired = true;
        WeightedValues.ValuesChanged += handler;

        vm.SaveAllCommand.Execute(null);

        Assert.True(eventFired);
        WeightedValues.ValuesChanged -= handler;

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }

    [Fact]
    public void ResetToDefaultsCommand_ReloadsValues()
    {
        var vm = CreateVm();
        var original = vm.WeightedValueRows[0].BaseHours;
        vm.WeightedValueRows[0].BaseHours = 999m;
        vm.SaveAllCommand.Execute(null);

        vm.ResetToDefaultsCommand.Execute(null);

        Assert.Equal(original, vm.WeightedValueRows[0].BaseHours);
    }

    [Fact]
    public void ResetToDefaultsCommand_SetsStatusMessage()
    {
        var vm = CreateVm();

        vm.ResetToDefaultsCommand.Execute(null);

        Assert.Contains("Reset all values to defaults", vm.StatusMessage);
    }

    [Fact]
    public void ResetToDefaultsCommand_ClearsHasChanges()
    {
        var vm = CreateVm();
        vm.WeightedValueRows[0].BaseHours = 999m;
        Assert.True(vm.HasChanges);

        vm.ResetToDefaultsCommand.Execute(null);

        Assert.False(vm.HasChanges);
    }

    [Fact]
    public void WeightedValueRow_IsModified_WhenChanged()
    {
        var vm = CreateVm();
        var row = vm.WeightedValueRows[0];
        Assert.False(row.IsModified);

        row.BaseHours = row.BaseHours + 1m;

        Assert.True(row.IsModified);
    }

    [Fact]
    public void WeightedValueRow_NotModified_AfterSave()
    {
        var vm = CreateVm();
        var row = vm.WeightedValueRows[0];
        row.BaseHours = 500m;
        Assert.True(row.IsModified);

        vm.SaveAllCommand.Execute(null);

        Assert.False(row.IsModified);

        // Restore
        vm.ResetToDefaultsCommand.Execute(null);
    }
}

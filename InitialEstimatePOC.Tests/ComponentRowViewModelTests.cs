using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for the ComponentRowViewModel — base hours lookup, 
/// TotalHours calculation, property change behavior.
/// </summary>
public class ComponentRowViewModelTests
{
    #region BaseHours Auto-Lookup

    [Fact]
    public void SetComponentType_UpdatesBaseHours()
    {
        var row = new ComponentRowViewModel();
        // Default is PowerBuilderWindows, change to Reports to trigger update
        row.ComponentType = ComponentType.Reports;
        // Reports Small New = 17.00
        Assert.Equal(17.00m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void SetSize_UpdatesBaseHours()
    {
        var row = new ComponentRowViewModel();
        row.ComponentType = ComponentType.Reports;
        row.Size = ComponentSize.Medium;
        // Reports Medium New = 51.00
        Assert.Equal(51.00m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void SetChangeType_UpdatesBaseHours()
    {
        var row = new ComponentRowViewModel();
        row.ComponentType = ComponentType.Webpage;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.Change;
        // Webpage Large Change = 75.00
        Assert.Equal(75.00m, row.BaseHoursPerUnit);
    }

    #endregion

    #region TotalHours Calculation

    [Fact]
    public void TotalHours_BaseTimesCount()
    {
        var row = new ComponentRowViewModel();
        row.ComponentType = ComponentType.K2Workflow;
        row.Size = ComponentSize.Medium;
        row.ChangeType = ChangeType.New;
        row.Count = 3;
        // 100.00 * 3 = 300.00
        Assert.Equal(300.00m, row.TotalHours);
    }

    [Fact]
    public void TotalHours_CountZero_ReturnsZero()
    {
        var row = new ComponentRowViewModel();
        row.ComponentType = ComponentType.K2Workflow;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 0;
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void TotalHours_CountOne_EqualsBaseHours()
    {
        var row = new ComponentRowViewModel();
        row.ComponentType = ComponentType.ProgramsDBStoredProcs;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;
        Assert.Equal(294.40m, row.TotalHours);
    }

    #endregion

    #region Property Defaults

    [Fact]
    public void NewRow_DefaultValues()
    {
        var row = new ComponentRowViewModel();
        Assert.Equal(0, row.LineNumber);
        Assert.Equal(string.Empty, row.RequirementId);
        Assert.Equal(ComponentType.PowerBuilderWindows, row.ComponentType); // First enum value
        Assert.Equal(string.Empty, row.Description);
        Assert.Equal(ChangeType.New, row.ChangeType); // First enum value
        Assert.Equal(ComponentSize.Small, row.Size); // First enum value
        Assert.Equal(1, row.Count);
    }

    [Fact]
    public void RequirementId_CanBeSet()
    {
        var row = new ComponentRowViewModel();
        row.RequirementId = "REQ-001";
        Assert.Equal("REQ-001", row.RequirementId);
    }

    [Fact]
    public void Description_CanBeSet()
    {
        var row = new ComponentRowViewModel();
        row.Description = "Main window UI";
        Assert.Equal("Main window UI", row.Description);
    }

    #endregion

    #region PropertyChanged Events

    [Fact]
    public void ChangeComponentType_RaisesPropertyChanged()
    {
        var row = new ComponentRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.ComponentType = ComponentType.Reports;

        Assert.Contains(nameof(ComponentRowViewModel.ComponentType), raised);
        Assert.Contains(nameof(ComponentRowViewModel.BaseHoursPerUnit), raised);
        Assert.Contains(nameof(ComponentRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void ChangeSize_RaisesPropertyChanged()
    {
        var row = new ComponentRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.Size = ComponentSize.Large;

        Assert.Contains(nameof(ComponentRowViewModel.Size), raised);
        Assert.Contains(nameof(ComponentRowViewModel.BaseHoursPerUnit), raised);
        Assert.Contains(nameof(ComponentRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void ChangeCount_RaisesTotalHoursChanged()
    {
        var row = new ComponentRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.Count = 5;

        Assert.Contains(nameof(ComponentRowViewModel.Count), raised);
        Assert.Contains(nameof(ComponentRowViewModel.TotalHours), raised);
    }

    #endregion

    #region All Component Types with Count Multiplier

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 3, 225.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 5, 104.70)]
    [InlineData(ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2, 34.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1, 294.40)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 4, 60.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1, 100.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 2, 70.00)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change, 3, 64.6875)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8, 65.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large, ChangeType.New, 2, 180.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New, 4, 32.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Medium, ChangeType.Change, 2, 50.00)]
    public void TotalHours_AllComponentTypes_CorrectCalculation(
        ComponentType type, ComponentSize size, ChangeType change, int count, decimal expected)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = type,
            Size = size,
            ChangeType = change,
            Count = count
        };

        Assert.Equal(expected, row.TotalHours);
    }

    #endregion
}

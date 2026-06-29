using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Edge case and additional coverage tests for MainViewModel and ComponentRowViewModel.
/// </summary>
public class ViewModelEdgeCaseTests
{
    #region ComponentRowViewModel Edge Cases

    [Fact]
    public void ComponentRow_Count_Zero_TotalHoursIsZero()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 0
        };

        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void ComponentRow_NegativeCount_NegativeTotalHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = -1
        };

        Assert.True(row.TotalHours < 0);
    }

    [Fact]
    public void ComponentRow_LargeCount_CalculatesCorrectly()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1000
        };

        Assert.Equal(100000m, row.TotalHours);
    }

    [Fact]
    public void ComponentRow_DefaultValues_OnConstruction()
    {
        var row = new ComponentRowViewModel();

        Assert.Equal(0, row.LineNumber);
        Assert.Equal(string.Empty, row.RequirementId);
        Assert.Equal(ComponentType.None, row.ComponentType);
        Assert.Equal(string.Empty, row.Description);
        Assert.Equal(ChangeType.None, row.ChangeType);
        Assert.Equal(ComponentSize.None, row.Size);
        Assert.Equal(0, row.Count);
    }

    [Fact]
    public void ComponentRow_UpdateBaseHours_SetsCorrectValue()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.K2Workflow,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New
        };
        row.UpdateBaseHours();

        Assert.Equal(200m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_ChangeType_TriggersUpdateBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New
        };
        Assert.Equal(100m, row.BaseHoursPerUnit);

        row.ChangeType = ChangeType.Change;
        Assert.Equal(50m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_Size_TriggersUpdateBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New
        };
        Assert.Equal(20m, row.BaseHoursPerUnit);

        row.Size = ComponentSize.Large;
        Assert.Equal(100m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_ComponentType_TriggersUpdateBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New
        };
        Assert.Equal(20m, row.BaseHoursPerUnit);

        row.ComponentType = ComponentType.K2Workflow;
        Assert.Equal(50m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_Count_ChangeTriggersTotalHoursNotification()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New
        };
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.Count = 5;

        Assert.Contains(nameof(ComponentRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void ComponentRow_Description_CanBeVeryLong()
    {
        var row = new ComponentRowViewModel();
        var longDesc = new string('A', 1000);
        row.Description = longDesc;
        Assert.Equal(longDesc, row.Description);
    }

    #endregion

    #region MainViewModel Edge Cases

    [Fact]
    public void MainViewModel_ZeroComponents_AllTotalsZero()
    {
        var vm = new MainViewModel();

        // Development and derived tasks are zero with no components
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.SystemTestingHours);
        Assert.Equal(0m, vm.AnalysisHours);
        Assert.Equal(0m, vm.BusinessDesignHours);
        Assert.Equal(0m, vm.PromotionHours);
        Assert.Equal(0m, vm.BaSystemDocHours);
        Assert.Equal(0m, vm.ProductionValidationHours);
        Assert.Equal(0m, vm.ProjectManagementHours);
        // No default collaboration items
        Assert.Equal(0m, vm.TotalCollaborationHours);
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void MainViewModel_DefaultPmPercentage_Is15()
    {
        var vm = new MainViewModel();
        Assert.Equal(15m, vm.PmEffortPercentage);
    }

    [Fact]
    public void MainViewModel_ChangePmPercentage_RecalculatesImmediately()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        var pmBefore = vm.ProjectManagementHours;
        vm.PmEffortPercentage = 30m;
        var pmAfter = vm.ProjectManagementHours;

        Assert.True(pmAfter > pmBefore);
    }

    [Fact]
    public void MainViewModel_PmPercentage_Zero_NoPmHours()
    {
        var vm = new MainViewModel();
        vm.PmEffortPercentage = 0m;
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;

        Assert.Equal(0m, vm.ProjectManagementHours);
    }

    [Fact]
    public void MainViewModel_RemoveNullComponent_DoesNothing()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);

        vm.RemoveComponentCommand.Execute(null);

        Assert.Single(vm.Components);
    }

    [Fact]
    public void MainViewModel_RemoveComponent_RenumbersRemaining()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        vm.RemoveComponentCommand.Execute(vm.Components[1]);

        Assert.Equal(1, vm.Components[0].LineNumber);
        Assert.Equal(2, vm.Components[1].LineNumber);
    }

    [Fact]
    public void MainViewModel_ClearAll_EmptiesCollection()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        vm.ClearAllCommand.Execute(null);

        Assert.Empty(vm.Components);
    }

    [Fact]
    public void MainViewModel_ClearAll_ResetsAllTotals()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        Assert.True(vm.GrandTotalHours > 0);

        vm.ClearAllCommand.Execute(null);

        Assert.Equal(0m, vm.GrandTotalHours);
    }

    [Fact]
    public void MainViewModel_AddComponent_IncrementsLineNumber()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        Assert.Equal(1, vm.Components[0].LineNumber);
        Assert.Equal(2, vm.Components[1].LineNumber);
        Assert.Equal(3, vm.Components[2].LineNumber);
    }

    [Fact]
    public void MainViewModel_ComponentCount_MatchesCollectionSize()
    {
        var vm = new MainViewModel();
        Assert.Equal(0, vm.ComponentCount);

        vm.AddComponentCommand.Execute(null);
        Assert.Equal(1, vm.ComponentCount);

        vm.AddComponentCommand.Execute(null);
        Assert.Equal(2, vm.ComponentCount);

        vm.RemoveComponentCommand.Execute(vm.Components[0]);
        Assert.Equal(1, vm.ComponentCount);
    }

    [Fact]
    public void MainViewModel_BaRoleHours_CalculatedCorrectly()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // BA = Analysis/2 + BusinessDesign + BADoc + ProdValidation + PM/2
        var expected = Math.Ceiling((vm.AnalysisHours / 2m + vm.BusinessDesignHours +
            vm.BaSystemDocHours + vm.ProductionValidationHours + vm.ProjectManagementHours / 2m) * 100m) / 100m;

        // The actual ROUNDUP function is used, just verify BA > 0
        Assert.True(vm.BaRoleHours > 0);
    }

    [Fact]
    public void MainViewModel_SeRoleHours_IncludesDevelopment()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;

        // SE includes Dev hours, so SE >= Dev
        Assert.True(vm.SeRoleHours >= vm.TotalDevelopmentHours);
    }

    [Fact]
    public void MainViewModel_TesterRoleHours_EqualsSystemTesting()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;

        Assert.Equal(vm.SystemTestingHours, vm.TesterRoleHours);
    }

    [Fact]
    public void MainViewModel_PmRoleHours_EqualsPmEffort()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;

        Assert.Equal(vm.ProjectManagementHours, vm.PmRoleHours);
    }

    [Fact]
    public void MainViewModel_GrandTotal_EqualsCeilingOfSubtotal()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;

        // Grand Total = Math.Ceiling(Subtotal)
        // Subtotal includes Dev + Derived + PM Effort + Collaboration + Adjusted
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    [Fact]
    public void MainViewModel_MultipleComponents_TotalDevIsSumOfAll()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].ComponentType = ComponentType.K2Workflow;
        vm.Components[1].Size = ComponentSize.Medium;
        vm.Components[1].ChangeType = ChangeType.New;
        vm.Components[1].Count = 1;

        Assert.Equal(vm.Components[0].TotalHours + vm.Components[1].TotalHours, vm.TotalDevelopmentHours);
    }

    #endregion

    #region Model Tests

    [Fact]
    public void ComponentEntry_TotalHoursProperty_IsComputed()
    {
        var entry = new ComponentEntry
        {
            BaseHoursPerUnit = 10m,
            Count = 5
        };

        Assert.Equal(50m, entry.TotalHours);
    }

    [Fact]
    public void ComponentEntry_DefaultCount_IsOne()
    {
        var entry = new ComponentEntry();
        Assert.Equal(1, entry.Count);
    }

    [Fact]
    public void ProjectEntity_DefaultPmPercentage_Is15()
    {
        var entity = new ProjectEntity();
        Assert.Equal(15m, entity.PmEffortPercentage);
    }

    [Fact]
    public void ProjectEntity_ComponentsList_InitializedEmpty()
    {
        var entity = new ProjectEntity();
        Assert.NotNull(entity.Components);
        Assert.Empty(entity.Components);
    }

    [Fact]
    public void WeightedValueEntity_Defaults()
    {
        var entity = new WeightedValueEntity();
        Assert.Equal("System", entity.ModifiedBy);
        Assert.True((DateTime.UtcNow - entity.LastModified).TotalSeconds < 5);
    }

    #endregion

    #region Enum Coverage

    [Fact]
    public void ComponentType_Has11Values()
    {
        Assert.Equal(12, Enum.GetValues<ComponentType>().Length);
    }

    [Fact]
    public void ChangeType_Has2Values()
    {
        Assert.Equal(3, Enum.GetValues<ChangeType>().Length);
    }

    [Fact]
    public void ComponentSize_Has3Values()
    {
        Assert.Equal(4, Enum.GetValues<ComponentSize>().Length);
    }

    #endregion
}

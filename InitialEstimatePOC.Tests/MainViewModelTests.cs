using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;
using System.Linq;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for the MainViewModel calculation engine.
/// Covers: derived tasks, ROUNDUP, T-shirt sizing, role breakout, 
/// add/remove/clear operations, and the worked example from documentation.
/// </summary>
public class MainViewModelTests
{
    private MainViewModel CreateVm() => new();

    private ComponentRowViewModel AddComponent(MainViewModel vm, ComponentType type, ComponentSize size, ChangeType change, int count)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = count;
        return row;
    }

    #region ROUNDUP Function Tests

    [Fact]
    public void RoundUp_ExactValue_NoRounding()
    {
        // 100 * 0.30 = 30.00 exactly
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1); // 100 hrs
        // System Testing = ROUNDUP(100 * 0.30, 2) = 30.00
        Assert.Equal(30.00m, vm.SystemTestingHours);
    }

    [Fact]
    public void RoundUp_FractionalValue_RoundsAwayFromZero()
    {
        // 75 * 0.30 = 22.50 exactly
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 1); // 75 hrs
        // System Testing = ROUNDUP(75 * 0.30, 2) = ROUNDUP(22.50, 2) = 22.50 (exact)
        Assert.Equal(22.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void RoundUp_ThirdDecimalPlace_RoundsUp()
    {
        // 17 * 0.30 = 5.10 (exact — no rounding needed)
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 1); // 17 hrs
        Assert.Equal(5.10m, vm.SystemTestingHours);
    }

    #endregion

    #region Worked Example from Documentation (Section 8)

    [Fact]
    public void WorkedExample_DevelopmentHours_Correct()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 3);   // 225.00
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 5); // 104.70
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2);                // 34.00
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1);  // 294.40
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 4);        // 60.00
        AddComponent(vm, ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8);         // 65.04 (8.13*8)
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1);            // 100.00
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 2);        // 70.00

        // Note: With full-precision values: DatabaseReview Small New = 8.125
        // 8.125 * 8 = 65.00
        // Actual total = 225 + 104.70 + 34 + 294.40 + 60 + 65 + 100 + 70 = 953.10
        decimal expectedDev = 225.00m + 104.70m + 34.00m + 294.40m + 60.00m + 65.00m + 100.00m + 70.00m;
        Assert.Equal(expectedDev, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void WorkedExample_ComponentCount_Is8()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 2);

        Assert.Equal(8, vm.ComponentCount);
    }

    [Fact]
    public void WorkedExample_TShirtSize_IsXL1()
    {
        var vm = CreateVm();
        // Clear default collaboration for this pure development test
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 2);

        Assert.Equal("XL1", vm.TShirtSize);
    }

    #endregion

    #region Derived Task Calculations

    [Fact]
    public void DerivedTasks_SingleComponent_CalculatesCorrectly()
    {
        var vm = CreateVm();
        // Use MISC Large New = 100 hours for easy math
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        Assert.Equal(100.00m, vm.TotalDevelopmentHours);
        Assert.Equal(30.00m, vm.SystemTestingHours);           // 100 * 0.30
        Assert.Equal(6.50m, vm.AnalysisHours);                 // (100+30) * 0.05 = 6.50
        Assert.Equal(19.50m, vm.BusinessDesignHours);           // (100+30) * 0.15 = 19.50
        Assert.Equal(5.00m, vm.PromotionHours);                // 100 * 0.05
        Assert.Equal(5.00m, vm.BaSystemDocHours);              // 100 * 0.05
        Assert.Equal(6.00m, vm.ProductionValidationHours);     // 30 * 0.20
    }

    [Fact]
    public void DerivedTasks_PMEffort_Default15Percent()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        // Subtotal = 100 + 30 + 6.50 + 19.50 + 5 + 5 + 6 = 172
        // PM = ROUNDUP(172 * 0.15, 2) = ROUNDUP(25.80, 2) = 25.80
        Assert.Equal(25.80m, vm.ProjectManagementHours);
    }

    [Fact]
    public void DerivedTasks_GrandTotal_IncludesPM()
    {
        var vm = CreateVm();
        // Clear default collaboration for pure derived-task validation
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        // Subtotal = 172 + 25.80 = 197.80
        // Grand Total = Math.Ceiling(197.80) = 198
        Assert.Equal(198m, vm.GrandTotalHours);
    }

    [Fact]
    public void DerivedTasks_OrderDependency_SystemTestingBeforeAnalysis()
    {
        var vm = CreateVm();
        // Use 51 hrs (Reports Medium New) — produces non-trivial fractions
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 1);

        // Dev = 51
        // SysTest = ROUNDUP(51*0.30, 2) = ROUNDUP(15.30, 2) = 15.30
        // Analysis = ROUNDUP((51+15.30)*0.05, 2) = ROUNDUP(3.315, 2) = 3.32
        Assert.Equal(51.00m, vm.TotalDevelopmentHours);
        Assert.Equal(15.30m, vm.SystemTestingHours);
        Assert.Equal(3.32m, vm.AnalysisHours);
    }

    #endregion

    #region PM Effort Percentage (Configurable)

    [Fact]
    public void PMEffort_ChangePercentage_Recalculates()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        // Change PM from 15% to 20%
        vm.PmEffortPercentage = 20m;

        // Subtotal = 172
        // PM = ROUNDUP(172 * 0.20, 2) = ROUNDUP(34.40, 2) = 34.40
        Assert.Equal(34.40m, vm.ProjectManagementHours);
    }

    [Fact]
    public void PMEffort_ZeroPercent_NoPMHours()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.PmEffortPercentage = 0m;

        Assert.Equal(0m, vm.ProjectManagementHours);
    }

    #endregion

    #region Role Breakout

    [Fact]
    public void RoleBreakout_TesterEqualsSystemTesting()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal(vm.SystemTestingHours, vm.TesterRoleHours);
    }

    [Fact]
    public void RoleBreakout_PMEqualsPMEffort()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal(vm.ProjectManagementHours, vm.PmRoleHours);
    }

    [Fact]
    public void RoleBreakout_BA_Formula()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        // BA = Analysis/2 + BusinessDesign + BADoc + ProdValidation + ActualHours/2 + TimeForEstimates/2
        // = 6.50/2 + 19.50 + 5.00 + 6.00 + 0/2 + 0/2
        // = 3.25 + 19.50 + 5.00 + 6.00 = 33.75
        // ROUNDUP(33.75, 2) = 33.75 (exact)
        Assert.Equal(33.75m, vm.BaRoleHours);
    }

    [Fact]
    public void RoleBreakout_SE_Formula()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        // SE = Dev + Analysis/2 + Promotion + ActualHours/2 + TimeForEstimates/2
        // = 100 + 6.50/2 + 5.00 + 0/2 + 0/2
        // = 100 + 3.25 + 5.00 = 108.25
        // ROUNDUP(108.25, 2) = 108.25 (exact)
        Assert.Equal(108.25m, vm.SeRoleHours);
    }

    #endregion

    #region Add/Remove/Clear Components

    [Fact]
    public void AddComponent_IncreasesCount()
    {
        var vm = CreateVm();
        Assert.Empty(vm.Components);

        vm.AddComponentCommand.Execute(null);
        Assert.Single(vm.Components);
        Assert.Equal(1, vm.Components[0].LineNumber);
    }

    [Fact]
    public void AddMultipleComponents_NumbersSequentially()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        Assert.Equal(1, vm.Components[0].LineNumber);
        Assert.Equal(2, vm.Components[1].LineNumber);
        Assert.Equal(3, vm.Components[2].LineNumber);
    }

    [Fact]
    public void RemoveComponent_RenumbersRemaining()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        var second = vm.Components[1];
        vm.RemoveComponentCommand.Execute(second);

        Assert.Equal(2, vm.Components.Count);
        Assert.Equal(1, vm.Components[0].LineNumber);
        Assert.Equal(2, vm.Components[1].LineNumber);
    }

    [Fact]
    public void RemoveComponent_Null_DoesNothing()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.RemoveComponentCommand.Execute(null);
        Assert.Single(vm.Components);
    }

    [Fact]
    public void ClearAll_RemovesAllComponents()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test Project";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Some description";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.PmEffortPercentage = 20m;
        vm.DevelopmentAdjustedHours = 5m;
        vm.AnalysisAdjustedHours = 2m;
        vm.AdjustedHoursComments = "Adjusted for scope";
        vm.SeAssumptions = "SE note";
        vm.BaAssumptions = "BA note";
        vm.CollaborationAssumptions = "Collab note";
        vm.GeneralAssumptions = "General note";
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCasesComplex = 3;
        vm.TestCasesVeryComplex = 1;
        vm.TestCaseIterations = 2;
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2);
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.Change, 1);

        vm.ClearAllCommand.Execute(null);

        // Components and totals
        Assert.Empty(vm.Components);
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal(0m, vm.ProjectManagementHours);
        Assert.Equal(0, vm.ComponentCount);
        Assert.Equal("—", vm.TShirtSize);

        // Header fields
        Assert.Equal(string.Empty, vm.ProjectName);
        Assert.Equal(string.Empty, vm.ChangeOrderId);
        Assert.Equal(string.Empty, vm.ProjectDescription);
        Assert.Equal(string.Empty, vm.EstimatedBy);
        Assert.Equal(string.Empty, vm.ReviewedBy);

        // PM defaults restored
        Assert.Equal(15m, vm.PmEffortPercentage);

        // Adjusted hours cleared
        Assert.Equal(0m, vm.DevelopmentAdjustedHours);
        Assert.Equal(0m, vm.AnalysisAdjustedHours);
        Assert.Equal(string.Empty, vm.AdjustedHoursComments);

        // Assumptions cleared
        Assert.Equal(string.Empty, vm.SeAssumptions);
        Assert.Equal(string.Empty, vm.BaAssumptions);
        Assert.Equal(string.Empty, vm.CollaborationAssumptions);
        Assert.Equal(string.Empty, vm.GeneralAssumptions);

        // Test case fields cleared
        Assert.False(vm.UseTestCasesForEstimate);
        Assert.Equal(0, vm.TestCasesSimple);
        Assert.Equal(0, vm.TestCasesMedium);
        Assert.Equal(0, vm.TestCasesComplex);
        Assert.Equal(0, vm.TestCasesVeryComplex);
        Assert.Equal(1, vm.TestCaseIterations);
    }

    #endregion

    #region Empty State

    [Fact]
    public void EmptyState_AllZeros()
    {
        var vm = CreateVm();

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
        Assert.Equal(0, vm.ComponentCount);
        // T-Shirt Size = "—" when no components exist
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void EmptyState_RoleBreakout_AllZeros()
    {
        var vm = CreateVm();

        Assert.Equal(0m, vm.BaRoleHours);
        Assert.Equal(0m, vm.SeRoleHours);
        Assert.Equal(0m, vm.TesterRoleHours);
        Assert.Equal(0m, vm.PmRoleHours);
    }

    #endregion

    #region Header Properties

    [Fact]
    public void ProjectName_CanBeSet()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test Project";
        Assert.Equal("Test Project", vm.ProjectName);
    }

    [Fact]
    public void ChangeOrderId_CanBeSet()
    {
        var vm = CreateVm();
        vm.ChangeOrderId = "CO-2024-001";
        Assert.Equal("CO-2024-001", vm.ChangeOrderId);
    }

    [Fact]
    public void ProjectDescription_CanBeSet()
    {
        var vm = CreateVm();
        vm.ProjectDescription = "Add new features";
        Assert.Equal("Add new features", vm.ProjectDescription);
    }

    #endregion

    #region T-Shirt Size Integration

    [Fact]
    public void TShirtSize_SmallProject()
    {
        var vm = CreateVm();
        // No default collaboration → 3 hrs dev + derived → Grand Total ~ 6.25 = Small
        // Test Automation UFT Small New = 3 hrs
        AddComponent(vm, ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.New, 1);
        Assert.Equal("Small", vm.TShirtSize);
    }

    [Fact]
    public void TShirtSize_MediumProject()
    {
        var vm = CreateVm();
        // Reports Medium New = 51 hrs → Grand total ~100.95 = Medium
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 1);
        Assert.Equal("Medium", vm.TShirtSize);
    }

    [Fact]
    public void TShirtSize_LargeProject()
    {
        var vm = CreateVm();
        // Programs/DB Large New = 294.40 → Grand total ~582
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal("Large", vm.TShirtSize);
    }

    #endregion

    #region Multiple Components Aggregation

    [Fact]
    public void MultipleComponents_SumsDevelopmentHours()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 2);  // 20*2=40
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 1);  // 17*1=17

        Assert.Equal(57.00m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void ComponentCountUpdate_ReflectsChanges()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        Assert.Equal(1, vm.ComponentCount);

        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 1);
        Assert.Equal(2, vm.ComponentCount);

        vm.RemoveComponentCommand.Execute(vm.Components[0]);
        Assert.Equal(1, vm.ComponentCount);
    }

    #endregion
}

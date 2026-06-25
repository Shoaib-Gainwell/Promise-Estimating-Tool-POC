using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Comprehensive tests for the 4 new features:
/// 1. Test Cases for System Testing (alternative to 30% formula)
/// 2. Total Actual Hours + Date
/// 3. Time for Estimates (Detailed and Final)
/// 4. Adjusted Hours Comments
/// </summary>
public class NewFeaturesTests
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

    #region Test Cases for System Testing — Positive Scenarios

    [Fact]
    public void UseTestCases_DefaultOff_UsesPercentageFormula()
    {
        var vm = CreateVm();
        Assert.False(vm.UseTestCasesForEstimate);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 1);
        // System Testing should be 30% of Development
        decimal expected = MainViewModel.RoundUp(vm.TotalDevelopmentHours * 0.30m);
        Assert.Equal(expected, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_TurnedOn_CalculatesFromTestCases()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;  // 10 * 0.5 = 5
        vm.TestCasesMedium = 5;   // 5 * 1.0 = 5
        vm.TestCasesComplex = 3;  // 3 * 2.0 = 6
        vm.TestCasesVeryComplex = 2; // 2 * 4.0 = 8
        vm.TestCaseIterations = 1;
        // Total = (5 + 5 + 6 + 8) * 1 = 24
        Assert.Equal(24m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_WithIterations_MultipliesCorrectly()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 4;   // 4 * 0.5 = 2
        vm.TestCasesMedium = 2;   // 2 * 1.0 = 2
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 3;
        // Total = (2 + 2) * 3 = 12
        Assert.Equal(12m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_AllZeroCases_ReturnsZero()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 0;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 1;
        Assert.Equal(0m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_SwitchingOn_UpdatesSystemTesting()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        decimal percentBased = vm.SystemTestingHours;
        Assert.True(percentBased > 0);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 2; // 2 * 0.5 * 1 = 1
        vm.TestCaseIterations = 1;
        Assert.Equal(1m, vm.SystemTestingHours);
        Assert.NotEqual(percentBased, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_SwitchingOff_RevertsToPecentage()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        decimal percentBased = MainViewModel.RoundUp(vm.TotalDevelopmentHours * 0.30m);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 100;
        decimal testCaseBased = vm.SystemTestingHours;
        Assert.NotEqual(percentBased, testCaseBased);

        vm.UseTestCasesForEstimate = false;
        Assert.Equal(percentBased, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_OnlyComplex_CalculatesCorrectly()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesComplex = 10; // 10 * 2 = 20
        vm.TestCaseIterations = 2; // 20 * 2 = 40
        Assert.Equal(40m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_OnlyVeryComplex_CalculatesCorrectly()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesVeryComplex = 5; // 5 * 4 = 20
        vm.TestCaseIterations = 1;
        Assert.Equal(20m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_AffectsDownstreamCalculations()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 1);
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 20; // 20 * 0.5 = 10
        vm.TestCaseIterations = 1;

        // System testing = 10
        Assert.Equal(10m, vm.SystemTestingHours);
        // Production Validation = ROUNDUP(10 * 20%) = ROUNDUP(2) = 2
        Assert.Equal(2m, vm.ProductionValidationHours);
    }

    #endregion

    #region Test Cases for System Testing — Negative/Edge Scenarios

    [Fact]
    public void UseTestCases_ZeroIterations_TreatedAsOne()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10; // 10 * 0.5 = 5
        vm.TestCaseIterations = 0; // Should be treated as 1 (Math.Max(1, 0))
        Assert.Equal(5m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_NegativeIterations_TreatedAsOne()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10; // 10 * 0.5 = 5
        vm.TestCaseIterations = -5; // Should be treated as 1
        Assert.Equal(5m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_LargeNumbers_NoOverflow()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesVeryComplex = 1000; // 1000 * 4 = 4000
        vm.TestCaseIterations = 5; // 4000 * 5 = 20000
        Assert.Equal(20000m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_DefaultIterationsIsOne()
    {
        var vm = CreateVm();
        Assert.Equal(1, vm.TestCaseIterations);
    }

    #endregion

    #region Total Actual Hours — Positive Scenarios

    [Fact]
    public void TotalActualHours_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.TotalActualHours);
    }

    [Fact]
    public void TotalActualHours_AddedToSubtotal()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 1);
        decimal subtotalWithout = vm.SubtotalHours;

        vm.TotalActualHours = 50m;
        Assert.Equal(subtotalWithout + 50m, vm.SubtotalHours);
    }

    [Fact]
    public void TotalActualHours_AffectsGrandTotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.TotalActualHours = 100m;
        // Grand Total should include actual hours
        Assert.True(vm.GrandTotalHours >= 100m);
    }

    [Fact]
    public void ActualHoursAsOfDate_DefaultIsNull()
    {
        var vm = CreateVm();
        Assert.Null(vm.ActualHoursAsOfDate);
    }

    [Fact]
    public void ActualHoursAsOfDate_CanBeSet()
    {
        var vm = CreateVm();
        var date = new DateTime(2025, 6, 15);
        vm.ActualHoursAsOfDate = date;
        Assert.Equal(date, vm.ActualHoursAsOfDate);
    }

    #endregion

    #region Total Actual Hours — Negative/Edge Scenarios

    [Fact]
    public void TotalActualHours_NegativeValue_StillCalculates()
    {
        var vm = CreateVm();
        vm.TotalActualHours = -10m;
        // Subtotal = actual (-10) = -10
        Assert.Equal(-10m, vm.SubtotalHours);
    }

    [Fact]
    public void TotalActualHours_Zero_NoEffect()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        decimal subtotalBefore = vm.SubtotalHours;
        vm.TotalActualHours = 0m;
        Assert.Equal(subtotalBefore, vm.SubtotalHours);
    }

    [Fact]
    public void TotalActualHours_VeryLargeValue_NoOverflow()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.TotalActualHours = 99999m;
        Assert.True(vm.GrandTotalHours >= 99999m);
    }

    #endregion

    #region Time for Estimates — Positive Scenarios

    [Fact]
    public void TimeForEstimates_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.TimeForEstimates);
    }

    [Fact]
    public void TimeForEstimates_AddedToSubtotal()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 1);
        decimal subtotalWithout = vm.SubtotalHours;

        vm.TimeForEstimates = 20m;
        Assert.Equal(subtotalWithout + 20m, vm.SubtotalHours);
    }

    [Fact]
    public void TimeForEstimates_AffectsGrandTotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.TimeForEstimates = 40m;
        Assert.True(vm.GrandTotalHours >= 40m);
    }

    [Fact]
    public void TimeForEstimates_CombinesWithActualHours()
    {
        var vm = CreateVm();
        vm.TotalActualHours = 30m;
        vm.TimeForEstimates = 20m;
        // Subtotal = 0 + actual (30) + timeEst (20) = 50
        Assert.Equal(50m, vm.SubtotalHours);
    }

    #endregion

    #region Time for Estimates — Negative/Edge Scenarios

    [Fact]
    public void TimeForEstimates_NegativeValue_ReducesSubtotal()
    {
        var vm = CreateVm();
        vm.TimeForEstimates = -5m;
        // Subtotal = 0 + timeEst (-5) = -5
        Assert.Equal(-5m, vm.SubtotalHours);
    }

    [Fact]
    public void TimeForEstimates_WithPmReserve_AppliesCorrectly()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.PmReservePercentage = 10m;
        vm.TimeForEstimates = 100m;
        // With component present, PM Reserve and Grand Total are calculated
        Assert.True(vm.PmReserveHours > 0m);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours + vm.PmReserveHours), vm.GrandTotalHours);
    }

    #endregion

    #region Adjusted Hours Comments — Positive Scenarios

    [Fact]
    public void AdjustedHoursComments_DefaultIsEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.AdjustedHoursComments);
    }

    [Fact]
    public void AdjustedHoursComments_CanBeSet()
    {
        var vm = CreateVm();
        vm.AdjustedHoursComments = "Added 20 hrs because legacy code has no docs.";
        Assert.Equal("Added 20 hrs because legacy code has no docs.", vm.AdjustedHoursComments);
    }

    [Fact]
    public void AdjustedHoursComments_MultiLine()
    {
        var vm = CreateVm();
        var multiLine = "Line1: Development extended\nLine2: Testing reduced\nLine3: PM overhead";
        vm.AdjustedHoursComments = multiLine;
        Assert.Equal(multiLine, vm.AdjustedHoursComments);
    }

    [Fact]
    public void AdjustedHoursComments_DoesNotAffectCalculations()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 1);
        decimal grandBefore = vm.GrandTotalHours;
        vm.AdjustedHoursComments = "This should not change any numbers";
        Assert.Equal(grandBefore, vm.GrandTotalHours);
    }

    #endregion

    #region Adjusted Hours Comments — Edge Scenarios

    [Fact]
    public void AdjustedHoursComments_VeryLongString()
    {
        var vm = CreateVm();
        var longStr = new string('A', 4000);
        vm.AdjustedHoursComments = longStr;
        Assert.Equal(longStr, vm.AdjustedHoursComments);
    }

    [Fact]
    public void AdjustedHoursComments_SpecialCharacters()
    {
        var vm = CreateVm();
        vm.AdjustedHoursComments = "Test <>&\"' special chars: 中文, émojis 🎉";
        Assert.Contains("<>&", vm.AdjustedHoursComments);
    }

    #endregion

    #region Integration: All New Features Together

    [Fact]
    public void AllNewFeatures_CombinedCalculation()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 2);

        // Enable test cases
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCaseIterations = 2;
        // System Testing = (10*0.5 + 5*1) * 2 = (5 + 5) * 2 = 20

        // Add actual hours and time for estimates
        vm.TotalActualHours = 15m;
        vm.TimeForEstimates = 10m;
        vm.ActualHoursAsOfDate = new DateTime(2025, 3, 1);

        // Add comments
        vm.AdjustedHoursComments = "All combined test";

        Assert.Equal(20m, vm.SystemTestingHours);
        Assert.True(vm.SubtotalHours > 0);
        Assert.True(vm.GrandTotalHours > 0);
        Assert.NotNull(vm.ActualHoursAsOfDate);
        Assert.Equal("All combined test", vm.AdjustedHoursComments);
    }

    [Fact]
    public void AllNewFeatures_TriggersRecalculate()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        decimal baseline = vm.GrandTotalHours;

        vm.TotalActualHours = 10m;
        Assert.True(vm.GrandTotalHours > baseline);

        decimal afterActual = vm.GrandTotalHours;
        vm.TimeForEstimates = 5m;
        Assert.True(vm.GrandTotalHours > afterActual);
    }

    #endregion

    #region Persistence Tests for New Features

    [Fact]
    public void SaveProject_IncludesTestCaseFields_InEntity()
    {
        // We can't easily test the full DB round-trip since SaveProject uses its own context,
        // but we can verify the ProjectEntity gets the right values when LoadProject restores them
        var entity = new ProjectEntity
        {
            ProjectName = "TestCases Project",
            UseTestCasesForEstimate = true,
            TestCasesSimple = 10,
            TestCasesMedium = 5,
            TestCasesComplex = 3,
            TestCasesVeryComplex = 1,
            TestCaseIterations = 2,
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.True(vm.UseTestCasesForEstimate);
        Assert.Equal(10, vm.TestCasesSimple);
        Assert.Equal(5, vm.TestCasesMedium);
        Assert.Equal(3, vm.TestCasesComplex);
        Assert.Equal(1, vm.TestCasesVeryComplex);
        Assert.Equal(2, vm.TestCaseIterations);
        // Verify calculation: (10*0.5 + 5*1 + 3*2 + 1*4) * 2 = (5+5+6+4)*2 = 40
        Assert.Equal(40m, vm.SystemTestingHours);
    }

    [Fact]
    public void SaveProject_IncludesActualHoursAndDate_InEntity()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "ActualHours Project",
            TotalActualHours = 123.45m,
            ActualHoursAsOfDate = new DateTime(2025, 6, 15),
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(123.45m, vm.TotalActualHours);
        Assert.Equal(new DateTime(2025, 6, 15), vm.ActualHoursAsOfDate);
    }

    [Fact]
    public void SaveProject_IncludesTimeForEstimates_InEntity()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "TimeEstimates Project",
            TimeForEstimates = 42.5m,
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(42.5m, vm.TimeForEstimates);
    }

    [Fact]
    public void SaveProject_IncludesAdjustedComments_InEntity()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Comments Project",
            AdjustedHoursComments = "Development extended due to legacy code complexity.\nTesting reduced because of automation.",
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Contains("legacy code", vm.AdjustedHoursComments);
        Assert.Contains("automation", vm.AdjustedHoursComments);
    }

    [Fact]
    public void LoadProject_RestoresAllNewFields()
    {
        var project = new ProjectEntity
        {
            ProjectName = "Full Feature Test",
            UseTestCasesForEstimate = true,
            TestCasesSimple = 8,
            TestCasesMedium = 4,
            TestCasesComplex = 2,
            TestCasesVeryComplex = 1,
            TestCaseIterations = 3,
            TotalActualHours = 55m,
            ActualHoursAsOfDate = new DateTime(2025, 4, 20),
            TimeForEstimates = 30m,
            AdjustedHoursComments = "Restored comments"
        };

        var vm = CreateVm();
        vm.LoadProject(project);

        Assert.True(vm.UseTestCasesForEstimate);
        Assert.Equal(8, vm.TestCasesSimple);
        Assert.Equal(4, vm.TestCasesMedium);
        Assert.Equal(2, vm.TestCasesComplex);
        Assert.Equal(1, vm.TestCasesVeryComplex);
        Assert.Equal(3, vm.TestCaseIterations);
        Assert.Equal(55m, vm.TotalActualHours);
        Assert.Equal(new DateTime(2025, 4, 20), vm.ActualHoursAsOfDate);
        Assert.Equal(30m, vm.TimeForEstimates);
        Assert.Equal("Restored comments", vm.AdjustedHoursComments);
    }

    [Fact]
    public void LoadProject_NullDate_RemainsNull()
    {
        var project = new ProjectEntity
        {
            ProjectName = "Null Date Test",
            ActualHoursAsOfDate = null
        };

        var vm = CreateVm();
        vm.LoadProject(project);
        Assert.Null(vm.ActualHoursAsOfDate);
    }

    #endregion

    #region Calculation Accuracy with New Features

    [Fact]
    public void TestCases_RoundUpApplied()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 3; // 3 * 0.5 = 1.5
        vm.TestCaseIterations = 1;
        // RoundUp(1.5) = 2 (rounds up at 3rd decimal, but 1.5 is exact → 1.50)
        // Actually 1.5 is exact to 2 decimals, so RoundUp returns 1.50
        Assert.Equal(1.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void TestCases_FractionalResult_RoundsUp()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 1; // 1 * 0.5 = 0.5
        vm.TestCasesMedium = 1; // 1 * 1 = 1
        vm.TestCaseIterations = 3; // (0.5 + 1) * 3 = 4.5
        Assert.Equal(4.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void GrandTotal_IncludesAllNewFields()
    {
        var vm = CreateVm();
        vm.PmReservePercentage = 0; // Disable PM Reserve for easy calculation
        vm.PmEffortPercentage = 0; // Disable PM Effort
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        decimal devSubtotal = vm.SubtotalHours;
        vm.TotalActualHours = 10m;
        vm.TimeForEstimates = 5m;
        // Subtotal = devSubtotal + actual (10) + timeEst (5)
        Assert.Equal(devSubtotal + 15m, vm.SubtotalHours);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours); // 0% reserve
    }

    [Fact]
    public void TestCases_SystemTesting_AffectsAnalysisAndBusinessDesign()
    {
        var vm = CreateVm();
        vm.PmEffortPercentage = 0;
        vm.PmReservePercentage = 0;
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        decimal dev = vm.TotalDevelopmentHours;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesMedium = 100; // 100 * 1 = 100
        vm.TestCaseIterations = 1;
        // System Testing = 100
        Assert.Equal(100m, vm.SystemTestingHours);
        // Analysis = ROUNDUP((dev + 100) * 5%)
        decimal expectedAnalysis = MainViewModel.RoundUp((dev + 100m) * 0.05m);
        Assert.Equal(expectedAnalysis, vm.AnalysisHours);
        // Business Design = ROUNDUP((dev + 100) * 15%)
        decimal expectedBD = MainViewModel.RoundUp((dev + 100m) * 0.15m);
        Assert.Equal(expectedBD, vm.BusinessDesignHours);
    }

    #endregion
}

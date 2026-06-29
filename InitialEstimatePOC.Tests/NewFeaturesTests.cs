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
        vm.TestCasesSimple = 10;     // row31: 10*2.1925=21.925  row32defect: 10*1.5675*0.1=1.5675
        vm.TestCasesMedium = 5;      // row31: 5*4.065=20.325    row32defect: 5*3.44*0.1=1.72
        vm.TestCasesComplex = 3;     // row31: 3*8.76=26.28      row32defect: 3*7.51*0.1=2.253
        vm.TestCasesVeryComplex = 2; // row31: 2*14.38=28.76     row32defect: 2*13.13*0.1=2.626
        vm.TestCaseIterations = 1;
        // main=(97.29) defect=(8.1665) → ROUNDUP((97.29+8.1665)*1,2) = 105.46
        Assert.Equal(105.46m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_WithIterations_MultipliesCorrectly()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 4;   // row31: 4*2.1925=8.77   row32defect: 4*1.5675*0.1=0.627
        vm.TestCasesMedium = 2;   // row31: 2*4.065=8.13    row32defect: 2*3.44*0.1=0.688
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 3;
        // main=(16.9) defect=(1.315) → ROUNDUP(18.215*3,2) = 54.65
        Assert.Equal(54.65m, vm.SystemTestingHours);
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
        vm.TestCasesSimple = 2; // row31: 2*2.1925=4.385 row32defect: 2*1.5675*0.1=0.3135 → ROUNDUP(4.6985,2)=4.70
        vm.TestCaseIterations = 1;
        Assert.Equal(4.70m, vm.SystemTestingHours);
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
        vm.TestCasesComplex = 10; // row31: 10*8.76=87.6  row32defect: 10*7.51*0.1=7.51 → ROUNDUP(95.11*2,2)=190.22
        vm.TestCaseIterations = 2;
        Assert.Equal(190.22m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_OnlyVeryComplex_CalculatesCorrectly()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesVeryComplex = 5; // row31: 5*14.38=71.9  row32defect: 5*13.13*0.1=6.565 → ROUNDUP(78.465,2)=78.47
        vm.TestCaseIterations = 1;
        Assert.Equal(78.47m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_AffectsDownstreamCalculations()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 1);
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 20; // row31: 20*2.1925=43.85  row32defect: 20*1.5675*0.1=3.135 → ROUNDUP(46.985,2)=46.99
        vm.TestCaseIterations = 1;

        // System testing = 46.99
        Assert.Equal(46.99m, vm.SystemTestingHours);
        // Production Validation = ROUNDUP(46.99 * 20%) = ROUNDUP(9.398) = 9.40
        Assert.Equal(9.40m, vm.ProductionValidationHours);
    }

    #endregion

    #region Test Cases for System Testing — Negative/Edge Scenarios

    [Fact]
    public void UseTestCases_ZeroIterations_TreatedAsOne()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10; // ROUNDUP((10*2.1925 + 10*1.5675*0.1)*1,2) = ROUNDUP(23.4925,2) = 23.50
        vm.TestCaseIterations = 0; // Should be treated as 1 (Math.Max(1, 0))
        Assert.Equal(23.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_NegativeIterations_TreatedAsOne()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10; // ROUNDUP((10*2.1925 + 10*1.5675*0.1)*1,2) = ROUNDUP(23.4925,2) = 23.50
        vm.TestCaseIterations = -5; // Should be treated as 1
        Assert.Equal(23.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_LargeNumbers_NoOverflow()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesVeryComplex = 1000; // row31: 1000*14.38=14380  row32defect: 1000*13.13*0.1=1313 → (14380+1313)*5=78465
        vm.TestCaseIterations = 5;
        Assert.Equal(78465m, vm.SystemTestingHours);
    }

    [Fact]
    public void UseTestCases_DefaultIterationsIsOne()
    {
        var vm = CreateVm();
        Assert.Equal(1m, vm.TestCaseIterations);
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
    public void TimeForEstimates_ComponentPresentAffectsGrandTotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.TimeForEstimates = 100m;
        // With component present, Grand Total is ceiling of SubtotalHours
        Assert.True(vm.GrandTotalHours > 0m);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
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

        // Enable test cases (using new row31/row32 formula)
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCaseIterations = 2;
        // row31: 10*2.1925+5*4.065=42.25  row32defect: (10*1.5675+5*3.44)*0.1=3.2875 → ROUNDUP((42.25+3.2875)*2,2)=91.08
        decimal expectedSysTest = MainViewModel.RoundUp(
            (10m * 2.1925m + 5m * 4.065m + (10m * 1.5675m + 5m * 3.44m) * 0.1m) * 2m);

        // Add actual hours and time for estimates
        vm.TotalActualHours = 15m;
        vm.TimeForEstimates = 10m;
        vm.ActualHoursAsOfDate = new DateTime(2025, 3, 1);

        // Add comments
        vm.AdjustedHoursComments = "All combined test";

        Assert.Equal(expectedSysTest, vm.SystemTestingHours);
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
        Assert.Equal(2m, vm.TestCaseIterations);
        // Verify calculation: row31: 10*2.1925+5*4.065+3*8.76+1*14.38=82.91  defect*0.1=6.8535  → ROUNDUP(89.7635*2,2)=179.53
        Assert.Equal(179.53m, vm.SystemTestingHours);
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
        vm.TestCasesSimple = 3; // ROUNDUP((3*2.1925 + 3*1.5675*0.1)*1,2) = ROUNDUP(7.04775,2) = 7.05
        vm.TestCaseIterations = 1;
        Assert.Equal(7.05m, vm.SystemTestingHours);
    }

    [Fact]
    public void TestCases_FractionalResult_RoundsUp()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 1; // row31: 2.1925  row32defect: 1.5675*0.1=0.15675
        vm.TestCasesMedium = 1; // row31: 4.065   row32defect: 3.44*0.1=0.344
        vm.TestCaseIterations = 3; // ROUNDUP((6.2575+0.50075)*3,2) = ROUNDUP(20.27475,2) = 20.28
        Assert.Equal(20.28m, vm.SystemTestingHours);
    }

    [Fact]
    public void GrandTotal_IncludesAllNewFields()
    {
        var vm = CreateVm();
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
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    [Fact]
    public void TestCases_SystemTesting_AffectsAnalysisAndBusinessDesign()
    {
        var vm = CreateVm();
        vm.PmEffortPercentage = 0;
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        decimal dev = vm.TotalDevelopmentHours;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesMedium = 100; // row31: 100*4.065=406.5  row32defect: 100*3.44*0.1=34.4 → ROUNDUP(440.9,2)=440.90
        vm.TestCaseIterations = 1;
        // System Testing = 440.90
        Assert.Equal(440.90m, vm.SystemTestingHours);
        // Analysis = ROUNDUP((dev + 440.90) * 5%)
        decimal expectedAnalysis = MainViewModel.RoundUp((dev + 440.90m) * 0.05m);
        Assert.Equal(expectedAnalysis, vm.AnalysisHours);
        // Business Design = ROUNDUP((dev + 440.90) * 15%)
        decimal expectedBD = MainViewModel.RoundUp((dev + 440.90m) * 0.15m);
        Assert.Equal(expectedBD, vm.BusinessDesignHours);
    }

    #endregion
}

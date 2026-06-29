using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Full end-to-end calculation tests verifying the complete pipeline
/// matches the Excel worked example from documentation. Tests all derived
/// tasks, role breakout, and grand total step by step.
/// </summary>
public class FullCalculationPipelineTests
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

    #region Worked Example — Full Pipeline (Section 8 of Documentation)

    /// <summary>
    /// Complete verification of the worked example from documentation.
    /// Dev=953.10, System Testing=285.93, Analysis=61.96, Business Design=185.86,
    /// Promotion=47.66, BA Sys Doc=47.66, Prod Validation=57.19, PM=245.91
    /// </summary>
    [Fact]
    public void WorkedExample_AllDerivedTasks_MatchDocumentation()
    {
        var vm = CreateVm();
        // Clear default collaboration for pure calculation test
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 3);   // 225.00
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 5); // 104.70
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 2);                // 34.00
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 1);  // 294.40
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 4);        // 60.00
        AddComponent(vm, ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8);         // 65.00
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1);            // 100.00
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 2);        // 70.00

        decimal dev = 225.00m + 104.70m + 34.00m + 294.40m + 60.00m + 65.00m + 100.00m + 70.00m;
        Assert.Equal(dev, vm.TotalDevelopmentHours); // 953.10

        // Step 2: System Testing = ROUNDUP(953.10 × 0.30, 2) = 285.93
        Assert.Equal(285.93m, vm.SystemTestingHours);

        // Step 3: Analysis = ROUNDUP((953.10 + 285.93) × 0.05, 2) = 61.96
        Assert.Equal(61.96m, vm.AnalysisHours);

        // Step 4: Business Design = ROUNDUP((953.10 + 285.93) × 0.15, 2) = 185.86
        Assert.Equal(185.86m, vm.BusinessDesignHours);

        // Step 5: Promotion = ROUNDUP(953.10 × 0.05, 2) = 47.66
        Assert.Equal(47.66m, vm.PromotionHours);

        // Step 6: BA System Doc = ROUNDUP(953.10 × 0.05, 2) = 47.66
        Assert.Equal(47.66m, vm.BaSystemDocHours);

        // Step 7: Production Validation = ROUNDUP(285.93 × 0.20, 2) = 57.19
        Assert.Equal(57.19m, vm.ProductionValidationHours);

        // Step 8: PM Effort = ROUNDUP((953.10+285.93+61.96+185.86+47.66+47.66+57.19) × 0.15, 2)
        // = ROUNDUP(1639.36 × 0.15, 2) = ROUNDUP(245.904, 2) = 245.91
        Assert.Equal(245.91m, vm.ProjectManagementHours);
    }

    [Fact]
    public void WorkedExample_RoleBreakout_MatchesExcel()
    {
        var vm = CreateVm();
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

        // BA = Analysis/2 + BusinessDesign + BADoc + ProdValidation + PM/2
        // = 61.96/2 + 185.86 + 47.66 + 57.19 + 245.91/2
        // = 30.98 + 185.86 + 47.66 + 57.19 + 122.955
        // = 444.645 → ROUNDUP = 444.65
        decimal baExpected = MainViewModel.RoundUp(
            vm.AnalysisHours / 2m + vm.BusinessDesignHours + vm.BaSystemDocHours
            + vm.ProductionValidationHours + vm.TotalActualHours / 2m + vm.TimeForEstimates / 2m);
        Assert.Equal(baExpected, vm.BaRoleHours);

        // SE = Dev + Analysis/2 + Promotion + ActualHours/2 + TimeForEstimates/2
        decimal seExpected = MainViewModel.RoundUp(
            vm.TotalDevelopmentHours + vm.AnalysisHours / 2m + vm.PromotionHours
            + vm.TotalActualHours / 2m + vm.TimeForEstimates / 2m);
        Assert.Equal(seExpected, vm.SeRoleHours);

        // Tester = System Testing
        Assert.Equal(vm.SystemTestingHours, vm.TesterRoleHours);

        // PM = PM Effort
        Assert.Equal(vm.ProjectManagementHours, vm.PmRoleHours);

        // Collaboration = 0 (we cleared it)
        Assert.Equal(0m, vm.CollaborationRoleHours);
    }

    [Fact]
    public void WorkedExample_GrandTotal_MatchesDocumentation()
    {
        var vm = CreateVm();
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

        // Subtotal = Dev+derived+PM = 953.10+285.93+61.96+185.86+47.66+47.66+57.19+245.91 = 1885.27
        decimal expectedSubtotal = 953.10m + 285.93m + 61.96m + 185.86m + 47.66m + 47.66m + 57.19m + 245.91m;
        Assert.Equal(expectedSubtotal, vm.SubtotalHours);

        // Grand Total = Math.Ceiling(1885.27) = 1886
        Assert.Equal(Math.Ceiling(expectedSubtotal), vm.GrandTotalHours);

        // T-Shirt = XL1 (1000-1999)
        Assert.Equal("XL1", vm.TShirtSize);
    }

    #endregion

    #region Simple Component Tests — Each Type Individually

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 25.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium, ChangeType.Change, 40.80)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change, 235.525)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.New, 11.875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change, 4.6875)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New, 8.125)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 60.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.Change, 150.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New, 15.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New, 8.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Large, ChangeType.Change, 50.00)]
    public void SingleComponent_Count1_DevEqualsBaseHours(ComponentType type, ComponentSize size, ChangeType change, decimal expectedBase)
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        // Create a row and set a different type first to ensure property changed fires
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        // Force a different type first so the Changed handler fires when we set target type
        if (type == ComponentType.PowerBuilderWindows)
            row.ComponentType = ComponentType.Reports;
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = 1;

        Assert.Equal(expectedBase, vm.TotalDevelopmentHours);
    }

    [Theory]
    [InlineData(1, 100.00)]
    [InlineData(2, 200.00)]
    [InlineData(5, 500.00)]
    [InlineData(10, 1000.00)]
    public void ComponentCount_MultipliesBaseHours(int count, decimal expectedDev)
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        // MISC Large New = 100 hrs base
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, count);

        Assert.Equal(expectedDev, vm.TotalDevelopmentHours);
    }

    #endregion

    #region PM Effort Percentage Variations

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    public void PMEffortDropdown_AllOptions_CalculateCorrectly(decimal pmPercent)
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1); // 100 hrs
        vm.PmEffortPercentage = pmPercent;

        // Dev=100, SysTest=30, Analysis=6.50, BizDesign=19.50, Promotion=5, BADoc=5, ProdVal=6 = 172
        decimal devPlusDerived = 100m + 30m + 6.50m + 19.50m + 5m + 5m + 6m;
        decimal expectedPM = MainViewModel.RoundUp(devPlusDerived * (pmPercent / 100m));
        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion

    #region Calculation Order Dependencies

    [Fact]
    public void CalculationOrder_SystemTestingBeforeAnalysis()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 1); // 60 hrs

        // SysTest = ROUNDUP(60 * 0.30, 2) = 18.00
        // Analysis = ROUNDUP((60 + 18) * 0.05, 2) = ROUNDUP(3.90, 2) = 3.90
        Assert.Equal(18.00m, vm.SystemTestingHours);
        Assert.Equal(3.90m, vm.AnalysisHours);
    }

    [Fact]
    public void CalculationOrder_SystemTestingBeforeBusinessDesign()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 1); // 60 hrs

        // BizDesign = ROUNDUP((60 + 18) * 0.15, 2) = ROUNDUP(11.70, 2) = 11.70
        Assert.Equal(11.70m, vm.BusinessDesignHours);
    }

    [Fact]
    public void CalculationOrder_SystemTestingBeforeProductionValidation()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 1); // 60 hrs

        // ProdVal = ROUNDUP(18 * 0.20, 2) = ROUNDUP(3.60, 2) = 3.60
        Assert.Equal(3.60m, vm.ProductionValidationHours);
    }

    [Fact]
    public void CalculationOrder_AllDerivedBeforePM()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 1); // 60 hrs

        // All derived: 60 + 18 + 3.90 + 11.70 + 3 + 3 + 3.60 = 103.20
        decimal sum = vm.TotalDevelopmentHours + vm.SystemTestingHours + vm.AnalysisHours
                    + vm.BusinessDesignHours + vm.PromotionHours + vm.BaSystemDocHours
                    + vm.ProductionValidationHours;
        decimal expectedPM = MainViewModel.RoundUp(sum * 0.15m);
        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion

    #region Test Cases Override — Full Pipeline

    [Fact]
    public void TestCaseOverride_AffectsEntirePipeline()
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1); // Dev = 100

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;     // row31: 10*2.1925=21.925  row32defect: 10*1.5675*0.1=1.5675
        vm.TestCasesMedium = 5;      // row31: 5*4.065=20.325    row32defect: 5*3.44*0.1=1.72
        vm.TestCasesComplex = 3;     // row31: 3*8.76=26.28      row32defect: 3*7.51*0.1=2.253
        vm.TestCasesVeryComplex = 1; // row31: 1*14.38=14.38     row32defect: 1*13.13*0.1=1.313
        vm.TestCaseIterations = 2;   // main=82.91 defect=6.8535 → ROUNDUP(89.7635*2,2)=179.53

        Assert.Equal(100m, vm.TotalDevelopmentHours);
        Assert.Equal(179.53m, vm.SystemTestingHours);

        // Analysis = ROUNDUP((100 + 179.53) * 0.05, 2) = ROUNDUP(13.9765, 2) = 13.98
        Assert.Equal(13.98m, vm.AnalysisHours);

        // BizDesign = ROUNDUP((100 + 179.53) * 0.15, 2) = ROUNDUP(41.9295, 2) = 41.93
        Assert.Equal(41.93m, vm.BusinessDesignHours);

        // Promotion = ROUNDUP(100 * 0.05, 2) = 5.00
        Assert.Equal(5.00m, vm.PromotionHours);

        // BADoc = ROUNDUP(100 * 0.05, 2) = 5.00
        Assert.Equal(5.00m, vm.BaSystemDocHours);

        // ProdVal = ROUNDUP(179.53 * 0.20, 2) = ROUNDUP(35.906, 2) = 35.91
        Assert.Equal(35.91m, vm.ProductionValidationHours);

        // PM = ROUNDUP((100+179.53+13.98+41.93+5+5+35.91) * 0.15, 2) = ROUNDUP(381.35*0.15,2) = ROUNDUP(57.2025,2) = 57.21
        Assert.Equal(57.21m, vm.ProjectManagementHours);
    }

    #endregion

    #region Complete Subtotal Verification

    [Fact]
    public void Subtotal_IncludesAllComponents()
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1); // 100

        vm.DevelopmentAdjustedHours = 10m;
        vm.TimeForEstimates = 20m;
        vm.TotalActualHours = 5m;

        // effectiveDev = 100 + 10 = 110 (dev adjustment cascades into derived tasks)
        // SysTest = ROUNDUP(110*0.30) = 33
        // Analysis = ROUNDUP(143*0.05) = 7.15
        // BizDesign = ROUNDUP(143*0.15) = 21.45
        // Promotion = ROUNDUP(110*0.05) = 5.50
        // BaSysDoc = ROUNDUP(110*0.05) = 5.50
        // ProdVal = ROUNDUP(33*0.20) = 6.60
        // devPlusDerived = 110+33+7.15+21.45+5.50+5.50+6.60 = 189.20
        // PM = ROUNDUP(189.20*0.15) = 28.38
        // Subtotal = 189.20 + 28.38 + 0 + 0 + 20 + 5 = 242.58
        decimal effectiveDev = 110m;
        decimal sysTest = MainViewModel.RoundUp(effectiveDev * 0.30m);
        decimal analysis = MainViewModel.RoundUp((effectiveDev + sysTest) * 0.05m);
        decimal bizDesign = MainViewModel.RoundUp((effectiveDev + sysTest) * 0.15m);
        decimal promotion = MainViewModel.RoundUp(effectiveDev * 0.05m);
        decimal baSysDoc = MainViewModel.RoundUp(effectiveDev * 0.05m);
        decimal prodVal = MainViewModel.RoundUp(sysTest * 0.20m);
        decimal devPlusDerived = effectiveDev + sysTest + analysis + bizDesign + promotion + baSysDoc + prodVal;
        decimal pm = MainViewModel.RoundUp(devPlusDerived * 0.15m);
        decimal expected = devPlusDerived + pm + 0m + 0m + 20m + 5m;
        Assert.Equal(expected, vm.SubtotalHours);
    }

    #endregion

    #region Database Review — Same for All Sizes

    [Fact]
    public void DatabaseReview_AllSizes_SameHours_New()
    {
        // Database Review has same hours regardless of size
        Assert.Equal(8.125m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New));
        Assert.Equal(8.125m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.New));
        Assert.Equal(8.125m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New));
    }

    [Fact]
    public void DatabaseReview_AllSizes_SameHours_Change()
    {
        Assert.Equal(6.10m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.Change));
        Assert.Equal(6.10m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.Change));
        Assert.Equal(6.10m, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.Change));
    }

    #endregion
}

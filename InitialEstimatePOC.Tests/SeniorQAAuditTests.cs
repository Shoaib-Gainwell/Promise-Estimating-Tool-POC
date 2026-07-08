using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Senior QA Audit — Comprehensive end-to-end tests verifying the .NET tool
/// produces EXACTLY the same results as "CO 23327 002 Final Estimate V1.0.xlsm".
/// 
/// Covers:
/// - Happy path: Full Excel scenario exact match (every intermediate value)
/// - Sad path: Missing/invalid inputs, zero components, empty state
/// - Negative path: Negative adjustments, boundary overflows
/// - Formula verification: Each derived task formula independently
/// - Role breakout: Exact match with Excel B47:B51
/// - T-Shirt sizing: Every boundary
/// - Collaboration: All formula variations
/// - RoundUp: Exhaustive edge cases matching Excel ROUNDUP(x, 2)
/// </summary>
public class SeniorQAAuditTests
{
    #region === HAPPY PATH: Full Excel CO 23327 002 Exact Match ===

    /// <summary>
    /// THE GOLD STANDARD TEST: Reproduces the entire Excel spreadsheet and verifies
    /// every single calculated value matches cell-for-cell.
    /// Excel: CO 23327 002 Final Estimate V1.0.xlsm → InitialEstimate sheet
    /// </summary>
    [Fact]
    public void Excel_CO23327_FullPipeline_ExactCellByCell_Match()
    {
        var vm = CreateExcelScenarioVm();

        // === G column (Calculated values) ===
        // G27: Development calculated = sum of components = 596.5625
        Assert.Equal(596.5625m, vm.TotalDevelopmentHours);

        // G30: System Testing (test case formula) = 2517.46
        Assert.Equal(2517.46m, vm.SystemTestingHours);

        // G28: Analysis = ROUNDUP((I27+I30) * 5%, 2) = ROUNDUP((596.5625+2517.46)*0.05, 2) = 155.71
        Assert.Equal(155.71m, vm.AnalysisHours);

        // G29: Business Design = ROUNDUP((I27+I30) * 15%, 2) = 467.11
        Assert.Equal(467.11m, vm.BusinessDesignHours);

        // G31: Promotion = ROUNDUP(I27 * 5%, 2) = ROUNDUP(596.5625*0.05, 2) = 29.83
        Assert.Equal(29.83m, vm.PromotionHours);

        // G32: BA System Doc = ROUNDUP(I27 * 5%, 2) = 29.83
        Assert.Equal(29.83m, vm.BaSystemDocHours);

        // G33: Production Validation = ROUNDUP(I30 * 20%, 2) = ROUNDUP(2517.46*0.20, 2) = 503.50
        Assert.Equal(503.50m, vm.ProductionValidationHours);

        // === I column (Total = Calculated + Adjusted) ===
        // I27: Dev Total = 596.5625 + 0 = 596.5625
        Assert.Equal(596.5625m, vm.DevelopmentTotalHours);

        // I30: SysTest Total = 2517.46 + 0 = 2517.46
        Assert.Equal(2517.46m, vm.SystemTestingTotalHours);

        // I28: Analysis Total = 155.71 + 0 = 155.71
        Assert.Equal(155.71m, vm.AnalysisTotalHours);

        // I29: BizDesign Total = 467.11 + 0 = 467.11
        Assert.Equal(467.11m, vm.BusinessDesignTotalHours);

        // I31: Promotion Total = 29.83 + 0 = 29.83
        Assert.Equal(29.83m, vm.PromotionTotalHours);

        // I32: BA Sys Doc Total = 29.83 + 1.17 = 31.00
        Assert.Equal(31.00m, vm.BaSystemDocTotalHours);

        // I33: Production Validation Total = 503.50 + 0 = 503.50
        Assert.Equal(503.50m, vm.ProductionValidationTotalHours);

        // G34: PM = ROUNDUP((I27+I28+I29+I30+I31+I32+I33) * 15%, 2)
        // = ROUNDUP((596.5625+155.71+467.11+2517.46+29.83+31+503.5) * 0.15, 2)
        // = ROUNDUP(4301.1725 * 0.15, 2) = ROUNDUP(645.175875, 2) = 645.18
        Assert.Equal(645.18m, vm.ProjectManagementHours);

        // I34: PM Total = 645.18 + 0 = 645.18
        Assert.Equal(645.18m, vm.ProjectManagementTotalHours);

        // === Collaboration (I36:I40) ===
        Assert.Equal(125m, vm.WprsTotalHours);       // WPRs: 20×(15/60+60/60)×5 = 125
        Assert.Equal(42m, vm.ClientMeetingsTotalHours); // Client: 7×(60/60+60/60)×3 = 42
        Assert.Equal(18.75m, vm.InternalMeetingsTotalHours); // Internal: 3×(15/60+60/60)×5 = 18.75
        Assert.Equal(0m, vm.AutomationTestCollabTotalHours);
        Assert.Equal(0m, vm.ConsultantMentorTotalHours);
        Assert.Equal(185.75m, vm.TotalCollaborationHours);

        // === I42: Time for Estimates = 129 ===
        Assert.Equal(129m, vm.TimeForEstimates);

        // === I41: Actual Hours = 0 ===
        Assert.Equal(0m, vm.TotalActualHours);

        // === I43: Subtotal = ROUNDUP(SUM(I27:I42), 2) ===
        // = ROUNDUP(596.5625 + 155.71 + 467.11 + 2517.46 + 29.83 + 31 + 503.5 + 645.18 + 185.75 + 129 + 0, 2)
        // = ROUNDUP(5261.1025, 2) = 5261.11
        Assert.Equal(5261.11m, vm.SubtotalHours);

        // === I3: Grand Total = ROUNDUP(I43, 0) = Ceiling(5261.11) = 5262 ===
        Assert.Equal(5262m, vm.GrandTotalHours);

        // === T-Shirt Size: 5262 → XL5 (5000-5999) ===
        Assert.Equal("XL5", vm.TShirtSize);
    }

    /// <summary>
    /// Excel Role Breakout B47:B51 — exact match with formulas from spreadsheet.
    /// </summary>
    [Fact]
    public void Excel_CO23327_RoleBreakout_ExactMatch()
    {
        var vm = CreateExcelScenarioVm();

        // B47: BA = ROUNDUP((I28/2)+I29+I32+I33+(I41/2)+(I42/2), 2)
        // = ROUNDUP((155.71/2)+467.11+31+503.5+(0/2)+(129/2), 2)
        // = ROUNDUP(77.855+467.11+31+503.5+0+64.5, 2)
        // = ROUNDUP(1143.965, 2) = 1143.97
        Assert.Equal(1143.97m, vm.BaRoleHours);

        // B48: SE = ROUNDUP(I27+(I28/2)+I31+(I41/2)+(I42/2), 2)
        // = ROUNDUP(596.5625+(155.71/2)+29.83+(0/2)+(129/2), 2)
        // = ROUNDUP(596.5625+77.855+29.83+0+64.5, 2)
        // = ROUNDUP(768.7475, 2) = 768.75
        Assert.Equal(768.75m, vm.SeRoleHours);

        // B49: Tester = I30 = System Testing Total = 2517.46
        Assert.Equal(2517.46m, vm.TesterRoleHours);

        // B50: PM = I34 = PM Total = 645.18
        Assert.Equal(645.18m, vm.PmRoleHours);

        // B51: Collaboration = SUM(I36:I40) = 185.75
        Assert.Equal(185.75m, vm.CollaborationRoleHours);
    }

    /// <summary>
    /// Verify each component row matches Excel exactly (rows 7-9).
    /// </summary>
    [Fact]
    public void Excel_CO23327_ComponentRows_ExactMatch()
    {
        var vm = CreateExcelScenarioVm();

        // Row 7: DB Manipulation, Change, Large, Count=2, Base=25.625, Total=51.25
        Assert.Equal(25.625m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(51.25m, vm.Components[0].TotalHours);

        // Row 8: DB Manipulation, New, Large, Count=1, Base=31.875, Total=31.875
        Assert.Equal(31.875m, vm.Components[1].BaseHoursPerUnit);
        Assert.Equal(31.875m, vm.Components[1].TotalHours);

        // Row 9: Support Modules, Change, Medium, Count=53, Base=9.6875, Total=513.4375
        Assert.Equal(9.6875m, vm.Components[2].BaseHoursPerUnit);
        Assert.Equal(513.4375m, vm.Components[2].TotalHours);

        // Sum = 51.25 + 31.875 + 513.4375 = 596.5625
        Assert.Equal(596.5625m, vm.Components[0].TotalHours + vm.Components[1].TotalHours + vm.Components[2].TotalHours);
    }

    /// <summary>
    /// Verify test case formula intermediate values match Excel exactly.
    /// </summary>
    [Fact]
    public void Excel_CO23327_TestCaseFormula_IntermediateValues()
    {
        // Excel G30 formula with K30=125, L30=0, M30=75, N30=0, O30=2.5
        // Main hours = 125*2.1925 + 0*4.065 + 75*8.76 + 0*14.38
        //            = 274.0625 + 0 + 657 + 0 = 931.0625
        decimal mainHours = 125m * 2.1925m + 0m * 4.065m + 75m * 8.76m + 0m * 14.38m;
        Assert.Equal(931.0625m, mainHours);

        // Defect hours = (125*1.5675 + 0*3.44 + 75*7.51 + 0*13.13) * 0.1
        //             = (195.9375 + 0 + 563.25 + 0) * 0.1 = 759.1875 * 0.1 = 75.91875
        decimal defectHours = (125m * 1.5675m + 0m * 3.44m + 75m * 7.51m + 0m * 13.13m) * 0.1m;
        Assert.Equal(75.91875m, defectHours);

        // Total before iterations = 931.0625 + 75.91875 = 1006.98125
        decimal totalBeforeIter = mainHours + defectHours;
        Assert.Equal(1006.98125m, totalBeforeIter);

        // With 2.5 iterations: 1006.98125 * 2.5 = 2517.453125
        decimal withIterations = totalBeforeIter * 2.5m;
        Assert.Equal(2517.453125m, withIterations);

        // ROUNDUP(2517.453125, 2) = 2517.46
        Assert.Equal(2517.46m, MainViewModel.RoundUp(withIterations));
    }

    #endregion

    #region === HAPPY PATH: Standard Formula (No Test Cases) ===

    [Fact]
    public void StandardFormula_SingleComponent_AllDerivedTasksCorrect()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Dev = 125
        Assert.Equal(125m, vm.TotalDevelopmentHours);

        // System Testing = ROUNDUP(125 * 0.30, 2) = ROUNDUP(37.5, 2) = 37.50
        Assert.Equal(37.50m, vm.SystemTestingHours);

        // Analysis = ROUNDUP((125 + 37.50) * 0.05, 2) = ROUNDUP(8.125, 2) = 8.13
        Assert.Equal(8.13m, vm.AnalysisHours);

        // Business Design = ROUNDUP((125 + 37.50) * 0.15, 2) = ROUNDUP(24.375, 2) = 24.38
        Assert.Equal(24.38m, vm.BusinessDesignHours);

        // Promotion = ROUNDUP(125 * 0.05, 2) = ROUNDUP(6.25, 2) = 6.25
        Assert.Equal(6.25m, vm.PromotionHours);

        // BA Sys Doc = ROUNDUP(125 * 0.05, 2) = 6.25
        Assert.Equal(6.25m, vm.BaSystemDocHours);

        // Production Validation = ROUNDUP(37.50 * 0.20, 2) = ROUNDUP(7.5, 2) = 7.50
        Assert.Equal(7.50m, vm.ProductionValidationHours);

        // PM = ROUNDUP((125+37.50+8.13+24.38+6.25+6.25+7.50) * 0.15, 2)
        //    = ROUNDUP(215.01 * 0.15, 2) = ROUNDUP(32.2515, 2) = 32.26
        Assert.Equal(32.26m, vm.ProjectManagementHours);
    }

    [Fact]
    public void StandardFormula_MultipleComponents_SumsCorrectly()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        // Add 3 different components
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 5; // 5 × 17 = 85

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].ComponentType = ComponentType.Webpage;
        vm.Components[1].Size = ComponentSize.Medium;
        vm.Components[1].ChangeType = ChangeType.Change;
        vm.Components[1].Count = 2; // 2 × 48 = 96

        vm.AddComponentCommand.Execute(null);
        vm.Components[2].ComponentType = ComponentType.K2Workflow;
        vm.Components[2].Size = ComponentSize.Large;
        vm.Components[2].ChangeType = ChangeType.New;
        vm.Components[2].Count = 1; // 1 × 200 = 200

        // Dev = 85 + 96 + 200 = 381
        Assert.Equal(381m, vm.TotalDevelopmentHours);

        // Verify cascade
        decimal expectedSysTest = MainViewModel.RoundUp(381m * 0.30m);
        Assert.Equal(expectedSysTest, vm.SystemTestingHours);

        decimal expectedAnalysis = MainViewModel.RoundUp((381m + expectedSysTest) * 0.05m);
        Assert.Equal(expectedAnalysis, vm.AnalysisHours);
    }

    #endregion

    #region === SAD PATH: Empty/Invalid States ===

    [Fact]
    public void EmptyState_NoComponents_AllZeros()
    {
        var vm = new MainViewModel();
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.SystemTestingHours);
        Assert.Equal(0m, vm.AnalysisHours);
        Assert.Equal(0m, vm.BusinessDesignHours);
        Assert.Equal(0m, vm.PromotionHours);
        Assert.Equal(0m, vm.BaSystemDocHours);
        Assert.Equal(0m, vm.ProductionValidationHours);
        Assert.Equal(0m, vm.ProjectManagementHours);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void ComponentWithNoneType_ZeroBaseHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.None;
        vm.Components[0].Size = ComponentSize.Medium;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 5;

        // ComponentType.None not in weighted values → base = 0
        Assert.Equal(0m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(0m, vm.Components[0].TotalHours);
    }

    [Fact]
    public void ComponentWithNoneSize_ZeroBaseHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.None;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 3;

        Assert.Equal(0m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(0m, vm.Components[0].TotalHours);
    }

    [Fact]
    public void ComponentWithNoneChangeType_ZeroBaseHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Webpage;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.None;
        vm.Components[0].Count = 2;

        Assert.Equal(0m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(0m, vm.Components[0].TotalHours);
    }

    [Fact]
    public void ComponentWithZeroCount_ZeroTotalHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Webpage;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 0;

        // When Count <= 0, UpdateBaseHours clears BaseHoursPerUnit to 0 (by design)
        Assert.Equal(0m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(0m, vm.Components[0].TotalHours);
    }

    [Fact]
    public void ClearAll_ResetsEverything_ToInitialState()
    {
        var vm = CreateExcelScenarioVm();
        Assert.True(vm.GrandTotalHours > 0);

        vm.ClearAllCommand.Execute(null);

        Assert.Equal(0, vm.Components.Count);
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal("—", vm.TShirtSize);
        Assert.Equal(15m, vm.PmEffortPercentage); // reset to default
        Assert.Equal(0m, vm.BaSystemDocAdjustedHours);
        Assert.Equal(0m, vm.TimeForEstimates);
        Assert.False(vm.UseTestCasesForEstimate);
    }

    [Fact]
    public void RemoveAllComponents_ResetsToZero()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.Medium;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 3;

        Assert.True(vm.GrandTotalHours > 0);

        vm.RemoveComponentCommand.Execute(vm.Components[0]);

        Assert.Equal(0, vm.Components.Count);
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void SaveProject_MissingProjectName_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "";
        vm.ChangeOrderId = "CO-123";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";

        var result = vm.SaveProject();
        Assert.Equal("Project Name is required.", result);
    }

    [Fact]
    public void SaveProject_MissingChangeOrderId_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";

        var result = vm.SaveProject();
        Assert.Equal("CO / Defect # is required.", result);
    }

    [Fact]
    public void SaveProject_NoComponents_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-123";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";

        var result = vm.SaveProject();
        Assert.Equal("At least one component must be added before saving.", result);
    }

    #endregion

    #region === NEGATIVE PATH: Negative Adjustments ===

    [Fact]
    public void NegativeAdjustment_Development_ReducesEffective()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Dev = 125, then adjust by -25
        vm.DevelopmentAdjustedHours = -25m;

        // Effective Dev = 125 + (-25) = 100
        Assert.Equal(100m, vm.DevelopmentTotalHours);

        // System Testing now based on effective=100
        Assert.Equal(MainViewModel.RoundUp(100m * 0.30m), vm.SystemTestingHours);
    }

    [Fact]
    public void NegativeAdjustment_AllTasks_StillCalculates()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Webpage;
        vm.Components[0].Size = ComponentSize.Medium;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 60 hours

        vm.DevelopmentAdjustedHours = -10m;
        vm.SystemTestingAdjustedHours = -5m;
        vm.AnalysisAdjustedHours = -2m;
        vm.BusinessDesignAdjustedHours = -3m;

        // Should still calculate without error
        Assert.True(vm.GrandTotalHours > 0);
        Assert.Equal(50m, vm.DevelopmentTotalHours); // 60 - 10
    }

    [Fact]
    public void NegativeAdjustment_ExceedingCalculated_ResultsInNegativeEffective()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 17 hours

        // Adjusted exceeds calculated → negative effective
        vm.DevelopmentAdjustedHours = -50m;

        // Effective Dev = 17 + (-50) = -33
        Assert.Equal(-33m, vm.DevelopmentTotalHours);

        // The calculation should still proceed (no crash)
        // System Testing will be based on negative effective
        Assert.True(vm.SystemTestingHours < 0);
    }

    [Fact]
    public void LargeNegativeAdjustment_DoesNotCrash()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.DevelopmentAdjustedHours = -999999m;

        // Should not throw, should calculate (even if results are negative)
        Assert.NotNull(vm.TShirtSize);
    }

    #endregion

    #region === BOUNDARY: PM Effort Percentage ===

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void PMEffort_AllValidPercentages_CalculateCorrectly(int pmPercent)
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows;
        vm.Components[0].Size = ComponentSize.Medium;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 75 hours

        vm.PmEffortPercentage = pmPercent;

        decimal dev = 75m;
        decimal sysTest = MainViewModel.RoundUp(dev * 0.30m);
        decimal analysis = MainViewModel.RoundUp((dev + sysTest) * 0.05m);
        decimal bizDesign = MainViewModel.RoundUp((dev + sysTest) * 0.15m);
        decimal promotion = MainViewModel.RoundUp(dev * 0.05m);
        decimal baSysDoc = MainViewModel.RoundUp(dev * 0.05m);
        decimal prodVal = MainViewModel.RoundUp(sysTest * 0.20m);
        decimal allTasks = dev + sysTest + analysis + bizDesign + promotion + baSysDoc + prodVal;
        decimal expectedPM = MainViewModel.RoundUp(allTasks * (pmPercent / 100m));

        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion

    #region === BOUNDARY: T-Shirt Sizing ===

    [Theory]
    [InlineData(0, "—")]
    [InlineData(1, "Small")]
    [InlineData(99, "Small")]
    [InlineData(100, "Medium")]
    [InlineData(299, "Medium")]
    [InlineData(300, "Large")]
    [InlineData(749, "Large")]
    [InlineData(750, "X-Large")]
    [InlineData(999, "X-Large")]
    [InlineData(1000, "XL1")]
    [InlineData(1999, "XL1")]
    [InlineData(2000, "XL2")]
    [InlineData(2999, "XL2")]
    [InlineData(3000, "XL3")]
    [InlineData(3999, "XL3")]
    [InlineData(4000, "XL4")]
    [InlineData(4999, "XL4")]
    [InlineData(5000, "XL5")]
    [InlineData(5262, "XL5")]
    [InlineData(5999, "XL5")]
    [InlineData(6000, "XL6")]
    [InlineData(6999, "XL6")]
    [InlineData(7000, "XL7")]
    [InlineData(7999, "XL7")]
    [InlineData(8000, "XL8")]
    [InlineData(10000, "XL8")]
    [InlineData(99999, "XL8")]
    public void TShirtSize_AllBoundaries_Correct(decimal grandTotal, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(grandTotal));
    }

    #endregion

    #region === BOUNDARY: RoundUp Function (Excel ROUNDUP(x, 2)) ===

    [Theory]
    [InlineData(0, 0)]           // Zero stays zero
    [InlineData(1.0, 1.0)]       // Already rounded
    [InlineData(1.00, 1.00)]     // Already 2 decimal places
    [InlineData(1.001, 1.01)]    // Rounds up at 3rd decimal
    [InlineData(1.009, 1.01)]    // Any fraction rounds up
    [InlineData(1.999, 2.00)]    // Rounds up to next unit
    [InlineData(0.001, 0.01)]    // Small positive
    [InlineData(99.999, 100.00)] // Large value rounding
    [InlineData(37.5, 37.50)]    // Exact half
    [InlineData(3114.0225, 3114.03)] // Analysis formula intermediate
    public void RoundUp_ExactBehavior_MatchesExcel(decimal input, decimal expected)
    {
        Assert.Equal(expected, MainViewModel.RoundUp(input));
    }

    [Fact]
    public void RoundUp_ExactlyTwoDecimals_NoChange()
    {
        // Values that are already at 2dp should not change
        Assert.Equal(155.71m, MainViewModel.RoundUp(155.71m));
        Assert.Equal(467.11m, MainViewModel.RoundUp(467.11m));
        Assert.Equal(29.83m, MainViewModel.RoundUp(29.83m));
        Assert.Equal(503.50m, MainViewModel.RoundUp(503.50m));
        Assert.Equal(645.18m, MainViewModel.RoundUp(645.18m));
    }

    #endregion

    #region === FORMULA: Analysis depends on (Dev+SysTest) ===

    [Fact]
    public void Analysis_UsesEffectiveDev_AndEffectiveSysTest()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hours

        // Dev = 100, adjust by +20 → effective = 120
        vm.DevelopmentAdjustedHours = 20m;

        // SysTest = ROUNDUP(120 * 0.30, 2) = 36
        Assert.Equal(36m, vm.SystemTestingHours);

        // Adjust SysTest by +4 → effective = 40
        vm.SystemTestingAdjustedHours = 4m;

        // Analysis = ROUNDUP((120 + 40) * 0.05, 2) = ROUNDUP(8, 2) = 8
        Assert.Equal(8m, vm.AnalysisHours);
    }

    #endregion

    #region === FORMULA: PM uses ALL effective task totals ===

    [Fact]
    public void PM_UsesAllEffectiveTotals_IncludingAdjustments()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hours

        // Add adjustments to various tasks
        vm.DevelopmentAdjustedHours = 10m;  // effective dev = 110
        vm.BaSystemDocAdjustedHours = 5m;    // BA Sys Doc adjusted

        decimal effectiveDev = 100m + 10m; // 110
        decimal sysTest = MainViewModel.RoundUp(effectiveDev * 0.30m);
        decimal effectiveSysTest = sysTest; // no sys test adjustment
        decimal analysis = MainViewModel.RoundUp((effectiveDev + effectiveSysTest) * 0.05m);
        decimal bizDesign = MainViewModel.RoundUp((effectiveDev + effectiveSysTest) * 0.15m);
        decimal promotion = MainViewModel.RoundUp(effectiveDev * 0.05m);
        decimal baSysDoc = MainViewModel.RoundUp(effectiveDev * 0.05m);
        decimal effectiveBaSysDoc = baSysDoc + 5m;
        decimal prodVal = MainViewModel.RoundUp(effectiveSysTest * 0.20m);

        decimal allEffective = effectiveDev + effectiveSysTest + analysis + bizDesign + promotion + effectiveBaSysDoc + prodVal;
        decimal expectedPM = MainViewModel.RoundUp(allEffective * 0.15m);

        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion

    #region === FORMULA: Production Validation depends on SysTest ===

    [Fact]
    public void ProductionValidation_DependsOnEffectiveSysTest()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hours

        // SysTest = ROUNDUP(100 * 0.30, 2) = 30
        Assert.Equal(30m, vm.SystemTestingHours);
        // ProdVal = ROUNDUP(30 * 0.20, 2) = 6
        Assert.Equal(6m, vm.ProductionValidationHours);

        // Add adjustment to SysTest → effective = 30 + 10 = 40
        vm.SystemTestingAdjustedHours = 10m;
        // ProdVal should now use effective SysTest = 40
        // ProdVal = ROUNDUP(40 * 0.20, 2) = 8
        Assert.Equal(8m, vm.ProductionValidationHours);
    }

    #endregion

    #region === COLLABORATION: Formula Verification ===

    [Theory]
    [InlineData(10, 60, 5, 30, 75.0)]   // Standard: 10×(60/60+30/60)×5 = 10×1.5×5 = 75
    [InlineData(20, 15, 5, 60, 125.0)]  // Excel WPRs: 20×(15/60+60/60)×5 = 20×1.25×5 = 125
    [InlineData(7, 60, 3, 60, 42.0)]    // Excel Client: 7×(60/60+60/60)×3 = 7×2×3 = 42
    [InlineData(3, 15, 5, 60, 18.75)]   // Excel Internal: 3×(15/60+60/60)×5 = 3×1.25×5 = 18.75
    [InlineData(1, 15, 1, 15, 0.5)]     // Minimum practical: 1×(15/60+15/60)×1 = 0.5
    [InlineData(0, 60, 5, 60, 0)]       // Zero meetings = zero
    [InlineData(10, 0, 5, 0, 0)]        // Zero duration and prep = zero
    [InlineData(10, 60, 0, 60, 0)]      // Zero participants = zero
    public void Collaboration_Formula_AllVariations(int meetings, int duration, int participants, int prep, decimal expected)
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = meetings,
            MeetingDurationMinutes = duration,
            NumberOfParticipants = participants,
            ParticipantPrepTimeMinutes = prep
        };
        Assert.Equal(expected, row.TotalHours);
    }

    [Fact]
    public void Collaboration_MaximumValues_NoOverflow()
    {
        // Maximum dropdown values: 20 meetings, 60 min, 20 participants, 180 min prep
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 20,
            ParticipantPrepTimeMinutes = 180
        };
        // 20 × (60/60 + 180/60) × 20 = 20 × 4 × 20 = 1600
        Assert.Equal(1600m, row.TotalHours);
    }

    #endregion

    #region === WEIGHTED VALUES: All 66 Combinations Verified Against Excel ===

    [Theory]
    // PowerBuilder Windows (Excel row 6)
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 25.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 20.94)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 75.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 60.63)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 125.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 100.00)]
    // Reports (Excel row 7)
    [InlineData(ComponentType.Reports, ComponentSize.Small, ChangeType.New, 17.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Small, ChangeType.Change, 13.60)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 51.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium, ChangeType.Change, 40.80)]
    [InlineData(ComponentType.Reports, ComponentSize.Large, ChangeType.New, 85.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Large, ChangeType.Change, 68.00)]
    // Programs/DB Stored Procs (Excel row 8)
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.New, 46.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.Change, 36.80)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 115.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.Change, 92.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 294.40)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change, 235.525)]
    // Support Modules (Excel row 9)
    [InlineData(ComponentType.SupportModules, ComponentSize.Small, ChangeType.New, 5.00)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 4.0625)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.New, 11.875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 9.6875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 26.875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change, 21.5625)]
    // DB Manipulation (Excel row 10)
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.New, 5.9375)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change, 4.6875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 15.00)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.Change, 11.875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 31.875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 25.625)]
    // Database Review (Excel row 11)
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.Change, 6.10)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.Change, 6.10)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.Change, 6.10)]
    // Webpage (Excel row 12)
    [InlineData(ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Small, ChangeType.Change, 16.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 60.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium, ChangeType.Change, 48.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large, ChangeType.Change, 75.00)]
    // K2 Workflow (Excel row 13)
    [InlineData(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.New, 50.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.Change, 35.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 100.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.Change, 80.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 200.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.Change, 150.00)]
    // K2 Smart Form (Excel row 14)
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New, 15.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 35.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 75.00)]
    // Test Automation UFT (Excel row 15)
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.New, 3.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.Change, 5.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New, 8.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.Change, 1.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.New, 2.50)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.Change, 5.00)]
    // MISC (Excel row 16)
    [InlineData(ComponentType.MISC, ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Medium, ChangeType.Change, 25.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Large, ChangeType.New, 100.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Large, ChangeType.Change, 50.00)]
    public void WeightedValues_AllCombinations_MatchExcelExactly(ComponentType type, ComponentSize size, ChangeType change, decimal expected)
    {
        Assert.Equal(expected, WeightedValues.GetBaseHours(type, size, change));
    }

    #endregion

    #region === TEST CASES: Formula Variations ===

    [Fact]
    public void TestCases_AllZero_SystemTestingZero()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 0;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 1;

        Assert.Equal(0m, vm.SystemTestingHours);
    }

    [Fact]
    public void TestCases_OnlyMedium_CalculatesCorrectly()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 0;
        vm.TestCasesMedium = 10;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 1;

        // mainHours = 10 * 4.065 = 40.65
        // defectHours = (10 * 3.44) * 0.1 = 3.44
        // total = (40.65 + 3.44) * 1 = 44.09
        // RoundUp(44.09, 2) = 44.09
        Assert.Equal(44.09m, vm.SystemTestingHours);
    }

    [Fact]
    public void TestCases_OnlyVeryComplex_CalculatesCorrectly()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 0;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 5;
        vm.TestCaseIterations = 2;

        // mainHours = 5 * 14.38 = 71.9
        // defectHours = (5 * 13.13) * 0.1 = 6.565
        // total = (71.9 + 6.565) * 2 = 78.465 * 2 = 156.93
        // RoundUp(156.93, 2) = 156.93
        Assert.Equal(156.93m, vm.SystemTestingHours);
    }

    [Fact]
    public void TestCases_SwitchingBetweenModes_RecalculatesCorrectly()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hours

        // Default formula: ROUNDUP(100 * 0.30, 2) = 30
        Assert.False(vm.UseTestCasesForEstimate);
        Assert.Equal(30m, vm.SystemTestingHours);

        // Switch to test case mode
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCaseIterations = 1;

        // mainHours = 10 * 2.1925 = 21.925
        // defectHours = (10 * 1.5675) * 0.1 = 1.5675
        // total = (21.925 + 1.5675) * 1 = 23.4925 → RoundUp = 23.50
        Assert.Equal(23.50m, vm.SystemTestingHours);

        // Switch back
        vm.UseTestCasesForEstimate = false;
        Assert.Equal(30m, vm.SystemTestingHours);
    }

    #endregion

    #region === SUBTOTAL & GRAND TOTAL: Component Verification ===

    [Fact]
    public void Subtotal_IncludesTimeForEstimates()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 20 hours

        var subtotalWithout = vm.SubtotalHours;
        vm.TimeForEstimates = 50m;
        var subtotalWith = vm.SubtotalHours;

        // Time for Estimates adds to subtotal
        Assert.True(subtotalWith > subtotalWithout);
    }

    [Fact]
    public void Subtotal_IncludesActualHours()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        var subtotalWithout = vm.SubtotalHours;
        vm.TotalActualHours = 100m;
        var subtotalWith = vm.SubtotalHours;

        Assert.True(subtotalWith > subtotalWithout);
    }

    [Fact]
    public void GrandTotal_IsCeilingOfSubtotal()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.ProgramsDBStoredProcs;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 294.40

        // Grand total should be ceiling of subtotal
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    #endregion

    #region === EDGE: Count Multiplier ===

    [Theory]
    [InlineData(1, 25.625)]
    [InlineData(2, 51.25)]
    [InlineData(10, 256.25)]
    [InlineData(53, 1358.125)]
    [InlineData(100, 2562.5)]
    public void Count_MultipliesBaseHours_Correctly(int count, decimal expectedTotal)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.DBManipulation,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.Change,
            Count = count
        };
        Assert.Equal(expectedTotal, row.TotalHours);
    }

    #endregion

    #region === EDGE: Database Review Same Hours All Sizes ===

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Large, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 6.10)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 6.10)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 6.10)]
    public void DatabaseReview_SameHoursRegardlessOfSize(ComponentSize size, ChangeType change, decimal expected)
    {
        Assert.Equal(expected, WeightedValues.GetBaseHours(ComponentType.DatabaseReview, size, change));
    }

    #endregion

    #region === EDGE: Multiple Collaboration Items of Same Type ===

    [Fact]
    public void MultipleCollaborationItems_SumCorrectly()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Add two WPRs items
        vm.AddCollaborationItemCommand.Execute(null);
        var item1 = vm.CollaborationItems[^1];
        item1.CollabType = CollaborationType.WPRs;
        item1.NumberOfMeetings = 5;
        item1.MeetingDurationMinutes = 60;
        item1.NumberOfParticipants = 3;
        item1.ParticipantPrepTimeMinutes = 0;
        // 5 × (60/60 + 0) × 3 = 5 × 1 × 3 = 15

        vm.AddCollaborationItemCommand.Execute(null);
        var item2 = vm.CollaborationItems[^1];
        item2.CollabType = CollaborationType.WPRs;
        item2.NumberOfMeetings = 3;
        item2.MeetingDurationMinutes = 30;
        item2.NumberOfParticipants = 2;
        item2.ParticipantPrepTimeMinutes = 30;
        // 3 × (30/60 + 30/60) × 2 = 3 × 1 × 2 = 6

        Assert.Equal(21m, vm.WprsHours); // 15 + 6
    }

    #endregion

    #region === REGRESSION: Recalculate on Property Changes ===

    [Fact]
    public void ChangingComponentType_RecalculatesImmediately()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows; // 125
        Assert.Equal(125m, vm.TotalDevelopmentHours);

        vm.Components[0].ComponentType = ComponentType.Reports; // 85
        Assert.Equal(85m, vm.TotalDevelopmentHours);

        vm.Components[0].ComponentType = ComponentType.K2Workflow; // 200
        Assert.Equal(200m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void ChangingSize_RecalculatesImmediately()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.Components[0].Size = ComponentSize.Small; // 25
        Assert.Equal(25m, vm.TotalDevelopmentHours);

        vm.Components[0].Size = ComponentSize.Medium; // 75
        Assert.Equal(75m, vm.TotalDevelopmentHours);

        vm.Components[0].Size = ComponentSize.Large; // 125
        Assert.Equal(125m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void ChangingChangeType_RecalculatesImmediately()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.PowerBuilderWindows;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].Count = 1;

        vm.Components[0].ChangeType = ChangeType.New; // 125
        Assert.Equal(125m, vm.TotalDevelopmentHours);

        vm.Components[0].ChangeType = ChangeType.Change; // 100
        Assert.Equal(100m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void ChangingCount_RecalculatesImmediately()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;

        vm.Components[0].Count = 1;
        Assert.Equal(17m, vm.TotalDevelopmentHours);

        vm.Components[0].Count = 5;
        Assert.Equal(85m, vm.TotalDevelopmentHours);

        vm.Components[0].Count = 10;
        Assert.Equal(170m, vm.TotalDevelopmentHours);
    }

    #endregion

    #region === FORMULA: Cascading Adjustments (Excel I column) ===

    /// <summary>
    /// Verify that adjustments cascade through the formula chain exactly as Excel does.
    /// In Excel: I column = G (calculated) + H (adjusted), and downstream formulas use I values.
    /// </summary>
    [Fact]
    public void CascadingAdjustments_MatchExcelIColumnBehavior()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 2; // 200 hours

        // Add adjustments simulating Excel H column
        vm.DevelopmentAdjustedHours = 50m;       // H27: +50, I27 = 250
        vm.SystemTestingAdjustedHours = 10m;     // H30: +10

        // Recalculated: effective dev = 200 + 50 = 250
        Assert.Equal(250m, vm.DevelopmentTotalHours);

        // SysTest G30 = ROUNDUP(250 * 0.30, 2) = 75
        Assert.Equal(75m, vm.SystemTestingHours);
        // SysTest I30 = 75 + 10 = 85
        Assert.Equal(85m, vm.SystemTestingTotalHours);

        // Analysis G28 = ROUNDUP((250 + 85) * 0.05, 2) = ROUNDUP(16.75, 2) = 16.75
        Assert.Equal(16.75m, vm.AnalysisHours);

        // BizDesign G29 = ROUNDUP((250 + 85) * 0.15, 2) = ROUNDUP(50.25, 2) = 50.25
        Assert.Equal(50.25m, vm.BusinessDesignHours);

        // Promotion G31 = ROUNDUP(250 * 0.05, 2) = ROUNDUP(12.5, 2) = 12.50
        Assert.Equal(12.50m, vm.PromotionHours);

        // BA Sys Doc G32 = ROUNDUP(250 * 0.05, 2) = 12.50
        Assert.Equal(12.50m, vm.BaSystemDocHours);

        // ProdVal G33 = ROUNDUP(85 * 0.20, 2) = 17
        Assert.Equal(17m, vm.ProductionValidationHours);
    }

    #endregion

    #region Helper

    /// <summary>
    /// Creates a MainViewModel populated with the exact Excel CO 23327 002 data,
    /// including BA System Doc adjustment and Time for Estimates.
    /// </summary>
    private MainViewModel CreateExcelScenarioVm()
    {
        var vm = new MainViewModel();

        // Components (Excel rows 7-9)
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].RequirementId = "2.2.1";
        vm.Components[0].ComponentType = ComponentType.DBManipulation;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.Change;
        vm.Components[0].Count = 2;

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].RequirementId = "2.2.1";
        vm.Components[1].ComponentType = ComponentType.DBManipulation;
        vm.Components[1].Size = ComponentSize.Large;
        vm.Components[1].ChangeType = ChangeType.New;
        vm.Components[1].Count = 1;

        vm.AddComponentCommand.Execute(null);
        vm.Components[2].RequirementId = "2.2.1";
        vm.Components[2].ComponentType = ComponentType.SupportModules;
        vm.Components[2].Size = ComponentSize.Medium;
        vm.Components[2].ChangeType = ChangeType.Change;
        vm.Components[2].Count = 53;

        // Test Cases (Excel J30="YES", K30=125, M30=75, O30=2.5)
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2.5m;

        // PM Effort (Excel J34=15)
        vm.PmEffortPercentage = 15m;

        // BA System Doc Adjusted (Excel H32=1.17)
        vm.BaSystemDocAdjustedHours = 1.17m;

        // Time for Estimates (Excel I42=129)
        vm.TimeForEstimates = 129m;

        // Collaboration (Excel rows 36-40)
        var wprs = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.WPRs);
        wprs.NumberOfMeetings = 20;
        wprs.MeetingDurationMinutes = 15;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        var client = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.ClientMeetings);
        client.NumberOfMeetings = 7;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 3;
        client.ParticipantPrepTimeMinutes = 60;

        var internalMtg = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.InternalMeetings);
        internalMtg.NumberOfMeetings = 3;
        internalMtg.MeetingDurationMinutes = 15;
        internalMtg.NumberOfParticipants = 5;
        internalMtg.ParticipantPrepTimeMinutes = 60;

        var auto = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.AutomationTestCollaboration);
        auto.NumberOfMeetings = 0;
        auto.MeetingDurationMinutes = 0;
        auto.NumberOfParticipants = 0;
        auto.ParticipantPrepTimeMinutes = 0;

        return vm;
    }

    #endregion
}

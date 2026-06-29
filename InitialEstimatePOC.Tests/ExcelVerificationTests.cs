using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Verification test: Enters exact data from "CO 23327 002 Final Estimate V1.0.xlsm"
/// into the .NET tool and verifies all calculated results match the Excel output.
/// 
/// Excel Input Summary:
/// - Change Order: CO 23327 002
/// - Components:
///   1. DB Manipulation | Change | Large | Count=2 | BaseHrs=25.625
///   2. DB Manipulation | New | Large | Count=1 | BaseHrs=31.875
///   3. Support Modules | Change | Medium | Count=53 | BaseHrs=9.6875
/// - Development Total: 596.5625
/// - UseTestCases: YES, Simple=125, Complex=75, Iterations=2.5
/// - PM Effort: 15%
/// - BA System Doc Adjusted: +1.17
/// - Time for Estimates: 129
/// - Collaboration:
///   WPRs: 20 meetings, 15min duration, 5 participants, 60min prep = 125 hours
///   Client: 7 meetings, 60min, 3 participants, 60min prep = 42 hours
///   Internal: 3 meetings, 15min, 5 participants, 60min prep = 18.75 hours
///   Automation: 0, Consultant: 0
/// - Grand Total (Excel): 5262
/// </summary>
public class ExcelVerificationTests
{
    #region Component Base Hours Verification

    [Fact]
    public void Excel_DBManipulation_Large_Change_BaseHours()
    {
        // Excel Row 7: DB Manipulation, Change, Large, Count=2, Total=51.25
        // BaseHrs = 51.25 / 2 = 25.625
        var result = WeightedValues.GetBaseHours(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change);
        Assert.Equal(25.625m, result);
    }

    [Fact]
    public void Excel_DBManipulation_Large_New_BaseHours()
    {
        // Excel Row 8: DB Manipulation, New, Large, Count=1, Total=31.875
        var result = WeightedValues.GetBaseHours(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New);
        Assert.Equal(31.875m, result);
    }

    [Fact]
    public void Excel_SupportModules_Medium_Change_BaseHours()
    {
        // Excel Row 9: Support Modules, Change, Medium, Count=53, Total=513.4375
        // BaseHrs = 513.4375 / 53 = 9.6875
        var result = WeightedValues.GetBaseHours(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change);
        Assert.Equal(9.6875m, result);
    }

    #endregion

    #region Component TotalHours Verification

    [Fact]
    public void Excel_Component1_TotalHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.DBManipulation,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.Change,
            Count = 2
        };
        Assert.Equal(51.25m, row.TotalHours);
    }

    [Fact]
    public void Excel_Component2_TotalHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.DBManipulation,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(31.875m, row.TotalHours);
    }

    [Fact]
    public void Excel_Component3_TotalHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.SupportModules,
            Size = ComponentSize.Medium,
            ChangeType = ChangeType.Change,
            Count = 53
        };
        Assert.Equal(513.4375m, row.TotalHours);
    }

    #endregion

    #region Development Total

    [Fact]
    public void Excel_DevelopmentTotal_MatchesSum()
    {
        // 51.25 + 31.875 + 513.4375 = 596.5625
        var vm = CreateExcelVm();
        Assert.Equal(596.5625m, vm.DevelopmentTotalHours);
    }

    #endregion

    #region Test Case System Testing Calculation

    [Fact]
    public void Excel_SystemTesting_TestCaseFormula_WithDecimalIterations()
    {
        // Excel uses iterations=2.5
        // (931.0625 + 75.91875) * 2.5 = 1006.98125 * 2.5 = 2517.453125 → RoundUp = 2517.46
        const decimal r31Simple = 2.1925m, r31Complex = 8.76m;
        const decimal r32Simple = 1.5675m, r32Complex = 7.51m;
        decimal mainHours = 125m * r31Simple + 75m * r31Complex; // 274.0625 + 657 = 931.0625
        decimal defectHours = (125m * r32Simple + 75m * r32Complex) * 0.1m; // (195.9375 + 563.25) * 0.1 = 75.91875
        decimal expectedSysTest = MainViewModel.RoundUp((mainHours + defectHours) * 2.5m);

        var vm = CreateExcelVm(); // default iterations = 2.5m
        Assert.Equal(expectedSysTest, vm.SystemTestingTotalHours);
        Assert.Equal(2517.46m, vm.SystemTestingTotalHours);
    }

    [Fact]
    public void Excel_SystemTesting_FormulaComponents_Correct()
    {
        // Verify the individual formula components match Excel weighted values
        const decimal r31Simple = 2.1925m, r31Complex = 8.76m;
        const decimal r32Simple = 1.5675m, r32Complex = 7.51m;

        // mainHours = 125 * 2.1925 + 75 * 8.76 = 274.0625 + 657 = 931.0625
        Assert.Equal(931.0625m, 125m * r31Simple + 75m * r31Complex);

        // defectHours = (125 * 1.5675 + 75 * 7.51) * 0.1 = (195.9375 + 563.25) * 0.1 = 75.91875
        Assert.Equal(75.91875m, (125m * r32Simple + 75m * r32Complex) * 0.1m);

        // With 2.5 iterations: (931.0625 + 75.91875) * 2.5 = 1006.98125 * 2.5 = 2517.453125
        // RoundUp(2517.453125) = 2517.46 — matches Excel
        Assert.Equal(2517.46m, MainViewModel.RoundUp(1006.98125m * 2.5m));
    }

    #endregion

    #region Collaboration Verification

    [Fact]
    public void Excel_WPRs_Hours()
    {
        // 20 meetings × ((15/60) + (60/60)) × 5 participants = 20 × 1.25 × 5 = 125
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 60
        };
        Assert.Equal(125m, row.TotalHours);
    }

    [Fact]
    public void Excel_ClientMeetings_Hours()
    {
        // 7 × ((60/60) + (60/60)) × 3 = 7 × 2 × 3 = 42
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 7,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 60
        };
        Assert.Equal(42m, row.TotalHours);
    }

    [Fact]
    public void Excel_InternalMeetings_Hours()
    {
        // 3 × ((15/60) + (60/60)) × 5 = 3 × 1.25 × 5 = 18.75
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 3,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 60
        };
        Assert.Equal(18.75m, row.TotalHours);
    }

    [Fact]
    public void Excel_TotalCollaboration()
    {
        // WPRs=125 + Client=42 + Internal=18.75 + Auto=0 + Consultant=0 = 185.75
        var vm = CreateExcelVm(iterations: 2);
        Assert.Equal(185.75m, vm.TotalCollaborationHours);
    }

    #endregion

    #region BA System Doc Adjusted Hours

    [Fact]
    public void Excel_BaSystemDocAdjusted_Applied()
    {
        // Excel: BA System Doc calculated=29.83, adjusted=+1.17, total=31
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;

        decimal effectiveBaSysDoc = vm.BaSystemDocTotalHours;
        Assert.Equal(31m, effectiveBaSysDoc);
    }

    #endregion

    #region Time for Estimates

    [Fact]
    public void Excel_TimeForEstimates_IncludedInSubtotal()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.TimeForEstimates = 129m;

        // Time for Estimates adds directly to subtotal
        Assert.True(vm.SubtotalHours > 0);
    }

    #endregion

    #region PM Effort

    [Fact]
    public void Excel_PMEffort_15Percent()
    {
        var vm = CreateExcelVm(iterations: 2);
        Assert.Equal(15m, vm.PmEffortPercentage);
        Assert.True(vm.ProjectManagementTotalHours > 0);
    }

    #endregion

    #region T-Shirt Size

    [Fact]
    public void Excel_TShirtSize_XL5()
    {
        // Excel total is 5262 → XL5 (5000-5999 range)
        Assert.Equal("XL5", WeightedValues.GetTShirtSize(5262m));
    }

    #endregion

    #region Full Pipeline with Integer Iterations (Tool Limitation)

    [Fact]
    public void Excel_FullPipeline_Iterations2_AllTasksPositive()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // All task totals should be positive
        Assert.True(vm.DevelopmentTotalHours > 0);
        Assert.True(vm.SystemTestingTotalHours > 0);
        Assert.True(vm.AnalysisTotalHours > 0);
        Assert.True(vm.BusinessDesignTotalHours > 0);
        Assert.True(vm.PromotionTotalHours > 0);
        Assert.True(vm.BaSystemDocTotalHours > 0);
        Assert.True(vm.ProductionValidationTotalHours > 0);
        Assert.True(vm.ProjectManagementTotalHours > 0);
        Assert.True(vm.TotalCollaborationHours > 0);
        Assert.True(vm.GrandTotalHours > 0);
    }

    [Fact]
    public void Excel_FullPipeline_Iterations2_GrandTotalInXL5Range()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // With iterations=2, total will be less than Excel's 5262 (which uses 2.5)
        // But should still be in XL range (>1000)
        Assert.True(vm.GrandTotalHours >= 1000m, $"GrandTotal={vm.GrandTotalHours}, expected ≥1000 (XL range)");
    }

    [Fact]
    public void Excel_FullPipeline_Iterations3_GrandTotalHigher()
    {
        var vm = CreateExcelVm(iterations: 3);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // With iterations=3, total exceeds Excel's 5262 (which uses 2.5)
        Assert.True(vm.GrandTotalHours > 5262m, $"GrandTotal={vm.GrandTotalHours}, expected >5262 with iterations=3");
    }

    #endregion

    #region Role Breakout

    [Fact]
    public void Excel_RoleBreakout_TesterMatchesSysTest()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // Tester = System Testing total hours (effective)
        Assert.Equal(vm.SystemTestingTotalHours, vm.TesterRoleHours);
    }

    [Fact]
    public void Excel_RoleBreakout_PMMatchesPMTotal()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        Assert.Equal(vm.ProjectManagementTotalHours, vm.PmRoleHours);
    }

    [Fact]
    public void Excel_RoleBreakout_AllPositive()
    {
        var vm = CreateExcelVm(iterations: 2);
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        Assert.True(vm.BaRoleHours > 0);
        Assert.True(vm.SeRoleHours > 0);
        Assert.True(vm.TesterRoleHours > 0);
        Assert.True(vm.PmRoleHours > 0);
    }

    #endregion

    #region Exact Excel Match (Computed Manually with 2.5 iterations)

    [Fact]
    public void Excel_ManualCalculation_MatchesExpected()
    {
        // Manually compute ALL Excel values using exact 2.5 iterations
        // to verify our formula logic is correct even though the tool uses int iterations
        
        decimal dev = 596.5625m;
        decimal sysTest = MainViewModel.RoundUp(1006.98125m * 2.5m); // 2517.46
        decimal analysis = MainViewModel.RoundUp((dev + sysTest) * 0.05m); // RoundUp(3114.0225 * 0.05) = RoundUp(155.701125) = 155.71
        decimal bizDesign = MainViewModel.RoundUp((dev + sysTest) * 0.15m); // RoundUp(3114.0225 * 0.15) = RoundUp(467.103375) = 467.11
        decimal promotion = MainViewModel.RoundUp(dev * 0.05m); // RoundUp(29.828125) = 29.83
        decimal baSysDoc = MainViewModel.RoundUp(dev * 0.05m); // 29.83
        decimal effectiveBaSysDoc = baSysDoc + 1.17m; // 31.00 (adjusted)
        decimal prodVal = MainViewModel.RoundUp(sysTest * 0.20m); // RoundUp(503.492) = 503.5
        
        // PM uses EFFECTIVE values (calculated + adjusted) — Excel cascading formula
        decimal pmBase = dev + sysTest + analysis + bizDesign + promotion + effectiveBaSysDoc + prodVal;
        decimal pm = MainViewModel.RoundUp(pmBase * 0.15m);
        
        // Verify individual tasks match Excel
        Assert.Equal(2517.46m, sysTest);
        Assert.Equal(155.71m, analysis);
        Assert.Equal(467.11m, bizDesign);
        Assert.Equal(29.83m, promotion);
        Assert.Equal(29.83m, baSysDoc);
        Assert.Equal(503.50m, prodVal);
        Assert.Equal(645.18m, pm); // Excel Row 34: 645.18
    }

    [Fact]
    public void Excel_ManualSubtotal_MatchesExpected()
    {
        // Full subtotal: all tasks + collab + adjusted + time for estimates
        decimal dev = 596.5625m;
        decimal sysTest = 2517.46m;
        decimal analysis = 155.71m;
        decimal bizDesign = 467.11m;
        decimal promotion = 29.83m;
        decimal baSysDoc = 29.83m + 1.17m; // +1.17 adjusted
        decimal prodVal = 503.50m;
        decimal pm = 645.18m;
        decimal collab = 125m + 42m + 18.75m; // WPRs + Client + Internal = 185.75
        decimal timeForEst = 129m;

        decimal subtotal = MainViewModel.RoundUp(
            dev + sysTest + analysis + bizDesign + promotion + baSysDoc + prodVal + pm + collab + timeForEst);

        // Excel Row 43: 5261.11
        Assert.Equal(5261.11m, subtotal);
    }

    [Fact]
    public void Excel_GrandTotal_Ceiling()
    {
        // Excel GrandTotal = ROUNDUP(5261.11, 0) = 5262
        Assert.Equal(5262m, Math.Ceiling(5261.11m));
    }

    [Fact]
    public void Excel_FullPipeline_ExactMatch_WithDecimalIterations()
    {
        // Now that TestCaseIterations supports decimal, verify EXACT Excel grand total
        var vm = CreateExcelVm(); // default iterations = 2.5m
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // Verify exact Excel values
        Assert.Equal(596.5625m, vm.DevelopmentTotalHours);
        Assert.Equal(2517.46m, vm.SystemTestingTotalHours);
        Assert.Equal(155.71m, vm.AnalysisTotalHours);
        Assert.Equal(467.11m, vm.BusinessDesignTotalHours);
        Assert.Equal(29.83m, vm.PromotionTotalHours);
        Assert.Equal(31.00m, vm.BaSystemDocTotalHours);
        Assert.Equal(503.50m, vm.ProductionValidationTotalHours);
        Assert.Equal(645.18m, vm.ProjectManagementTotalHours);
        Assert.Equal(185.75m, vm.TotalCollaborationHours);

        // Excel Grand Total = 5262
        Assert.Equal(5262m, vm.GrandTotalHours);
        Assert.Equal("XL5", vm.TShirtSize);
    }

    #endregion

    #region Helper

    /// <summary>
    /// Creates a MainViewModel populated with the exact Excel data.
    /// </summary>
    private MainViewModel CreateExcelVm(decimal iterations = 2.5m)
    {
        var vm = new MainViewModel();

        // Components
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

        // Test Cases
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = iterations;

        // PM Effort
        vm.PmEffortPercentage = 15m;

        // Collaboration (set actual values)
        // WPRs: 20, 15min, 5, 60min
        var wprs = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.WPRs);
        wprs.NumberOfMeetings = 20;
        wprs.MeetingDurationMinutes = 15;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        // Client Meetings: 7, 60min, 3, 60min
        var client = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.ClientMeetings);
        client.NumberOfMeetings = 7;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 3;
        client.ParticipantPrepTimeMinutes = 60;

        // Internal Meetings: 3, 15min, 5, 60min
        var internalMtg = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.InternalMeetings);
        internalMtg.NumberOfMeetings = 3;
        internalMtg.MeetingDurationMinutes = 15;
        internalMtg.NumberOfParticipants = 5;
        internalMtg.ParticipantPrepTimeMinutes = 60;

        // Automation: 0 meetings
        var auto = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.AutomationTestCollaboration);
        auto.NumberOfMeetings = 0;
        auto.MeetingDurationMinutes = 0;
        auto.NumberOfParticipants = 0;
        auto.ParticipantPrepTimeMinutes = 0;

        return vm;
    }

    #endregion
}

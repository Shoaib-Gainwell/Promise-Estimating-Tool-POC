using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for per-task adjusted hours, PM reserve percentage, subtotal calculations,
/// and per-task total (Calculated + Adjusted) columns.
/// </summary>
public class AdjustedHoursTests
{
    private MainViewModel CreateVm() => new();

    private void AddMiscLarge(MainViewModel vm)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;
    }

    #region Development Adjusted Hours

    [Fact]
    public void DevelopmentAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.DevelopmentAdjustedHours);
    }

    [Fact]
    public void DevelopmentAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        vm.DevelopmentAdjustedHours = 20m;
        Assert.Equal(120m, vm.DevelopmentTotalHours); // 100 + 20
    }

    [Fact]
    public void DevelopmentAdjusted_Negative_DecreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        vm.DevelopmentAdjustedHours = -10m;
        Assert.Equal(90m, vm.DevelopmentTotalHours); // 100 - 10
    }

    [Fact]
    public void DevelopmentAdjusted_AffectsSubtotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal subtotalBefore = vm.SubtotalHours;
        vm.DevelopmentAdjustedHours = 50m;
        // Development adjustment cascades into derived tasks (SysTest, Analysis, etc.)
        Assert.True(vm.SubtotalHours > subtotalBefore);
        Assert.True(vm.SubtotalHours > subtotalBefore + 50m); // More than linear due to cascading
    }

    #endregion

    #region Analysis Adjusted Hours

    [Fact]
    public void AnalysisAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.AnalysisAdjustedHours);
    }

    [Fact]
    public void AnalysisAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.AnalysisHours; // 6.50
        vm.AnalysisAdjustedHours = 5m;
        Assert.Equal(calc + 5m, vm.AnalysisTotalHours);
    }

    [Fact]
    public void AnalysisAdjusted_AffectsSubtotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal before = vm.SubtotalHours;
        vm.AnalysisAdjustedHours = 10m;
        // Analysis adjustment cascades into PM calculation
        Assert.True(vm.SubtotalHours > before);
        Assert.True(vm.SubtotalHours > before + 10m);
    }

    #endregion

    #region Business Design Adjusted Hours

    [Fact]
    public void BusinessDesignAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.BusinessDesignAdjustedHours);
    }

    [Fact]
    public void BusinessDesignAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.BusinessDesignHours; // 19.50
        vm.BusinessDesignAdjustedHours = 10m;
        Assert.Equal(calc + 10m, vm.BusinessDesignTotalHours);
    }

    #endregion

    #region System Testing Adjusted Hours

    [Fact]
    public void SystemTestingAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.SystemTestingAdjustedHours);
    }

    [Fact]
    public void SystemTestingAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.SystemTestingHours; // 30
        vm.SystemTestingAdjustedHours = 15m;
        Assert.Equal(calc + 15m, vm.SystemTestingTotalHours);
    }

    #endregion

    #region Promotion Adjusted Hours

    [Fact]
    public void PromotionAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.PromotionAdjustedHours);
    }

    [Fact]
    public void PromotionAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.PromotionHours; // 5
        vm.PromotionAdjustedHours = 3m;
        Assert.Equal(calc + 3m, vm.PromotionTotalHours);
    }

    #endregion

    #region BA System Doc Adjusted Hours

    [Fact]
    public void BaSystemDocAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.BaSystemDocAdjustedHours);
    }

    [Fact]
    public void BaSystemDocAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.BaSystemDocHours; // 5
        vm.BaSystemDocAdjustedHours = 7m;
        Assert.Equal(calc + 7m, vm.BaSystemDocTotalHours);
    }

    #endregion

    #region Production Validation Adjusted Hours

    [Fact]
    public void ProductionValidationAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.ProductionValidationAdjustedHours);
    }

    [Fact]
    public void ProductionValidationAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.ProductionValidationHours; // 6
        vm.ProductionValidationAdjustedHours = 4m;
        Assert.Equal(calc + 4m, vm.ProductionValidationTotalHours);
    }

    #endregion

    #region Project Management Adjusted Hours

    [Fact]
    public void ProjectManagementAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.ProjectManagementAdjustedHours);
    }

    [Fact]
    public void ProjectManagementAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal calc = vm.ProjectManagementHours; // 25.80
        vm.ProjectManagementAdjustedHours = 5m;
        Assert.Equal(calc + 5m, vm.ProjectManagementTotalHours);
    }

    #endregion

    #region Collaboration Adjusted Hours

    [Fact]
    public void CollaborationAdjusted_DefaultIsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.CollaborationAdjustedHours);
    }

    [Fact]
    public void CollaborationAdjusted_Positive_IncreasesTotal()
    {
        var vm = CreateVm();
        decimal calc = vm.TotalCollaborationHours; // 93.75
        vm.CollaborationAdjustedHours = 10m;
        Assert.Equal(calc + 10m, vm.CollaborationTotalHours);
    }

    [Fact]
    public void CollaborationAdjusted_AffectsSubtotal()
    {
        var vm = CreateVm();
        decimal before = vm.SubtotalHours;
        vm.CollaborationAdjustedHours = 20m;
        Assert.Equal(before + 20m, vm.SubtotalHours);
    }

    #endregion

    #region Multiple Adjusted Hours Combined

    [Fact]
    public void MultipleAdjustedHours_AllAddToSubtotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);

        // Set non-dev adjustments first (these add linearly)
        vm.AnalysisAdjustedHours = 5m;
        vm.BusinessDesignAdjustedHours = 3m;
        vm.SystemTestingAdjustedHours = 7m;
        vm.PromotionAdjustedHours = 2m;
        vm.BaSystemDocAdjustedHours = 1m;
        vm.ProductionValidationAdjustedHours = 4m;
        vm.ProjectManagementAdjustedHours = 6m;
        vm.CollaborationAdjustedHours = 8m;

        decimal afterNonDev = vm.SubtotalHours;

        // Development adjustment cascades into derived tasks
        vm.DevelopmentAdjustedHours = 10m;
        Assert.True(vm.SubtotalHours > afterNonDev);
        Assert.True(vm.SubtotalHours > afterNonDev + 10m); // Cascading effect
    }

    [Fact]
    public void AllAdjustedHours_Negative_ReduceSubtotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal baseline = vm.SubtotalHours;

        vm.DevelopmentAdjustedHours = -5m;
        vm.SystemTestingAdjustedHours = -3m;

        // Dev adjustment cascades (reduces derived tasks too), so reduction > raw 8
        Assert.True(vm.SubtotalHours < baseline);
        Assert.True(vm.SubtotalHours < baseline - 8m);
    }

    #endregion

    #region PM Reserve Percentage

    [Fact]
    public void PmReservePercentage_Default5Percent()
    {
        var vm = CreateVm();
        Assert.Equal(5m, vm.PmReservePercentage);
    }

    [Fact]
    public void PmReservePercentage_ChangeRecalculates()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal reserve5 = vm.PmReserveHours;

        vm.PmReservePercentage = 10m;
        decimal reserve10 = vm.PmReserveHours;

        Assert.True(reserve10 > reserve5);
        // Approximately double
        Assert.InRange(reserve10, reserve5 * 1.9m, reserve5 * 2.1m);
    }

    [Fact]
    public void PmReservePercentage_Zero_NoReserve()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        vm.PmReservePercentage = 0m;
        Assert.Equal(0m, vm.PmReserveHours);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    [Fact]
    public void PmReserve_ROUNDUP_Applied()
    {
        var vm = CreateVm();
        // With no components, PM Reserve = 0 (summary hidden)
        Assert.Equal(0m, vm.PmReserveHours);
        // Add a component so reserve is calculated
        AddMiscLarge(vm);
        // Now subtotal includes dev + derived + collab, reserve should use ROUNDUP
        Assert.True(vm.PmReserveHours > 0m);
    }

    #endregion

    #region Grand Total = Subtotal + PM Reserve

    [Fact]
    public void GrandTotal_AlwaysEqualsSubtotalPlusReserve()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        vm.DevelopmentAdjustedHours = 25m;
        vm.PmReservePercentage = 7m;
        vm.TimeForEstimates = 15m;
        vm.TotalActualHours = 10m;

        Assert.Equal(Math.Ceiling(vm.SubtotalHours + vm.PmReserveHours), vm.GrandTotalHours);
    }

    #endregion

    #region Assumptions Fields

    [Fact]
    public void SeAssumptions_DefaultEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.SeAssumptions);
    }

    [Fact]
    public void SeAssumptions_CanBeSet()
    {
        var vm = CreateVm();
        vm.SeAssumptions = "All modules require refactoring.";
        Assert.Equal("All modules require refactoring.", vm.SeAssumptions);
    }

    [Fact]
    public void BaAssumptions_DefaultEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.BaAssumptions);
    }

    [Fact]
    public void BaAssumptions_CanBeSet()
    {
        var vm = CreateVm();
        vm.BaAssumptions = "BRD is finalized.";
        Assert.Equal("BRD is finalized.", vm.BaAssumptions);
    }

    [Fact]
    public void CollaborationAssumptions_DefaultEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.CollaborationAssumptions);
    }

    [Fact]
    public void CollaborationAssumptions_CanBeSet()
    {
        var vm = CreateVm();
        vm.CollaborationAssumptions = "Weekly standups only.";
        Assert.Equal("Weekly standups only.", vm.CollaborationAssumptions);
    }

    [Fact]
    public void GeneralAssumptions_DefaultEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.GeneralAssumptions);
    }

    [Fact]
    public void GeneralAssumptions_CanBeSet()
    {
        var vm = CreateVm();
        vm.GeneralAssumptions = "No scope changes expected.";
        Assert.Equal("No scope changes expected.", vm.GeneralAssumptions);
    }

    [Fact]
    public void Assumptions_DoNotAffectCalculations()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal grand = vm.GrandTotalHours;

        vm.SeAssumptions = "something";
        vm.BaAssumptions = "something else";
        vm.CollaborationAssumptions = "more text";
        vm.GeneralAssumptions = "notes";

        Assert.Equal(grand, vm.GrandTotalHours);
    }

    #endregion

    #region Cascading Adjustments

    [Fact]
    public void SystemTestingAdjusted_CascadesIntoAnalysis()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100, SysTest=30
        decimal analysisBefore = vm.AnalysisHours;

        vm.SystemTestingAdjustedHours = 10m;
        // Analysis = ROUNDUP((effectiveDev + effectiveSysTest) * 5%)
        // effectiveSysTest = 30 + 10 = 40, so Analysis = ROUNDUP((100+40)*0.05) = ROUNDUP(7) = 7
        Assert.True(vm.AnalysisHours > analysisBefore);
    }

    [Fact]
    public void SystemTestingAdjusted_CascadesIntoBusinessDesign()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal bizDesignBefore = vm.BusinessDesignHours;

        vm.SystemTestingAdjustedHours = 10m;
        // BusinessDesign = ROUNDUP((effectiveDev + effectiveSysTest) * 15%)
        Assert.True(vm.BusinessDesignHours > bizDesignBefore);
    }

    [Fact]
    public void SystemTestingAdjusted_CascadesIntoProductionValidation()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal prodValBefore = vm.ProductionValidationHours;

        vm.SystemTestingAdjustedHours = 10m;
        // ProdVal = ROUNDUP(effectiveSysTest * 20%)
        Assert.True(vm.ProductionValidationHours > prodValBefore);
    }

    [Fact]
    public void SystemTestingAdjusted_CascadesIntoPM()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal pmBefore = vm.ProjectManagementHours;

        vm.SystemTestingAdjustedHours = 10m;
        Assert.True(vm.ProjectManagementHours > pmBefore);
    }

    [Fact]
    public void AnalysisAdjusted_CascadesIntoPM()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal pmBefore = vm.ProjectManagementHours;

        vm.AnalysisAdjustedHours = 10m;
        // PM uses effectiveAnalysis which now includes +10
        Assert.True(vm.ProjectManagementHours > pmBefore);
    }

    [Fact]
    public void BusinessDesignAdjusted_CascadesIntoPM()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal pmBefore = vm.ProjectManagementHours;

        vm.BusinessDesignAdjustedHours = 10m;
        Assert.True(vm.ProjectManagementHours > pmBefore);
    }

    [Fact]
    public void PromotionAdjusted_CascadesIntoPM()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal pmBefore = vm.ProjectManagementHours;

        vm.PromotionAdjustedHours = 10m;
        Assert.True(vm.ProjectManagementHours > pmBefore);
    }

    [Fact]
    public void DevelopmentAdjusted_CascadesThroughAllDerived()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100

        decimal sysTestBefore = vm.SystemTestingHours;  // 30
        decimal analysisBefore = vm.AnalysisHours;
        decimal bizDesignBefore = vm.BusinessDesignHours;
        decimal promotionBefore = vm.PromotionHours;
        decimal baSysDocBefore = vm.BaSystemDocHours;
        decimal prodValBefore = vm.ProductionValidationHours;
        decimal pmBefore = vm.ProjectManagementHours;

        vm.DevelopmentAdjustedHours = 50m; // effectiveDev = 150

        Assert.True(vm.SystemTestingHours > sysTestBefore);
        Assert.True(vm.AnalysisHours > analysisBefore);
        Assert.True(vm.BusinessDesignHours > bizDesignBefore);
        Assert.True(vm.PromotionHours > promotionBefore);
        Assert.True(vm.BaSystemDocHours > baSysDocBefore);
        Assert.True(vm.ProductionValidationHours > prodValBefore);
        Assert.True(vm.ProjectManagementHours > pmBefore);
    }

    [Fact]
    public void CascadingAdjustment_ExactValues_DevPlus25()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100, effectiveDev=100

        vm.DevelopmentAdjustedHours = 25m; // effectiveDev = 125
        // SysTest = ROUNDUP(125*0.30) = ROUNDUP(37.5) = 37.50
        Assert.Equal(37.50m, vm.SystemTestingHours);
        // Analysis = ROUNDUP((125+37.50)*0.05) = ROUNDUP(162.50*0.05) = ROUNDUP(8.125) = 8.13
        Assert.Equal(8.13m, vm.AnalysisHours);
        // BusinessDesign = ROUNDUP((125+37.50)*0.15) = ROUNDUP(24.375) = 24.38
        Assert.Equal(24.38m, vm.BusinessDesignHours);
        // Promotion = ROUNDUP(125*0.05) = ROUNDUP(6.25) = 6.25
        Assert.Equal(6.25m, vm.PromotionHours);
        // BaSysDoc = ROUNDUP(125*0.05) = 6.25
        Assert.Equal(6.25m, vm.BaSystemDocHours);
        // ProdVal = ROUNDUP(37.50*0.20) = ROUNDUP(7.5) = 7.50
        Assert.Equal(7.50m, vm.ProductionValidationHours);
    }

    [Fact]
    public void CascadingAdjustment_SysTestAdj_AffectsDownstream()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100, SysTest=30

        vm.SystemTestingAdjustedHours = 20m; // effectiveSysTest = 30+20 = 50
        // Analysis = ROUNDUP((100+50)*0.05) = ROUNDUP(7.5) = 7.50
        Assert.Equal(7.50m, vm.AnalysisHours);
        // BusinessDesign = ROUNDUP((100+50)*0.15) = ROUNDUP(22.5) = 22.50
        Assert.Equal(22.50m, vm.BusinessDesignHours);
        // ProdVal = ROUNDUP(50*0.20) = ROUNDUP(10) = 10
        Assert.Equal(10m, vm.ProductionValidationHours);
    }

    [Fact]
    public void CascadingAdjustment_NegativeDevAdj_ReducesDerived()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100

        vm.DevelopmentAdjustedHours = -20m; // effectiveDev = 80
        // SysTest = ROUNDUP(80*0.30) = 24
        Assert.Equal(24m, vm.SystemTestingHours);
        // Promotion = ROUNDUP(80*0.05) = 4
        Assert.Equal(4m, vm.PromotionHours);
    }

    [Fact]
    public void CascadingAdjustment_MultipleAdjustments_CompoundCorrectly()
    {
        var vm = CreateVm();
        AddMiscLarge(vm); // dev=100

        vm.DevelopmentAdjustedHours = 25m; // effectiveDev=125, SysTest=37.50
        vm.SystemTestingAdjustedHours = 12.50m; // effectiveSysTest = 37.50+12.50 = 50

        // Analysis = ROUNDUP((125+50)*0.05) = ROUNDUP(175*0.05) = ROUNDUP(8.75) = 8.75
        Assert.Equal(8.75m, vm.AnalysisHours);
        // BusinessDesign = ROUNDUP(175*0.15) = ROUNDUP(26.25) = 26.25
        Assert.Equal(26.25m, vm.BusinessDesignHours);
        // ProdVal = ROUNDUP(50*0.20) = 10
        Assert.Equal(10m, vm.ProductionValidationHours);
    }

    #endregion

    #region GrandTotal Ceiling (Round Up to Whole Number)

    [Fact]
    public void GrandTotal_IsAlwaysWholeNumber()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        Assert.Equal(Math.Ceiling(vm.GrandTotalHours), vm.GrandTotalHours);
        Assert.Equal(0m, vm.GrandTotalHours % 1m);
    }

    [Fact]
    public void GrandTotal_RoundsUpFractional()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        // SubtotalHours + PmReserveHours is typically not a whole number
        decimal raw = vm.SubtotalHours + vm.PmReserveHours;
        if (raw != Math.Floor(raw))
        {
            Assert.True(vm.GrandTotalHours > raw);
            Assert.Equal(Math.Ceiling(raw), vm.GrandTotalHours);
        }
    }

    [Fact]
    public void GrandTotal_WholeNumberInput_StaysTheSame()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        // Force subtotal to produce a whole number by adjusting
        // If SubtotalHours + Reserve is already whole, GrandTotal = that value
        decimal grand = vm.GrandTotalHours;
        Assert.Equal(grand, Math.Ceiling(grand));
    }

    [Fact]
    public void GrandTotal_WithVariousAdjustments_AlwaysCeiling()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);

        vm.DevelopmentAdjustedHours = 13m;
        Assert.Equal(Math.Ceiling(vm.SubtotalHours + vm.PmReserveHours), vm.GrandTotalHours);

        vm.SystemTestingAdjustedHours = 7m;
        Assert.Equal(Math.Ceiling(vm.SubtotalHours + vm.PmReserveHours), vm.GrandTotalHours);

        vm.PmReservePercentage = 8m;
        Assert.Equal(Math.Ceiling(vm.SubtotalHours + vm.PmReserveHours), vm.GrandTotalHours);
    }

    #endregion
}

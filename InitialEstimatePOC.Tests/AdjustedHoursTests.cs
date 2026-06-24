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
        Assert.Equal(subtotalBefore + 50m, vm.SubtotalHours);
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
        Assert.Equal(before + 10m, vm.SubtotalHours);
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
        decimal baseline = vm.SubtotalHours;

        vm.DevelopmentAdjustedHours = 10m;
        vm.AnalysisAdjustedHours = 5m;
        vm.BusinessDesignAdjustedHours = 3m;
        vm.SystemTestingAdjustedHours = 7m;
        vm.PromotionAdjustedHours = 2m;
        vm.BaSystemDocAdjustedHours = 1m;
        vm.ProductionValidationAdjustedHours = 4m;
        vm.ProjectManagementAdjustedHours = 6m;
        vm.CollaborationAdjustedHours = 8m;

        decimal totalAdj = 10m + 5m + 3m + 7m + 2m + 1m + 4m + 6m + 8m; // 46
        Assert.Equal(baseline + totalAdj, vm.SubtotalHours);
    }

    [Fact]
    public void AllAdjustedHours_Negative_ReduceSubtotal()
    {
        var vm = CreateVm();
        AddMiscLarge(vm);
        decimal baseline = vm.SubtotalHours;

        vm.DevelopmentAdjustedHours = -5m;
        vm.SystemTestingAdjustedHours = -3m;

        Assert.Equal(baseline - 8m, vm.SubtotalHours);
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
        Assert.Equal(vm.SubtotalHours, vm.GrandTotalHours);
    }

    [Fact]
    public void PmReserve_ROUNDUP_Applied()
    {
        var vm = CreateVm();
        // Create scenario where reserve produces 3+ decimals
        // Collab default = 93.75, SubtotalHours = 93.75
        // Reserve = ROUNDUP(93.75 * 0.05, 2) = ROUNDUP(4.6875, 2) = 4.69
        Assert.Equal(4.69m, vm.PmReserveHours);
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

        Assert.Equal(vm.SubtotalHours + vm.PmReserveHours, vm.GrandTotalHours);
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
}

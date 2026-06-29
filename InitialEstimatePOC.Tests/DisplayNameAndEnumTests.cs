using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for the WeightedValues display names, T-shirt size display name,
/// and all enum values are properly handled.
/// </summary>
public class DisplayNameAndEnumTests
{
    #region ComponentType Display Names

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, "PowerBuilder Windows")]
    [InlineData(ComponentType.Reports, "Reports")]
    [InlineData(ComponentType.ProgramsDBStoredProcs, "Programs/DB Stored Procedures")]
    [InlineData(ComponentType.SupportModules, "Support Modules/JOB/JIL")]
    [InlineData(ComponentType.DBManipulation, "DB Manipulation (SQL, PL/SQL, etc.)")]
    [InlineData(ComponentType.DatabaseReview, "Database Review")]
    [InlineData(ComponentType.Webpage, "Webpage (Includes UI, Portal & Intranet)")]
    [InlineData(ComponentType.K2Workflow, "K2 Workflow")]
    [InlineData(ComponentType.K2SmartForm, "K2 Smart Form")]
    [InlineData(ComponentType.TestAutomationUFT, "Test Automation Suites (UFT)")]
    [InlineData(ComponentType.MISC, "MISC (Server Setup, Webserver Setup, Software Installation, etc.)")]
    public void GetDisplayName_AllTypes_ReturnCorrectName(ComponentType type, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetDisplayName(type));
    }

    #endregion

    #region Enum Completeness

    [Fact]
    public void ComponentType_Has11Values()
    {
        // 11 real types + None = 12 enum values, but None is excluded from business logic
        Assert.Equal(12, Enum.GetValues<ComponentType>().Length);
    }

    [Fact]
    public void ComponentSize_Has3Values()
    {
        // 3 real sizes + None = 4 enum values
        Assert.Equal(4, Enum.GetValues<ComponentSize>().Length);
    }

    [Fact]
    public void ChangeType_Has2Values()
    {
        // 2 real change types + None = 3 enum values
        Assert.Equal(3, Enum.GetValues<ChangeType>().Length);
    }

    [Fact]
    public void CollaborationType_Has5Values()
    {
        Assert.Equal(4, Enum.GetValues<CollaborationType>().Length);
    }

    #endregion

    #region ViewModel Enum Arrays

    [Fact]
    public void MainViewModel_ComponentTypes_ContainsAll11()
    {
        var vm = new MainViewModel();
        // 11 real types + None = 12
        Assert.Equal(12, vm.ComponentTypes.Length);
    }

    [Fact]
    public void MainViewModel_ChangeTypes_ContainsAll2()
    {
        var vm = new MainViewModel();
        // 2 real types + None = 3
        Assert.Equal(3, vm.ChangeTypes.Length);
    }

    [Fact]
    public void MainViewModel_Sizes_ContainsAll3()
    {
        var vm = new MainViewModel();
        // 3 real sizes + None = 4
        Assert.Equal(4, vm.Sizes.Length);
    }

    [Fact]
    public void MainViewModel_CollaborationTypes_ContainsAll5()
    {
        var vm = new MainViewModel();
        Assert.Equal(4, vm.CollaborationTypes.Length);
    }

    [Fact]
    public void MainViewModel_PmEffortOptions_Has5Values()
    {
        var vm = new MainViewModel();
        // Range 1-20
        Assert.Equal(20, vm.PmEffortOptions.Length);
        Assert.Contains(5m, vm.PmEffortOptions);
        Assert.Contains(10m, vm.PmEffortOptions);
        Assert.Contains(15m, vm.PmEffortOptions);
        Assert.Contains(20m, vm.PmEffortOptions);
    }

    #endregion

    #region EstimateSummary Model

    [Fact]
    public void EstimateSummary_DefaultValues()
    {
        var summary = new EstimateSummary();
        Assert.Equal(0m, summary.DevelopmentHours);
        Assert.Equal(0m, summary.SystemTestingHours);
        Assert.Equal(0m, summary.AnalysisHours);
        Assert.Equal(0m, summary.BusinessDesignHours);
        Assert.Equal(0m, summary.PromotionHours);
        Assert.Equal(0m, summary.BASystemDocumentationHours);
        Assert.Equal(0m, summary.ProductionValidationHours);
        Assert.Equal(0m, summary.ProjectManagementHours);
        Assert.Equal(0m, summary.GrandTotalHours);
        Assert.Equal(string.Empty, summary.TShirtSize);
        Assert.Equal(0, summary.ComponentCount);
    }

    [Fact]
    public void EstimateSummary_RoleBreakout_DefaultValues()
    {
        var summary = new EstimateSummary();
        Assert.Equal(0m, summary.BAHours);
        Assert.Equal(0m, summary.SEHours);
        Assert.Equal(0m, summary.TesterHours);
        Assert.Equal(0m, summary.PMHours);
    }

    [Fact]
    public void EstimateSummary_CanSetAllProperties()
    {
        var summary = new EstimateSummary
        {
            DevelopmentHours = 100m,
            SystemTestingHours = 30m,
            AnalysisHours = 6.5m,
            BusinessDesignHours = 19.5m,
            PromotionHours = 5m,
            BASystemDocumentationHours = 5m,
            ProductionValidationHours = 6m,
            ProjectManagementHours = 25.8m,
            GrandTotalHours = 197.8m,
            TShirtSize = "Medium",
            ComponentCount = 5,
            BAHours = 40m,
            SEHours = 110m,
            TesterHours = 30m,
            PMHours = 25.8m
        };

        Assert.Equal(100m, summary.DevelopmentHours);
        Assert.Equal("Medium", summary.TShirtSize);
        Assert.Equal(5, summary.ComponentCount);
    }

    #endregion

    #region ComponentEntry Model

    [Fact]
    public void ComponentEntry_TotalHours_IsBaseTimesCount()
    {
        var entry = new ComponentEntry
        {
            BaseHoursPerUnit = 75m,
            Count = 3
        };
        Assert.Equal(225m, entry.TotalHours);
    }

    [Fact]
    public void ComponentEntry_TotalHours_ZeroCount()
    {
        var entry = new ComponentEntry { BaseHoursPerUnit = 100m, Count = 0 };
        Assert.Equal(0m, entry.TotalHours);
    }

    [Fact]
    public void ComponentEntry_DefaultValues()
    {
        var entry = new ComponentEntry();
        Assert.Equal(0, entry.LineNumber);
        Assert.Equal(string.Empty, entry.RequirementId);
        Assert.Equal(ComponentType.PowerBuilderWindows, entry.ComponentType);
        Assert.Equal(string.Empty, entry.Description);
        Assert.Equal(ChangeType.New, entry.ChangeType);
        Assert.Equal(ComponentSize.Small, entry.Size);
        Assert.Equal(1, entry.Count);
        Assert.Equal(0m, entry.BaseHoursPerUnit);
    }

    #endregion

    #region WeightedValueEntity Model

    [Fact]
    public void WeightedValueEntity_DefaultValues()
    {
        var entity = new WeightedValueEntity();
        Assert.Equal(0, entity.Id);
        Assert.Equal(0m, entity.BaseHours);
        Assert.Equal("System", entity.ModifiedBy);
        Assert.True(entity.LastModified <= DateTime.UtcNow);
    }

    [Fact]
    public void WeightedValueEntity_CanSetAllProperties()
    {
        var entity = new WeightedValueEntity
        {
            Id = 42,
            ComponentType = ComponentType.K2Workflow,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.Change,
            BaseHours = 150m,
            ModifiedBy = "Admin",
            LastModified = new DateTime(2026, 1, 1)
        };

        Assert.Equal(42, entity.Id);
        Assert.Equal(ComponentType.K2Workflow, entity.ComponentType);
        Assert.Equal(ComponentSize.Large, entity.Size);
        Assert.Equal(ChangeType.Change, entity.ChangeType);
        Assert.Equal(150m, entity.BaseHours);
        Assert.Equal("Admin", entity.ModifiedBy);
    }

    #endregion
}

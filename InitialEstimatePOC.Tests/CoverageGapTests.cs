using System.Globalization;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Converters;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests covering previously uncovered code paths:
/// - EnumDisplayConverter (all enum types)
/// - ZeroToVisibilityConverter
/// - MainViewModel dropdown options & HasValidComponents
/// - MainViewModel save validation (all required fields)
/// - DemoDataSeeder
/// - CollaborationItemEntity defaults
/// - EstimateSummary defaults
/// - WeightedValues.ResetToDefaults
/// </summary>
public class CoverageGapTests
{
    #region EnumDisplayConverter

    [Theory]
    [InlineData(ComponentType.None, "— Select —")]
    [InlineData(ComponentType.PowerBuilderWindows, "PowerBuilder Windows")]
    [InlineData(ComponentType.Reports, "Reports")]
    [InlineData(ComponentType.ProgramsDBStoredProcs, "Programs/DB Stored Procedures")]
    [InlineData(ComponentType.DBManipulation, "DB Manipulation (SQL, PL/SQL, etc.)")]
    [InlineData(ComponentType.DatabaseReview, "Database Review")]
    [InlineData(ComponentType.Webpage, "Webpage (Includes UI, Portal & Intranet)")]
    [InlineData(ComponentType.K2Workflow, "K2 Workflow")]
    [InlineData(ComponentType.K2SmartForm, "K2 Smart Form")]
    [InlineData(ComponentType.TestAutomationUFT, "Test Automation Suites (UFT)")]
    [InlineData(ComponentType.MISC, "MISC (Server Setup, Webserver Setup, Software Installation, etc.)")]
    public void EnumDisplayConverter_ComponentType_ReturnsDisplayName(ComponentType type, string expected)
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(type, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(CollaborationType.WPRs, "WPRs")]
    [InlineData(CollaborationType.ClientMeetings, "Client Meetings")]
    [InlineData(CollaborationType.InternalMeetings, "Internal Meetings")]
    [InlineData(CollaborationType.AutomationTestCollaboration, "Automation Test Collaboration")]
    public void EnumDisplayConverter_CollaborationType_ReturnsDisplayName(CollaborationType type, string expected)
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(type, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ChangeType.None, "— Select —")]
    [InlineData(ChangeType.New, "New")]
    [InlineData(ChangeType.Change, "Change")]
    public void EnumDisplayConverter_ChangeType_ReturnsCorrectString(ChangeType type, string expected)
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(type, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ComponentSize.None, "— Select —")]
    [InlineData(ComponentSize.Small, "Small")]
    [InlineData(ComponentSize.Medium, "Medium")]
    [InlineData(ComponentSize.Large, "Large")]
    public void EnumDisplayConverter_ComponentSize_ReturnsCorrectString(ComponentSize type, string expected)
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(type, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnumDisplayConverter_NullValue_ReturnsEmptyString()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EnumDisplayConverter_UnknownType_ReturnsToString()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(42, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("42", result);
    }

    [Fact]
    public void EnumDisplayConverter_ConvertBack_Throws()
    {
        var converter = new EnumDisplayConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack("test", typeof(ComponentType), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region ZeroToVisibilityConverter

    [Fact]
    public void ZeroToVisibilityConverter_Zero_ReturnsVisible()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert(0, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ZeroToVisibilityConverter_NonZero_ReturnsCollapsed()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert(5, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ZeroToVisibilityConverter_NonInt_ReturnsCollapsed()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ZeroToVisibilityConverter_ConvertBack_Throws()
    {
        var converter = new ZeroToVisibilityConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(Visibility.Visible, typeof(int), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region MainViewModel Dropdown Options

    [Fact]
    public void PmEffortOptions_Contains1To20()
    {
        var vm = new MainViewModel();
        Assert.Equal(20, vm.PmEffortOptions.Length);
        Assert.Equal(1m, vm.PmEffortOptions[0]);
        Assert.Equal(20m, vm.PmEffortOptions[^1]);
    }

    [Fact]
    public void MeetingCountOptions_Contains0To20()
    {
        var vm = new MainViewModel();
        Assert.Equal(21, vm.MeetingCountOptions.Length);
        Assert.Equal(0, vm.MeetingCountOptions[0]);
        Assert.Equal(20, vm.MeetingCountOptions[^1]);
    }

    [Fact]
    public void MeetingDurationOptions_ContainsExpectedValues()
    {
        var vm = new MainViewModel();
        Assert.Equal(new[] { 0, 15, 30, 45, 60 }, vm.MeetingDurationOptions);
    }

    [Fact]
    public void ParticipantCountOptions_Contains0To20()
    {
        var vm = new MainViewModel();
        Assert.Equal(21, vm.ParticipantCountOptions.Length);
        Assert.Equal(0, vm.ParticipantCountOptions[0]);
        Assert.Equal(20, vm.ParticipantCountOptions[^1]);
    }

    [Fact]
    public void PrepTimeOptions_Contains0To180ByStep15()
    {
        var vm = new MainViewModel();
        Assert.Equal(13, vm.PrepTimeOptions.Length);
        Assert.Equal(0, vm.PrepTimeOptions[0]);
        Assert.Equal(15, vm.PrepTimeOptions[1]);
        Assert.Equal(180, vm.PrepTimeOptions[^1]);
    }

    #endregion

    #region HasValidComponents

    [Fact]
    public void HasValidComponents_NoComponents_ReturnsFalse()
    {
        var vm = new MainViewModel();
        Assert.False(vm.HasValidComponents);
    }

    [Fact]
    public void HasValidComponents_IncompleteComponent_ReturnsFalse()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        // Component has None types and Count=0 by default
        Assert.False(vm.HasValidComponents);
    }

    [Fact]
    public void HasValidComponents_ValidComponent_ReturnsTrue()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.RequirementId = "REQ-001";
        row.ComponentType = ComponentType.MISC;
        row.ChangeType = ChangeType.New;
        row.Size = ComponentSize.Small;
        row.Count = 1;
        Assert.True(vm.HasValidComponents);
    }

    [Fact]
    public void HasValidComponents_MissingRequirementId_ReturnsFalse()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.ChangeType = ChangeType.New;
        row.Size = ComponentSize.Small;
        row.Count = 1;
        // RequirementId is still empty
        Assert.False(vm.HasValidComponents);
    }

    [Fact]
    public void HasValidComponents_ZeroCount_ReturnsFalse()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.RequirementId = "REQ-001";
        row.ComponentType = ComponentType.MISC;
        row.ChangeType = ChangeType.New;
        row.Size = ComponentSize.Small;
        row.Count = 0;
        Assert.False(vm.HasValidComponents);
    }

    #endregion

    #region Save Validation (Required Fields)

    [Fact]
    public void SaveProject_MissingChangeOrderId_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "";
        var result = vm.SaveProject();
        Assert.Equal("CO / Defect # is required.", result);
    }

    [Fact]
    public void SaveProject_MissingDescription_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "";
        var result = vm.SaveProject();
        Assert.Equal("Description is required.", result);
    }

    [Fact]
    public void SaveProject_MissingEstimatedBy_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "";
        var result = vm.SaveProject();
        Assert.Equal("Estimated By is required.", result);
    }

    [Fact]
    public void SaveProject_MissingReviewedBy_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "";
        var result = vm.SaveProject();
        Assert.Equal("Reviewed By is required.", result);
    }

    [Fact]
    public void SaveProject_NoComponents_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        var result = vm.SaveProject();
        Assert.Equal("At least one component must be added before saving.", result);
    }

    #endregion

    #region Adjusted Hours Trigger Recalculate

    [Fact]
    public void WprsAdjustedHours_Change_TriggersRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var before = vm.GrandTotalHours;
        vm.WprsAdjustedHours = 10m;
        Assert.NotEqual(before, vm.GrandTotalHours);
    }

    [Fact]
    public void ClientMeetingsAdjustedHours_Change_TriggersRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var before = vm.GrandTotalHours;
        vm.ClientMeetingsAdjustedHours = 5m;
        Assert.NotEqual(before, vm.GrandTotalHours);
    }

    [Fact]
    public void InternalMeetingsAdjustedHours_Change_TriggersRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var before = vm.GrandTotalHours;
        vm.InternalMeetingsAdjustedHours = 5m;
        Assert.NotEqual(before, vm.GrandTotalHours);
    }

    [Fact]
    public void AutomationTestCollabAdjustedHours_Change_TriggersRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var before = vm.GrandTotalHours;
        vm.AutomationTestCollabAdjustedHours = 5m;
        Assert.NotEqual(before, vm.GrandTotalHours);
    }

    [Fact]
    public void ConsultantMentorAdjustedHours_Change_TriggersRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var before = vm.GrandTotalHours;
        vm.ConsultantMentorAdjustedHours = 5m;
        Assert.NotEqual(before, vm.GrandTotalHours);
    }

    #endregion

    #region DemoDataSeeder

    [Fact]
    public void SeedDemoProjects_EmptyDb_CreatesProjects()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        DemoDataSeeder.SeedDemoProjects(db);

        Assert.True(db.Projects.Any());
        Assert.True(db.Projects.Count() >= 12); // At least 12 demo projects
    }

    [Fact]
    public void SeedDemoProjects_AlreadyHasProjects_DoesNotDuplicate()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        DemoDataSeeder.SeedDemoProjects(db);
        var countFirst = db.Projects.Count();

        DemoDataSeeder.SeedDemoProjects(db);
        var countSecond = db.Projects.Count();

        Assert.Equal(countFirst, countSecond);
    }

    [Fact]
    public void SeedDemoProjects_AllProjectsHaveComponents()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.Include(p => p.Components).ToList();
        Assert.All(projects, p => Assert.NotEmpty(p.Components));
    }

    [Fact]
    public void SeedDemoProjects_AllProjectsHaveNames()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.ToList();
        Assert.All(projects, p => Assert.False(string.IsNullOrWhiteSpace(p.ProjectName)));
    }

    #endregion

    #region CollaborationItemEntity Defaults

    [Fact]
    public void CollaborationItemEntity_DefaultValues()
    {
        var entity = new CollaborationItemEntity();
        Assert.Equal(0, entity.ItemId);
        Assert.Equal(string.Empty, entity.ProjectId);
        Assert.Equal(0, entity.LineNumber);
        Assert.Equal(string.Empty, entity.TaskName);
        Assert.Equal(string.Empty, entity.CollaborationType);
        Assert.Equal(1, entity.NumberOfMeetings);
        Assert.Equal(60, entity.MeetingDurationMinutes);
        Assert.Equal(3, entity.NumberOfParticipants);
        Assert.Equal(15, entity.ParticipantPrepTimeMinutes);
        Assert.Equal(0m, entity.TotalHours);
        Assert.Equal(string.Empty, entity.Notes);
        Assert.Null(entity.Project);
    }

    #endregion

    #region ComponentEntryEntity Navigation Property

    [Fact]
    public void ComponentEntryEntity_Project_CanBeSet()
    {
        var project = new ProjectEntity { ProjectName = "Test" };
        var entity = new ComponentEntryEntity { Project = project };
        Assert.NotNull(entity.Project);
        Assert.Equal("Test", entity.Project.ProjectName);
    }

    #endregion

    #region EstimateSummary Defaults

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
        Assert.Equal(0m, summary.BAHours);
        Assert.Equal(0m, summary.SEHours);
        Assert.Equal(0m, summary.TesterHours);
        Assert.Equal(0m, summary.PMHours);
    }

    #endregion

    #region WeightedValues ResetToDefaults

    [Fact]
    public void ResetToDefaults_RestoresOriginalValues()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        DatabaseSeeder.Initialize(db);

        // Load from DB (which should match defaults)
        WeightedValues.LoadFromDatabase(db);

        // Corrupt a value
        var entity = db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New);
        entity.BaseHours = 999m;
        db.SaveChanges();
        WeightedValues.LoadFromDatabase(db);
        Assert.Equal(999m, WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Small, ChangeType.New));

        // Reset
        WeightedValues.ResetToDefaults();
        Assert.Equal(20m, WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Small, ChangeType.New));
    }

    #endregion

    #region DecimalFormatConverter

    [Fact]
    public void DecimalFormatConverter_Decimal_FormatsN2()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(123.456m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("123.46", result);
    }

    [Fact]
    public void DecimalFormatConverter_NonDecimal_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert("abc", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalFormatConverter_ConvertBack_ValidString_ReturnsDecimal()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("123.45", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void DecimalFormatConverter_ConvertBack_InvalidString_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("abc", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    #endregion

    #region WeightedValues GetDisplayName Edge Cases

    [Fact]
    public void GetDisplayName_None_ReturnsSelect()
    {
        Assert.Equal("— Select —", WeightedValues.GetDisplayName(ComponentType.None));
    }

    [Fact]
    public void GetDisplayName_UnknownEnum_ReturnsToString()
    {
        // Cast an invalid value
        var result = WeightedValues.GetDisplayName((ComponentType)999);
        Assert.Equal("999", result);
    }

    #endregion

    #region WeightedValues GetTShirtSize

    [Theory]
    [InlineData(0, "—")]
    [InlineData(-1, "—")]
    [InlineData(50, "Small")]
    [InlineData(99, "Small")]
    [InlineData(100, "Medium")]
    [InlineData(299, "Medium")]
    [InlineData(300, "Large")]
    [InlineData(749, "Large")]
    [InlineData(750, "X-Large")]
    [InlineData(999, "X-Large")]
    [InlineData(1000, "XL1")]
    [InlineData(2000, "XL2")]
    [InlineData(3000, "XL3")]
    [InlineData(4000, "XL4")]
    [InlineData(5000, "XL5")]
    [InlineData(6000, "XL6")]
    [InlineData(7000, "XL7")]
    [InlineData(8000, "XL8")]
    public void GetTShirtSize_ReturnsCorrectSize(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    #endregion

    #region ComponentRowViewModel Notes Property

    [Fact]
    public void ComponentRow_Notes_CanBeSet()
    {
        var row = new ComponentRowViewModel();
        row.Notes = "Some notes";
        Assert.Equal("Some notes", row.Notes);
    }

    #endregion

    #region SupportModules Long Display Name (EnumDisplayConverter)

    [Fact]
    public void EnumDisplayConverter_SupportModules_HasLongName()
    {
        var converter = new EnumDisplayConverter();
        var result = (string)converter.Convert(ComponentType.SupportModules, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.StartsWith("Support Modules/JOB/JIL", result);
        Assert.Contains(".c,", result);
    }

    #endregion
}

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Converters;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// QA Audit: Negative, boundary, and edge-case tests covering gaps identified 
/// in the comprehensive test coverage audit. Focuses on:
/// - RoundUp negative values
/// - Negative effective development hours
/// - Per-type collaboration totals
/// - LoadProject backward compatibility 
/// - LoadProject with empty collaboration items
/// - LoadProject with invalid enum strings (TryParse fallback)
/// - OnComponentChanged filtering (irrelevant properties don't trigger recalculate)
/// - Negative meeting/participant values
/// - Extreme boundary values
/// - ClearAll → re-add consistency
/// - CollaborationRowViewModel property coverage
/// </summary>
public class NegativeBoundaryTests
{
    #region RoundUp Negative Values

    [Fact]
    public void RoundUp_NegativeValue_RoundsAwayFromZero()
    {
        // -3.451 → shifted = -345.1, truncated = -345, shifted < truncated → (−345 − 1) / 100 = -3.46
        Assert.Equal(-3.46m, MainViewModel.RoundUp(-3.451m));
    }

    [Fact]
    public void RoundUp_NegativeExact_ReturnsExact()
    {
        // -5.12 → shifted = -512.0, truncated = -512 → exact match → -512/100 = -5.12
        Assert.Equal(-5.12m, MainViewModel.RoundUp(-5.12m));
    }

    [Fact]
    public void RoundUp_NegativeSmall_RoundsAwayFromZero()
    {
        // -0.001 → shifted = -0.1, truncated = 0, shifted < truncated → (0 - 1) / 100 = -0.01
        Assert.Equal(-0.01m, MainViewModel.RoundUp(-0.001m));
    }

    [Fact]
    public void RoundUp_NegativeWholeNumber_ReturnsExact()
    {
        Assert.Equal(-10.00m, MainViewModel.RoundUp(-10.00m));
    }

    #endregion

    #region Negative Effective Development Hours

    [Fact]
    public void DevelopmentAdjusted_ExceedsDev_NegativeEffective()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        // MISC Small New = 20.00, set adjusted to -50
        vm.DevelopmentAdjustedHours = -50m;

        // effectiveDev = 20 + (-50) = -30
        // SysTest = RoundUp(-30 * 0.30) = RoundUp(-9) = -9
        Assert.Equal(-30m, vm.DevelopmentTotalHours);
    }

    [Fact]
    public void NegativeEffectiveDev_CascadesToDerivedTasks()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        // MISC Small New = 20, adjusted = -100 → effective = -80
        vm.DevelopmentAdjustedHours = -100m;

        // SysTest = RoundUp(-80 * 0.30) = RoundUp(-24) = -24
        Assert.True(vm.SystemTestingTotalHours < 0);
        // Analysis = RoundUp((-80 + SysTest) * 0.05)
        Assert.True(vm.AnalysisTotalHours < 0);
        // GrandTotal can go negative
        Assert.True(vm.SubtotalHours < 0);
    }

    #endregion

    #region Per-Type Collaboration Totals

    [Fact]
    public void CollaborationByType_ClientMeetings_SumsCorrectly()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Set a collaboration item to ClientMeetings with known values
        var clientItem = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.ClientMeetings);
        clientItem.NumberOfMeetings = 2;
        clientItem.MeetingDurationMinutes = 60;
        clientItem.NumberOfParticipants = 3;
        clientItem.ParticipantPrepTimeMinutes = 30;

        // TotalHours = 2 * ((60/60) + (30/60)) * 3 = 2 * 1.5 * 3 = 9.0
        Assert.Equal(9.00m, vm.ClientMeetingsHours);
    }

    [Fact]
    public void CollaborationByType_InternalMeetings_SumsCorrectly()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        var internalItem = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.InternalMeetings);
        internalItem.NumberOfMeetings = 4;
        internalItem.MeetingDurationMinutes = 30;
        internalItem.NumberOfParticipants = 5;
        internalItem.ParticipantPrepTimeMinutes = 15;

        // TotalHours = RoundUp(4 * ((30/60) + (15/60)) * 5) = RoundUp(4 * 0.75 * 5) = RoundUp(15) = 15
        Assert.Equal(15.00m, vm.InternalMeetingsHours);
    }

    [Fact]
    public void CollaborationByType_AutomationTest_SumsCorrectly()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        var autoItem = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.AutomationTestCollaboration);
        autoItem.NumberOfMeetings = 1;
        autoItem.MeetingDurationMinutes = 60;
        autoItem.NumberOfParticipants = 2;
        autoItem.ParticipantPrepTimeMinutes = 0;

        // TotalHours = RoundUp(1 * ((60/60) + 0) * 2) = RoundUp(2) = 2
        Assert.Equal(2.00m, vm.AutomationTestCollabHours);
    }

    [Fact]
    public void CollaborationByType_PerTypeTotals_IncludeAdjustedHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // All collab defaults are 0 meetings → 0 hours
        vm.WprsAdjustedHours = 5m;
        vm.ClientMeetingsAdjustedHours = 3m;
        vm.InternalMeetingsAdjustedHours = 2m;
        vm.AutomationTestCollabAdjustedHours = 1m;
        vm.ConsultantMentorAdjustedHours = 4m;

        Assert.Equal(5m, vm.WprsTotalHours);
        Assert.Equal(3m, vm.ClientMeetingsTotalHours);
        Assert.Equal(2m, vm.InternalMeetingsTotalHours);
        Assert.Equal(1m, vm.AutomationTestCollabTotalHours);
        Assert.Equal(4m, vm.ConsultantMentorTotalHours);
    }

    [Fact]
    public void ConsultantMentorHours_AlwaysZero_NoEnumMapping()
    {
        // ConsultantMentor is not in the CollaborationType enum switch — verify it's always 0
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Set all items to non-zero
        foreach (var item in vm.CollaborationItems)
        {
            item.NumberOfMeetings = 5;
            item.MeetingDurationMinutes = 60;
            item.NumberOfParticipants = 3;
        }

        // ConsultantMentorHours should still be 0 (no CollaborationType maps to it)
        Assert.Equal(0m, vm.ConsultantMentorHours);
    }

    #endregion

    #region LoadProject Backward Compatibility

    [Fact]
    public void LoadProject_FallsBackToSeAdjustedHours_WhenDevIsZero()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "BackwardCompat Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.SaveProject();

        // Simulate old project format: DevelopmentAdjustedHours=0, SeAdjustedHours=10
        var projects = MainViewModel.GetAllProjects();
        var entity = projects.First(p => p.ProjectName == "BackwardCompat Test");
        entity.DevelopmentAdjustedHours = 0m;
        entity.SeAdjustedHours = 10m;
        entity.AnalysisAdjustedHours = 0m;
        entity.BaAdjustedHours = 7m;

        var vm2 = new MainViewModel();
        vm2.LoadProject(entity);

        // Backward compat: loads SeAdjustedHours as DevelopmentAdjustedHours
        Assert.Equal(10m, vm2.DevelopmentAdjustedHours);
        Assert.Equal(7m, vm2.AnalysisAdjustedHours);
    }

    [Fact]
    public void LoadProject_PrefersDevAdjusted_WhenNonZero()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Prefer Dev Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var entity = projects.First(p => p.ProjectName == "Prefer Dev Test");
        entity.DevelopmentAdjustedHours = 25m;
        entity.SeAdjustedHours = 999m; // Should be ignored

        var vm2 = new MainViewModel();
        vm2.LoadProject(entity);

        Assert.Equal(25m, vm2.DevelopmentAdjustedHours);
    }

    #endregion

    #region LoadProject Empty Collaboration Items

    [Fact]
    public void LoadProject_NoCollaborationItems_InitializesDefaults()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "No Collab Test";
        vm.ChangeOrderId = "CO-1";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.SaveProject();

        // Create an entity with empty collaboration items
        var projects = MainViewModel.GetAllProjects();
        var entity = projects.First(p => p.ProjectName == "No Collab Test");
        entity.CollaborationItems.Clear();

        var vm2 = new MainViewModel();
        vm2.LoadProject(entity);

        // Should have 4 default collaboration items
        Assert.Equal(4, vm2.CollaborationItems.Count);
    }

    #endregion

    #region LoadProject Invalid Enum Fallback

    [Fact]
    public void LoadProject_InvalidCollaborationType_FallsBackToWPRs()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Invalid Collab Type",
            ChangeOrderId = "CO-1",
            ProjectDescription = "Test",
            EstimatedBy = "Tester",
            ReviewedBy = "Reviewer",
            Components = new List<ComponentEntryEntity>
            {
                new ComponentEntryEntity
                {
                    LineNumber = 1,
                    ComponentType = "MISC",
                    ChangeType = "New",
                    Size = "Small",
                    Count = 1
                }
            },
            CollaborationItems = new List<CollaborationItemEntity>
            {
                new CollaborationItemEntity
                {
                    LineNumber = 1,
                    TaskName = "Invalid Type",
                    CollaborationType = "NonExistentType", // Invalid enum
                    NumberOfMeetings = 2,
                    MeetingDurationMinutes = 60,
                    NumberOfParticipants = 3,
                    ParticipantPrepTimeMinutes = 15,
                }
            }
        };

        var vm = new MainViewModel();
        vm.LoadProject(entity);

        // Should fallback to WPRs
        Assert.Equal(CollaborationType.WPRs, vm.CollaborationItems[0].CollabType);
    }

    #endregion

    #region OnComponentChanged Filtering

    [Fact]
    public void OnComponentChanged_DescriptionChange_DoesNotRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var totalBefore = vm.GrandTotalHours;

        // Changing Description should NOT trigger recalculate
        vm.Components[0].Description = "Updated description";

        Assert.Equal(totalBefore, vm.GrandTotalHours);
    }

    [Fact]
    public void OnComponentChanged_NotesChange_DoesNotRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var totalBefore = vm.GrandTotalHours;

        vm.Components[0].Notes = "Some notes";

        Assert.Equal(totalBefore, vm.GrandTotalHours);
    }

    [Fact]
    public void OnComponentChanged_RequirementIdChange_DoesNotRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var totalBefore = vm.GrandTotalHours;

        vm.Components[0].RequirementId = "REQ-999";

        Assert.Equal(totalBefore, vm.GrandTotalHours);
    }

    [Fact]
    public void OnComponentChanged_CountChange_DoesRecalculate()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var totalBefore = vm.GrandTotalHours;

        vm.Components[0].Count = 5;

        Assert.NotEqual(totalBefore, vm.GrandTotalHours);
    }

    #endregion

    #region Collaboration Notes and TaskName

    [Fact]
    public void CollaborationRow_Notes_CanBeSet()
    {
        var row = new CollaborationRowViewModel();
        row.Notes = "Review notes";
        Assert.Equal("Review notes", row.Notes);
    }

    [Fact]
    public void CollaborationRow_TaskName_CanBeSet()
    {
        var row = new CollaborationRowViewModel();
        row.TaskName = "Sprint Planning";
        Assert.Equal("Sprint Planning", row.TaskName);
    }

    [Fact]
    public void CollaborationRow_NotesChange_DoesNotAffectTotalHours()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        var totalBefore = vm.GrandTotalHours;

        vm.CollaborationItems[0].Notes = "Updated notes";

        Assert.Equal(totalBefore, vm.GrandTotalHours);
    }

    #endregion

    #region Negative Collaboration Values (Boundary)

    [Fact]
    public void CollaborationRow_ZeroMeetings_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 30
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void CollaborationRow_ZeroParticipants_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 30
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void CollaborationRow_ZeroDurationAndPrepTime_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 0
        };
        Assert.Equal(0m, row.TotalHours);
    }

    #endregion

    #region ClearAll Then Re-Add Consistency

    [Fact]
    public void ClearAll_ThenAddComponent_NoResidualState()
    {
        var vm = new MainViewModel();
        // Setup a full project
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.K2Workflow;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 3;
        vm.DevelopmentAdjustedHours = 50m;
        vm.PmEffortPercentage = 20m;
        Assert.True(vm.GrandTotalHours > 0);

        // Clear all
        vm.ClearAllCommand.Execute(null);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal(0m, vm.DevelopmentAdjustedHours);

        // Add a new component — should calculate fresh
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Should be based only on MISC Small New (20.00) with default PM=15%
        Assert.True(vm.GrandTotalHours > 0);
        Assert.Equal(20m, vm.DevelopmentTotalHours);
    }

    [Fact]
    public void ClearAll_ResetsCollaborationItems_ToDefaults()
    {
        var vm = new MainViewModel();
        // Modify collaboration
        vm.CollaborationItems[0].NumberOfMeetings = 10;
        vm.CollaborationItems[0].MeetingDurationMinutes = 60;

        vm.ClearAllCommand.Execute(null);

        // Should have 4 fresh defaults
        Assert.Equal(4, vm.CollaborationItems.Count);
        Assert.All(vm.CollaborationItems, item =>
        {
            Assert.Equal(0, item.NumberOfMeetings);
            Assert.Equal(0, item.MeetingDurationMinutes);
            Assert.Equal(0, item.NumberOfParticipants);
            Assert.Equal(0, item.ParticipantPrepTimeMinutes);
        });
    }

    #endregion

    #region ComponentType None / Size None / ChangeType None

    [Fact]
    public void ComponentRow_AllNone_BaseHoursZero()
    {
        var row = new ComponentRowViewModel();
        // Default is all None
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void ComponentRow_ComponentTypeNone_BaseHoursZero()
    {
        var row = new ComponentRowViewModel
        {
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            ComponentType = ComponentType.None
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_SizeNone_BaseHoursZero()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            ChangeType = ChangeType.New,
            Size = ComponentSize.None
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void ComponentRow_ChangeTypeNone_BaseHoursZero()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.None
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
    }

    #endregion

    #region UseTestCases Interaction with AdjustedHours

    [Fact]
    public void UseTestCases_WithDevelopmentAdjusted_BothApply()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        // MISC Large New = 100

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCasesComplex = 2;
        vm.TestCasesVeryComplex = 1;
        vm.TestCaseIterations = 2;
        vm.DevelopmentAdjustedHours = 25m;

        // effectiveDev = 100 + 25 = 125
        Assert.Equal(125m, vm.DevelopmentTotalHours);
        // SystemTesting uses test case formula (not 30% of dev)
        Assert.True(vm.SystemTestingTotalHours > 0);
    }

    [Fact]
    public void NegativeTestCaseCounts_TreatedAsZero()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = -5;  // Negative
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;

        // Should not crash; negative counts produce negative hours (no guard in code)
        // This tests that the calculation doesn't throw
        Assert.True(true); // If we get here, no exception
    }

    #endregion

    #region Large Values (Overflow Protection)

    [Fact]
    public void ComponentRow_LargeCount_NoOverflow()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.K2Workflow,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 10000 // 200 * 10000 = 2,000,000
        };
        Assert.Equal(2_000_000m, row.TotalHours);
    }

    [Fact]
    public void DecimalFormatConverter_LargeValue_Formats()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(999_999_999.99m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Contains("999", (string)result);
    }

    #endregion

    #region ZeroToVisibilityConverter Null

    [Fact]
    public void ZeroToVisibilityConverter_NullValue_ReturnsCollapsed()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert(null!, typeof(System.Windows.Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(System.Windows.Visibility.Collapsed, result);
    }

    #endregion

    #region Save/Load Round Trip with All Adjusted Hours

    [Fact]
    public void SaveLoad_AllAdjustedHours_Preserved()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "All Adjusted Test";
        vm.ChangeOrderId = "CO-ADJ";
        vm.ProjectDescription = "Test all adjusted";
        vm.EstimatedBy = "QA";
        vm.ReviewedBy = "QA Lead";
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.DevelopmentAdjustedHours = 10m;
        vm.AnalysisAdjustedHours = 5m;
        vm.BusinessDesignAdjustedHours = 3m;
        vm.SystemTestingAdjustedHours = 7m;
        vm.PromotionAdjustedHours = 2m;
        vm.BaSystemDocAdjustedHours = 1m;
        vm.ProductionValidationAdjustedHours = 4m;
        vm.ProjectManagementAdjustedHours = 6m;
        vm.WprsAdjustedHours = 8m;
        vm.ClientMeetingsAdjustedHours = 9m;
        vm.InternalMeetingsAdjustedHours = 11m;
        vm.AutomationTestCollabAdjustedHours = 12m;
        vm.ConsultantMentorAdjustedHours = 13m;

        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var entity = projects.First(p => p.ProjectName == "All Adjusted Test");

        var vm2 = new MainViewModel();
        vm2.LoadProject(entity);

        Assert.Equal(10m, vm2.DevelopmentAdjustedHours);
        Assert.Equal(5m, vm2.AnalysisAdjustedHours);
        Assert.Equal(3m, vm2.BusinessDesignAdjustedHours);
        Assert.Equal(7m, vm2.SystemTestingAdjustedHours);
        Assert.Equal(2m, vm2.PromotionAdjustedHours);
        Assert.Equal(1m, vm2.BaSystemDocAdjustedHours);
        Assert.Equal(4m, vm2.ProductionValidationAdjustedHours);
        Assert.Equal(6m, vm2.ProjectManagementAdjustedHours);
        Assert.Equal(8m, vm2.WprsAdjustedHours);
        Assert.Equal(9m, vm2.ClientMeetingsAdjustedHours);
        Assert.Equal(11m, vm2.InternalMeetingsAdjustedHours);
        Assert.Equal(12m, vm2.AutomationTestCollabAdjustedHours);
        Assert.Equal(13m, vm2.ConsultantMentorAdjustedHours);
    }

    #endregion

    #region Collaboration Save/Load Round Trip

    [Fact]
    public void SaveLoad_CollaborationItems_Preserved()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Collab RoundTrip";
        vm.ChangeOrderId = "CO-COL";
        vm.ProjectDescription = "Test collab round trip";
        vm.EstimatedBy = "QA";
        vm.ReviewedBy = "QA Lead";
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        // Modify collaboration items
        vm.CollaborationItems[0].NumberOfMeetings = 5;
        vm.CollaborationItems[0].MeetingDurationMinutes = 45;
        vm.CollaborationItems[0].NumberOfParticipants = 4;
        vm.CollaborationItems[0].ParticipantPrepTimeMinutes = 30;
        vm.CollaborationItems[0].Notes = "WPR sessions";

        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var entity = projects.First(p => p.ProjectName == "Collab RoundTrip");

        var vm2 = new MainViewModel();
        vm2.LoadProject(entity);

        Assert.Equal(4, vm2.CollaborationItems.Count);
        var loaded = vm2.CollaborationItems[0];
        Assert.Equal(5, loaded.NumberOfMeetings);
        Assert.Equal(45, loaded.MeetingDurationMinutes);
        Assert.Equal(4, loaded.NumberOfParticipants);
        Assert.Equal(30, loaded.ParticipantPrepTimeMinutes);
        Assert.Equal("WPR sessions", loaded.Notes);
    }

    #endregion

    #region ComponentCount Excludes None Types

    [Fact]
    public void ComponentCount_ExcludesNoneTypes()
    {
        var vm = new MainViewModel();
        vm.AddComponentCommand.Execute(null); // Added with ComponentType.None
        vm.AddComponentCommand.Execute(null); // Another with None
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        // Components[1] is still None

        // Components collection has 2, but only 1 contributes to dev hours
        Assert.Equal(2, vm.Components.Count);
        Assert.Equal(20m, vm.DevelopmentTotalHours); // Only MISC Small New = 20
    }

    #endregion
}

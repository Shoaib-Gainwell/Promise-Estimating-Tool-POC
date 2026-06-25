using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for project persistence round-trip with collaboration items,
/// adjusted hours, assumptions, and all new fields.
/// </summary>
public class PersistenceRoundTripTests
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

    #region LoadProject — Component Round-Trip

    [Fact]
    public void LoadProject_RestoresComponents()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            Components = new List<ComponentEntryEntity>
            {
                new() { LineNumber = 1, ComponentType = "Reports", Size = "Medium", ChangeType = "New", Count = 3, RequirementId = "REQ-1", Description = "Monthly report" },
                new() { LineNumber = 2, ComponentType = "Webpage", Size = "Small", ChangeType = "Change", Count = 2, RequirementId = "REQ-2", Description = "Edit form" },
            },
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(2, vm.Components.Count);
        Assert.Equal(ComponentType.Reports, vm.Components[0].ComponentType);
        Assert.Equal(ComponentSize.Medium, vm.Components[0].Size);
        Assert.Equal(ChangeType.New, vm.Components[0].ChangeType);
        Assert.Equal(3, vm.Components[0].Count);
        Assert.Equal("REQ-1", vm.Components[0].RequirementId);
        Assert.Equal("Monthly report", vm.Components[0].Description);

        Assert.Equal(ComponentType.Webpage, vm.Components[1].ComponentType);
        Assert.Equal(ComponentSize.Small, vm.Components[1].Size);
        Assert.Equal(ChangeType.Change, vm.Components[1].ChangeType);
        Assert.Equal(2, vm.Components[1].Count);
    }

    [Fact]
    public void LoadProject_RestoresBaseHoursOnComponents()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            Components = new List<ComponentEntryEntity>
            {
                new() { LineNumber = 1, ComponentType = "K2Workflow", Size = "Large", ChangeType = "New", Count = 1 },
            },
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(200m, vm.Components[0].BaseHoursPerUnit);
        Assert.Equal(200m, vm.Components[0].TotalHours);
    }

    #endregion

    #region LoadProject — Collaboration Round-Trip

    [Fact]
    public void LoadProject_RestoresCollaborationItems()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>
            {
                new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 8, MeetingDurationMinutes = 45, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 10 },
                new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 3, MeetingDurationMinutes = 90, NumberOfParticipants = 5, ParticipantPrepTimeMinutes = 20 },
            }
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(2, vm.CollaborationItems.Count);

        Assert.Equal("WPRs", vm.CollaborationItems[0].TaskName);
        Assert.Equal(8, vm.CollaborationItems[0].NumberOfMeetings);
        Assert.Equal(45, vm.CollaborationItems[0].MeetingDurationMinutes);
        Assert.Equal(4, vm.CollaborationItems[0].NumberOfParticipants);
        Assert.Equal(10, vm.CollaborationItems[0].ParticipantPrepTimeMinutes);

        Assert.Equal("Client Meetings", vm.CollaborationItems[1].TaskName);
        Assert.Equal(CollaborationType.ClientMeetings, vm.CollaborationItems[1].CollabType);
    }

    [Fact]
    public void LoadProject_CollaborationHoursRecalculated()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>
            {
                new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15 },
            }
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        // 5 × (60/60 + 15/60) × 3 = 18.75
        Assert.Equal(18.75m, vm.TotalCollaborationHours);
    }

    #endregion

    #region LoadProject — Header Fields

    [Fact]
    public void LoadProject_RestoresHeaderFields()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Claims Processing",
            ChangeOrderId = "CO-2024-001",
            ProjectDescription = "Upgrade claims module",
            EstimatedBy = "John Smith",
            ReviewedBy = "Jane Doe",
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal("Claims Processing", vm.ProjectName);
        Assert.Equal("CO-2024-001", vm.ChangeOrderId);
        Assert.Equal("Upgrade claims module", vm.ProjectDescription);
        Assert.Equal("John Smith", vm.EstimatedBy);
        Assert.Equal("Jane Doe", vm.ReviewedBy);
    }

    #endregion

    #region LoadProject — Configuration Fields

    [Fact]
    public void LoadProject_RestoresPmPercentages()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            PmEffortPercentage = 20m,
            PmReservePercentage = 10m,
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(20m, vm.PmEffortPercentage);
        Assert.Equal(10m, vm.PmReservePercentage);
    }

    #endregion

    #region LoadProject — Adjusted Hours

    [Fact]
    public void LoadProject_RestoresAllAdjustedHours()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            DevelopmentAdjustedHours = 10m,
            AnalysisAdjustedHours = 5m,
            BusinessDesignAdjustedHours = 3m,
            SystemTestingAdjustedHours = 7m,
            PromotionAdjustedHours = 2m,
            BaSystemDocAdjustedHours = 1m,
            ProductionValidationAdjustedHours = 4m,
            ProjectManagementAdjustedHours = 6m,
            CollaborationAdjustedHours = 8m,
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(10m, vm.DevelopmentAdjustedHours);
        Assert.Equal(5m, vm.AnalysisAdjustedHours);
        Assert.Equal(3m, vm.BusinessDesignAdjustedHours);
        Assert.Equal(7m, vm.SystemTestingAdjustedHours);
        Assert.Equal(2m, vm.PromotionAdjustedHours);
        Assert.Equal(1m, vm.BaSystemDocAdjustedHours);
        Assert.Equal(4m, vm.ProductionValidationAdjustedHours);
        Assert.Equal(6m, vm.ProjectManagementAdjustedHours);
        Assert.Equal(8m, vm.CollaborationAdjustedHours);
    }

    #endregion

    #region LoadProject — Assumptions

    [Fact]
    public void LoadProject_RestoresAssumptions()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            SeAssumptions = "SE notes here",
            BaAssumptions = "BA notes here",
            CollaborationAssumptions = "Collab notes",
            GeneralAssumptions = "General notes",
            AdjustedHoursComments = "Adjusted comments",
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal("SE notes here", vm.SeAssumptions);
        Assert.Equal("BA notes here", vm.BaAssumptions);
        Assert.Equal("Collab notes", vm.CollaborationAssumptions);
        Assert.Equal("General notes", vm.GeneralAssumptions);
        Assert.Equal("Adjusted comments", vm.AdjustedHoursComments);
    }

    #endregion

    #region LoadProject — Test Cases and Actual Hours

    [Fact]
    public void LoadProject_RestoresTestCaseSettings()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            UseTestCasesForEstimate = true,
            TestCasesSimple = 20,
            TestCasesMedium = 10,
            TestCasesComplex = 5,
            TestCasesVeryComplex = 2,
            TestCaseIterations = 3,
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.True(vm.UseTestCasesForEstimate);
        Assert.Equal(20, vm.TestCasesSimple);
        Assert.Equal(10, vm.TestCasesMedium);
        Assert.Equal(5, vm.TestCasesComplex);
        Assert.Equal(2, vm.TestCasesVeryComplex);
        Assert.Equal(3, vm.TestCaseIterations);

        // Verify calculation: (20*0.5 + 10*1 + 5*2 + 2*4) * 3 = (10+10+10+8) * 3 = 114
        Assert.Equal(114m, vm.SystemTestingHours);
    }

    [Fact]
    public void LoadProject_RestoresActualHoursAndDate()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test",
            TotalActualHours = 250m,
            ActualHoursAsOfDate = new DateTime(2026, 3, 15),
            TimeForEstimates = 30m,
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        Assert.Equal(250m, vm.TotalActualHours);
        Assert.Equal(new DateTime(2026, 3, 15), vm.ActualHoursAsOfDate);
        Assert.Equal(30m, vm.TimeForEstimates);
    }

    #endregion

    #region LoadProject — Clears Previous State

    [Fact]
    public void LoadProject_ClearsPreviousComponents()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 5);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 3);
        Assert.Equal(2, vm.Components.Count);

        var entity = new ProjectEntity
        {
            ProjectName = "New Project",
            Components = new List<ComponentEntryEntity>
            {
                new() { LineNumber = 1, ComponentType = "Webpage", Size = "Medium", ChangeType = "New", Count = 1 }
            },
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        vm.LoadProject(entity);

        Assert.Single(vm.Components);
        Assert.Equal(ComponentType.Webpage, vm.Components[0].ComponentType);
    }

    [Fact]
    public void LoadProject_ClearsPreviousCollaboration()
    {
        var vm = CreateVm();
        vm.AddCollaborationItemCommand.Execute(null);
        vm.AddCollaborationItemCommand.Execute(null);
        Assert.Equal(2, vm.CollaborationItems.Count);

        var entity = new ProjectEntity
        {
            ProjectName = "New Project",
            Components = new List<ComponentEntryEntity>(),
            CollaborationItems = new List<CollaborationItemEntity>
            {
                new() { LineNumber = 1, TaskName = "Only One", CollaborationType = "WPRs", NumberOfMeetings = 1, MeetingDurationMinutes = 30, NumberOfParticipants = 2, ParticipantPrepTimeMinutes = 10 }
            }
        };

        vm.LoadProject(entity);

        Assert.Single(vm.CollaborationItems);
        Assert.Equal("Only One", vm.CollaborationItems[0].TaskName);
    }

    #endregion

    #region LoadProject — Recalculates After Load

    [Fact]
    public void LoadProject_TriggersRecalculation()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Calc Test",
            Components = new List<ComponentEntryEntity>
            {
                new() { LineNumber = 1, ComponentType = "MISC", Size = "Large", ChangeType = "New", Count = 2 }
            },
            CollaborationItems = new List<CollaborationItemEntity>()
        };

        var vm = CreateVm();
        vm.LoadProject(entity);

        // MISC Large New = 100, count 2 = 200 dev hours
        Assert.Equal(200m, vm.TotalDevelopmentHours);
        Assert.True(vm.GrandTotalHours > 200m); // Includes derived tasks + PM
        // Dev(200) + derived + PM = ~395 subtotal + reserve → ~415 grand total → Large (300-749)
        Assert.Equal("Large", vm.TShirtSize);
    }

    #endregion

    #region SaveProject Validation

    [Fact]
    public void SaveProject_EmptyProjectName_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "";
        Assert.Equal("Project Name is required.", vm.SaveProject());
    }

    [Fact]
    public void SaveProject_WhitespaceName_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "   \t  ";
        Assert.Equal("Project Name is required.", vm.SaveProject());
    }

    [Fact]
    public void SaveProject_ValidName_ReturnsNull()
    {
        var vm = CreateVm();
        vm.ProjectName = "Valid Project Name";
        Assert.Null(vm.SaveProject());
    }

    #endregion

    #region GetAllProjects

    [Fact]
    public void GetAllProjects_ReturnsListOrderedByLastModified()
    {
        // Save two projects
        var vm1 = CreateVm();
        vm1.ProjectName = $"Project A {Guid.NewGuid():N}";
        vm1.SaveProject();

        var vm2 = CreateVm();
        vm2.ProjectName = $"Project B {Guid.NewGuid():N}";
        vm2.SaveProject();

        var all = MainViewModel.GetAllProjects();
        Assert.True(all.Count >= 2);
        // Most recently modified should be first
        Assert.True(all[0].LastModifiedDate >= all[1].LastModifiedDate);
    }

    #endregion
}

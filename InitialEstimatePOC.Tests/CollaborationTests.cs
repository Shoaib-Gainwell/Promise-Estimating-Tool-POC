using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for collaboration row calculations, default initialization,
/// add/remove collaboration items, and formula verification.
/// </summary>
public class CollaborationTests
{
    private MainViewModel CreateVm() => new();

    #region Collaboration Formula Tests

    [Fact]
    public void CollaborationFormula_StandardValues_CalculatesCorrectly()
    {
        // 5 meetings × (60/60 + 15/60) × 3 = 5 × 1.25 × 3 = 18.75
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 15
        };

        Assert.Equal(18.75m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_WPRs_Default_37Point5Hours()
    {
        // 10 meetings × (60/60 + 15/60) × 3 = 10 × 1.25 × 3 = 37.50
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 15
        };

        Assert.Equal(37.50m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_ZeroMeetings_ReturnsZero()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 15
        };

        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_ZeroParticipants_ReturnsZero()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 15
        };

        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_ZeroDurationAndPrepTime_ReturnsZero()
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

    [Fact]
    public void CollaborationFormula_OnlyPrepTime_CalculatesCorrectly()
    {
        // 5 × (0/60 + 30/60) × 2 = 5 × 0.5 × 2 = 5.00
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 2,
            ParticipantPrepTimeMinutes = 30
        };

        Assert.Equal(5.00m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_OnlyDuration_CalculatesCorrectly()
    {
        // 3 × (90/60 + 0/60) × 4 = 3 × 1.5 × 4 = 18.00
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 3,
            MeetingDurationMinutes = 90,
            NumberOfParticipants = 4,
            ParticipantPrepTimeMinutes = 0
        };

        Assert.Equal(18.00m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_LargeValues_NoOverflow()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 100,
            MeetingDurationMinutes = 120,
            NumberOfParticipants = 20,
            ParticipantPrepTimeMinutes = 60
        };

        // 100 × (120/60 + 60/60) × 20 = 100 × 3 × 20 = 6000
        Assert.Equal(6000m, row.TotalHours);
    }

    [Fact]
    public void CollaborationFormula_15MinMeeting_CalculatesCorrectly()
    {
        // 10 × (15/60 + 5/60) × 2 = 10 × 0.333... × 2 = 6.666...
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 2,
            ParticipantPrepTimeMinutes = 5
        };

        // 10 × (0.25 + 0.0833...) × 2 = 10 × 0.3333... × 2 = 6.6666...
        decimal expected = 10m * ((15m / 60m) + (5m / 60m)) * 2m;
        Assert.Equal(expected, row.TotalHours);
    }

    #endregion

    #region Default Collaboration Items

    [Fact]
    public void DefaultCollaboration_Has5Items()
    {
        var vm = CreateVm();
        Assert.Equal(5, vm.CollaborationItems.Count);
    }

    [Fact]
    public void DefaultCollaboration_WPRs_CorrectDefaults()
    {
        var vm = CreateVm();
        var wprs = vm.CollaborationItems[0];
        Assert.Equal("WPRs", wprs.TaskName);
        Assert.Equal(CollaborationType.WPRs, wprs.CollabType);
        Assert.Equal(10, wprs.NumberOfMeetings);
        Assert.Equal(60, wprs.MeetingDurationMinutes);
        Assert.Equal(3, wprs.NumberOfParticipants);
        Assert.Equal(15, wprs.ParticipantPrepTimeMinutes);
        Assert.Equal(37.50m, wprs.TotalHours);
    }

    [Fact]
    public void DefaultCollaboration_ClientMeetings_CorrectDefaults()
    {
        var vm = CreateVm();
        var client = vm.CollaborationItems[1];
        Assert.Equal("Client Meetings", client.TaskName);
        Assert.Equal(CollaborationType.ClientMeetings, client.CollabType);
        Assert.Equal(5, client.NumberOfMeetings);
        Assert.Equal(60, client.MeetingDurationMinutes);
        Assert.Equal(3, client.NumberOfParticipants);
        Assert.Equal(15, client.ParticipantPrepTimeMinutes);
        Assert.Equal(18.75m, client.TotalHours);
    }

    [Fact]
    public void DefaultCollaboration_InternalMeetings_CorrectDefaults()
    {
        var vm = CreateVm();
        var item = vm.CollaborationItems[2];
        Assert.Equal("Internal Meetings", item.TaskName);
        Assert.Equal(CollaborationType.InternalMeetings, item.CollabType);
        Assert.Equal(5, item.NumberOfMeetings);
        Assert.Equal(18.75m, item.TotalHours);
    }

    [Fact]
    public void DefaultCollaboration_AutomationTest_CorrectDefaults()
    {
        var vm = CreateVm();
        var item = vm.CollaborationItems[3];
        Assert.Equal("Automation Test Collaboration", item.TaskName);
        Assert.Equal(CollaborationType.AutomationTestCollaboration, item.CollabType);
        Assert.Equal(5, item.NumberOfMeetings);
        Assert.Equal(18.75m, item.TotalHours);
    }

    [Fact]
    public void DefaultCollaboration_ConsultantMentor_ZeroHours()
    {
        var vm = CreateVm();
        var item = vm.CollaborationItems[4];
        Assert.Equal("Consultant/Mentor Effort", item.TaskName);
        Assert.Equal(CollaborationType.ConsultantMentorEffort, item.CollabType);
        Assert.Equal(0, item.NumberOfMeetings);
        Assert.Equal(0m, item.TotalHours);
    }

    [Fact]
    public void DefaultCollaboration_TotalHours_93Point75()
    {
        // 37.50 + 18.75 + 18.75 + 18.75 + 0 = 93.75
        var vm = CreateVm();
        Assert.Equal(93.75m, vm.TotalCollaborationHours);
    }

    [Fact]
    public void DefaultCollaboration_LineNumbers_Sequential()
    {
        var vm = CreateVm();
        for (int i = 0; i < vm.CollaborationItems.Count; i++)
            Assert.Equal(i + 1, vm.CollaborationItems[i].LineNumber);
    }

    #endregion

    #region Add/Remove Collaboration Items

    [Fact]
    public void AddCollaborationItem_IncreasesCount()
    {
        var vm = CreateVm();
        int initialCount = vm.CollaborationItems.Count;
        vm.AddCollaborationItemCommand.Execute(null);
        Assert.Equal(initialCount + 1, vm.CollaborationItems.Count);
    }

    [Fact]
    public void AddCollaborationItem_DefaultValues()
    {
        var vm = CreateVm();
        vm.AddCollaborationItemCommand.Execute(null);
        var added = vm.CollaborationItems[^1];
        Assert.Equal(5, added.NumberOfMeetings);
        Assert.Equal(60, added.MeetingDurationMinutes);
        Assert.Equal(3, added.NumberOfParticipants);
        Assert.Equal(15, added.ParticipantPrepTimeMinutes);
    }

    [Fact]
    public void AddCollaborationItem_UpdatesTotalCollaborationHours()
    {
        var vm = CreateVm();
        decimal before = vm.TotalCollaborationHours;
        vm.AddCollaborationItemCommand.Execute(null);
        // New item: 5 × (60/60 + 15/60) × 3 = 18.75
        Assert.Equal(before + 18.75m, vm.TotalCollaborationHours);
    }

    [Fact]
    public void RemoveCollaborationItem_DecreasesCount()
    {
        var vm = CreateVm();
        int initialCount = vm.CollaborationItems.Count;
        vm.RemoveCollaborationItemCommand.Execute(vm.CollaborationItems[0]);
        Assert.Equal(initialCount - 1, vm.CollaborationItems.Count);
    }

    [Fact]
    public void RemoveCollaborationItem_Null_DoesNothing()
    {
        var vm = CreateVm();
        int count = vm.CollaborationItems.Count;
        vm.RemoveCollaborationItemCommand.Execute(null);
        Assert.Equal(count, vm.CollaborationItems.Count);
    }

    [Fact]
    public void RemoveCollaborationItem_RenumbersRemaining()
    {
        var vm = CreateVm();
        vm.RemoveCollaborationItemCommand.Execute(vm.CollaborationItems[1]);
        for (int i = 0; i < vm.CollaborationItems.Count; i++)
            Assert.Equal(i + 1, vm.CollaborationItems[i].LineNumber);
    }

    [Fact]
    public void RemoveAllCollaboration_ZeroTotal()
    {
        var vm = CreateVm();
        while (vm.CollaborationItems.Count > 0)
            vm.RemoveCollaborationItemCommand.Execute(vm.CollaborationItems[0]);
        Assert.Equal(0m, vm.TotalCollaborationHours);
        Assert.Equal(0, vm.CollaborationCount);
    }

    #endregion

    #region Collaboration Changes Trigger Recalculation

    [Fact]
    public void ChangeNumberOfMeetings_UpdatesTotal()
    {
        var vm = CreateVm();
        decimal before = vm.TotalCollaborationHours;
        vm.CollaborationItems[0].NumberOfMeetings = 20; // was 10
        // Diff: 10 extra meetings × (60/60 + 15/60) × 3 = 10 × 1.25 × 3 = 37.50
        Assert.Equal(before + 37.50m, vm.TotalCollaborationHours);
    }

    [Fact]
    public void ChangeMeetingDuration_UpdatesTotal()
    {
        var vm = CreateVm();
        var wprs = vm.CollaborationItems[0];
        wprs.MeetingDurationMinutes = 120; // was 60
        // New: 10 × (120/60 + 15/60) × 3 = 10 × 2.25 × 3 = 67.50
        Assert.Equal(67.50m, wprs.TotalHours);
    }

    [Fact]
    public void ChangeNumberOfParticipants_UpdatesTotal()
    {
        var vm = CreateVm();
        var wprs = vm.CollaborationItems[0];
        wprs.NumberOfParticipants = 5; // was 3
        // 10 × (60/60 + 15/60) × 5 = 10 × 1.25 × 5 = 62.50
        Assert.Equal(62.50m, wprs.TotalHours);
    }

    [Fact]
    public void ChangePrepTime_UpdatesTotal()
    {
        var vm = CreateVm();
        var wprs = vm.CollaborationItems[0];
        wprs.ParticipantPrepTimeMinutes = 30; // was 15
        // 10 × (60/60 + 30/60) × 3 = 10 × 1.5 × 3 = 45.00
        Assert.Equal(45.00m, wprs.TotalHours);
    }

    [Fact]
    public void CollaborationChange_AffectsGrandTotal()
    {
        var vm = CreateVm();
        decimal before = vm.GrandTotalHours;
        vm.CollaborationItems[0].NumberOfMeetings = 20;
        Assert.True(vm.GrandTotalHours > before);
    }

    #endregion

    #region Collaboration PropertyChanged Events

    [Fact]
    public void CollaborationRow_ChangeNumberOfMeetings_RaisesTotalHoursChanged()
    {
        var row = new CollaborationRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.NumberOfMeetings = 10;

        Assert.Contains(nameof(CollaborationRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void CollaborationRow_ChangeDuration_RaisesTotalHoursChanged()
    {
        var row = new CollaborationRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.MeetingDurationMinutes = 90;

        Assert.Contains(nameof(CollaborationRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void CollaborationRow_ChangeParticipants_RaisesTotalHoursChanged()
    {
        var row = new CollaborationRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.NumberOfParticipants = 5;

        Assert.Contains(nameof(CollaborationRowViewModel.TotalHours), raised);
    }

    [Fact]
    public void CollaborationRow_ChangePrepTime_RaisesTotalHoursChanged()
    {
        var row = new CollaborationRowViewModel();
        var raised = new List<string>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        row.ParticipantPrepTimeMinutes = 30;

        Assert.Contains(nameof(CollaborationRowViewModel.TotalHours), raised);
    }

    #endregion

    #region CollaborationRoleHours

    [Fact]
    public void CollaborationRoleHours_MatchesTotalCollaboration()
    {
        var vm = CreateVm();
        Assert.Equal(vm.TotalCollaborationHours, vm.CollaborationRoleHours);
    }

    [Fact]
    public void CollaborationRoleHours_UpdatesWhenCollaborationChanges()
    {
        var vm = CreateVm();
        vm.CollaborationItems[0].NumberOfMeetings = 20;
        Assert.Equal(vm.TotalCollaborationHours, vm.CollaborationRoleHours);
    }

    #endregion
}

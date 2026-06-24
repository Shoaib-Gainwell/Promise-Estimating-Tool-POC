namespace InitialEstimatePOC.Models;

/// <summary>
/// Represents a single component row in the Initial Estimate grid.
/// </summary>
public class ComponentEntry
{
    public int LineNumber { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public string Description { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public ComponentSize Size { get; set; }
    public int Count { get; set; } = 1;
    public decimal BaseHoursPerUnit { get; set; }
    public decimal TotalHours => BaseHoursPerUnit * Count;
}

public enum ComponentType
{
    PowerBuilderWindows,
    Reports,
    ProgramsDBStoredProcs,
    SupportModules,
    DBManipulation,
    DatabaseReview,
    Webpage,
    K2Workflow,
    K2SmartForm,
    TestAutomationUFT,
    MISC
}

public enum ChangeType
{
    New,
    Change
}

public enum ComponentSize
{
    Small,
    Medium,
    Large
}

public enum CollaborationType
{
    WPRs,
    ClientMeetings,
    InternalMeetings,
    AutomationTestCollaboration,
    ConsultantMentorEffort
}

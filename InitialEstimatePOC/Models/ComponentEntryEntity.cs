namespace InitialEstimatePOC.Models;

/// <summary>
/// Persisted component row. Schema mirrors Oracle DB structure.
/// Foreign key to PROJECT_ESTIMATES via PROJECT_ID.
/// </summary>
public class ComponentEntryEntity
{
    /// <summary>Oracle: COMPONENT_ID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY</summary>
    public int ComponentId { get; set; }

    /// <summary>Oracle: PROJECT_ID VARCHAR2(36) NOT NULL REFERENCES PROJECT_ESTIMATES</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Oracle: LINE_NUMBER NUMBER(5)</summary>
    public int LineNumber { get; set; }

    /// <summary>Oracle: REQUIREMENT_ID VARCHAR2(50)</summary>
    public string RequirementId { get; set; } = string.Empty;

    /// <summary>Oracle: COMPONENT_TYPE VARCHAR2(50) NOT NULL</summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>Oracle: DESCRIPTION VARCHAR2(500)</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Oracle: CHANGE_TYPE VARCHAR2(10) NOT NULL</summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>Oracle: COMPONENT_SIZE VARCHAR2(10) NOT NULL</summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>Oracle: COMPONENT_COUNT NUMBER(5) DEFAULT 1</summary>
    public int Count { get; set; } = 1;

    /// <summary>Oracle: BASE_HOURS_PER_UNIT NUMBER(10,4)</summary>
    public decimal BaseHoursPerUnit { get; set; }

    /// <summary>Oracle: TOTAL_HOURS NUMBER(10,2)</summary>
    public decimal TotalHours { get; set; }

    /// <summary>Oracle: NOTES VARCHAR2(1000)</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Navigation property back to parent project</summary>
    public ProjectEntity? Project { get; set; }
}

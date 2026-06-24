namespace InitialEstimatePOC.Models;

/// <summary>
/// Represents a saved project estimate. Schema mirrors Oracle DB structure.
/// PROJECT_ID is auto-generated GUID, PROJECT_NAME is unique.
/// </summary>
public class ProjectEntity
{
    /// <summary>Oracle: PROJECT_ID VARCHAR2(36) PRIMARY KEY</summary>
    public string ProjectId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Oracle: PROJECT_NAME VARCHAR2(200) UNIQUE NOT NULL</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Oracle: CHANGE_ORDER_ID VARCHAR2(100)</summary>
    public string ChangeOrderId { get; set; } = string.Empty;

    /// <summary>Oracle: PROJECT_DESCRIPTION VARCHAR2(500)</summary>
    public string ProjectDescription { get; set; } = string.Empty;

    /// <summary>Oracle: ESTIMATED_BY VARCHAR2(100)</summary>
    public string EstimatedBy { get; set; } = Environment.UserName;

    /// <summary>Oracle: REVIEWED_BY VARCHAR2(100)</summary>
    public string ReviewedBy { get; set; } = string.Empty;

    /// <summary>Oracle: PM_EFFORT_PERCENTAGE NUMBER(5,2)</summary>
    public decimal PmEffortPercentage { get; set; } = 15m;

    /// <summary>Oracle: PM_RESERVE_PERCENTAGE NUMBER(5,2)</summary>
    public decimal PmReservePercentage { get; set; } = 5m;

    /// <summary>Oracle: TOTAL_DEVELOPMENT_HOURS NUMBER(10,2)</summary>
    public decimal TotalDevelopmentHours { get; set; }

    /// <summary>Oracle: GRAND_TOTAL_HOURS NUMBER(10,2)</summary>
    public decimal GrandTotalHours { get; set; }

    /// <summary>Oracle: TSHIRT_SIZE VARCHAR2(20)</summary>
    public string TShirtSize { get; set; } = string.Empty;

    // === Collaboration Hours ===
    /// <summary>Oracle: COLLABORATION_HOURS NUMBER(10,2)</summary>
    public decimal CollaborationHours { get; set; }

    // === Adjusted Hours (per task type, Mid-Project Re-estimation) ===
    /// <summary>Oracle: SE_ADJUSTED_HOURS NUMBER(10,2) — kept for backward compat, maps to DevelopmentAdjustedHours</summary>
    public decimal SeAdjustedHours { get; set; }

    /// <summary>Oracle: BA_ADJUSTED_HOURS NUMBER(10,2) — kept for backward compat, maps to AnalysisAdjustedHours</summary>
    public decimal BaAdjustedHours { get; set; }

    /// <summary>Oracle: COLLABORATION_ADJUSTED_HOURS NUMBER(10,2)</summary>
    public decimal CollaborationAdjustedHours { get; set; }

    public decimal DevelopmentAdjustedHours { get; set; }
    public decimal AnalysisAdjustedHours { get; set; }
    public decimal BusinessDesignAdjustedHours { get; set; }
    public decimal SystemTestingAdjustedHours { get; set; }
    public decimal PromotionAdjustedHours { get; set; }
    public decimal BaSystemDocAdjustedHours { get; set; }
    public decimal ProductionValidationAdjustedHours { get; set; }
    public decimal ProjectManagementAdjustedHours { get; set; }

    // === Assumptions ===
    /// <summary>Oracle: SE_ASSUMPTIONS VARCHAR2(2000)</summary>
    public string SeAssumptions { get; set; } = string.Empty;

    /// <summary>Oracle: BA_ASSUMPTIONS VARCHAR2(2000)</summary>
    public string BaAssumptions { get; set; } = string.Empty;

    /// <summary>Oracle: COLLABORATION_ASSUMPTIONS VARCHAR2(2000)</summary>
    public string CollaborationAssumptions { get; set; } = string.Empty;

    /// <summary>Oracle: GENERAL_ASSUMPTIONS VARCHAR2(2000)</summary>
    public string GeneralAssumptions { get; set; } = string.Empty;

    /// <summary>Oracle: ADJUSTED_HOURS_COMMENTS VARCHAR2(4000)</summary>
    public string AdjustedHoursComments { get; set; } = string.Empty;

    // === Total Actual Hours ===
    public decimal TotalActualHours { get; set; }
    public DateTime? ActualHoursAsOfDate { get; set; }

    // === Time for Estimates ===
    public decimal TimeForEstimates { get; set; }

    // === Test Cases for System Testing ===
    public bool UseTestCasesForEstimate { get; set; }
    public int TestCasesSimple { get; set; }
    public int TestCasesMedium { get; set; }
    public int TestCasesComplex { get; set; }
    public int TestCasesVeryComplex { get; set; }
    public int TestCaseIterations { get; set; } = 1;

    /// <summary>Oracle: CREATED_DATE DATE</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Oracle: LAST_MODIFIED_DATE DATE</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Oracle: CREATED_BY VARCHAR2(100)</summary>
    public string CreatedBy { get; set; } = Environment.UserName;

    /// <summary>Oracle: VERSION_NUMBER NUMBER(5)</summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>Navigation property: one-to-many with COMPONENT_ENTRIES</summary>
    public List<ComponentEntryEntity> Components { get; set; } = new();

    /// <summary>Navigation property: one-to-many with COLLABORATION_ITEMS</summary>
    public List<CollaborationItemEntity> CollaborationItems { get; set; } = new();
}

/// <summary>
/// Collaboration item (meetings, WPRs, reviews) that feeds into Grand Total.
/// Matches Excel formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × NumParticipants
/// </summary>
public class CollaborationItemEntity
{
    public int ItemId { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string CollaborationType { get; set; } = string.Empty;
    public int NumberOfMeetings { get; set; } = 1;
    public int MeetingDurationMinutes { get; set; } = 60;
    public int NumberOfParticipants { get; set; } = 3;
    public int ParticipantPrepTimeMinutes { get; set; } = 15;
    public decimal TotalHours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public ProjectEntity? Project { get; set; }
}

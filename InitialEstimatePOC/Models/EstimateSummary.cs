namespace InitialEstimatePOC.Models;

/// <summary>
/// Holds all derived task calculations and summary for the Initial Estimate.
/// </summary>
public class EstimateSummary
{
    public decimal DevelopmentHours { get; set; }
    public decimal SystemTestingHours { get; set; }
    public decimal AnalysisHours { get; set; }
    public decimal BusinessDesignHours { get; set; }
    public decimal PromotionHours { get; set; }
    public decimal BASystemDocumentationHours { get; set; }
    public decimal ProductionValidationHours { get; set; }
    public decimal ProjectManagementHours { get; set; }
    public decimal GrandTotalHours { get; set; }
    public string TShirtSize { get; set; } = string.Empty;
    public int ComponentCount { get; set; }

    // Role Breakout
    public decimal BAHours { get; set; }
    public decimal SEHours { get; set; }
    public decimal TesterHours { get; set; }
    public decimal PMHours { get; set; }
}

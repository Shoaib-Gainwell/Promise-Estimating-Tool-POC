namespace InitialEstimatePOC.Models;

/// <summary>
/// Database entity for storing weighted values.
/// Managers can edit these via the Settings page.
/// </summary>
public class WeightedValueEntity
{
    public int Id { get; set; }
    public ComponentType ComponentType { get; set; }
    public ComponentSize Size { get; set; }
    public ChangeType ChangeType { get; set; }
    public decimal BaseHours { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = "System";
}

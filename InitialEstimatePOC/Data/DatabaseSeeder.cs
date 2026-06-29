using InitialEstimatePOC.Models;
using Microsoft.EntityFrameworkCore;

namespace InitialEstimatePOC.Data;

public static class DatabaseSeeder
{
    public static void Initialize(EstimateDbContext db)
    {
        db.Database.EnsureCreated();

        // Add new columns to existing databases if not present (each independently so a pre-existing column doesn't abort the rest)
        var newCols = new[]
        {
            ("WPRS_ADJUSTED_HOURS", "REAL DEFAULT 0"),
            ("CLIENT_MTG_ADJUSTED_HOURS", "REAL DEFAULT 0"),
            ("INTERNAL_MTG_ADJUSTED_HOURS", "REAL DEFAULT 0"),
            ("AUTO_TEST_ADJUSTED_HOURS", "REAL DEFAULT 0"),
            ("CONSULTANT_ADJUSTED_HOURS", "REAL DEFAULT 0"),
        };
        foreach (var (col, def) in newCols)
        {
            try { db.Database.ExecuteSqlRaw($"ALTER TABLE PROJECT_ESTIMATES ADD COLUMN {col} {def}"); }
            catch { /* column already exists — ignore */ }
        }

        if (db.WeightedValues.Any()) return;
        db.WeightedValues.AddRange(GetDefaultValues());
        db.SaveChanges();
    }

    private static List<WeightedValueEntity> GetDefaultValues()
    {
        var v = new List<WeightedValueEntity>();
        int id = 1;
        void A(ComponentType t, ComponentSize s, ChangeType c, decimal h) =>
            v.Add(new WeightedValueEntity { Id = id++, ComponentType = t, Size = s, ChangeType = c, BaseHours = h, LastModified = DateTime.UtcNow, ModifiedBy = "System" });

        A(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 25.00m);
        A(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 20.94m);
        A(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 75.00m);
        A(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 60.63m);
        A(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 125.00m);
        A(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 100.00m);
        A(ComponentType.Reports, ComponentSize.Small, ChangeType.New, 17.00m);
        A(ComponentType.Reports, ComponentSize.Small, ChangeType.Change, 13.60m);
        A(ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 51.00m);
        A(ComponentType.Reports, ComponentSize.Medium, ChangeType.Change, 40.80m);
        A(ComponentType.Reports, ComponentSize.Large, ChangeType.New, 85.00m);
        A(ComponentType.Reports, ComponentSize.Large, ChangeType.Change, 68.00m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.New, 46.00m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.Change, 36.80m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 115.00m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.Change, 92.00m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 294.40m);
        A(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change, 235.53m);
        A(ComponentType.SupportModules, ComponentSize.Small, ChangeType.New, 5.00m);
        A(ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 4.06m);
        A(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.New, 11.88m);
        A(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 9.69m);
        A(ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 26.88m);
        A(ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change, 21.56m);
        A(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.New, 5.94m);
        A(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change, 4.69m);
        A(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 15.00m);
        A(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.Change, 11.88m);
        A(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 31.88m);
        A(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 25.63m);
        A(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8.13m);
        A(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.Change, 6.10m);
        A(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.New, 8.13m);
        A(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.Change, 6.10m);
        A(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New, 8.13m);
        A(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.Change, 6.10m);
        A(ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 20.00m);
        A(ComponentType.Webpage, ComponentSize.Small, ChangeType.Change, 16.00m);
        A(ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 60.00m);
        A(ComponentType.Webpage, ComponentSize.Medium, ChangeType.Change, 48.00m);
        A(ComponentType.Webpage, ComponentSize.Large, ChangeType.New, 90.00m);
        A(ComponentType.Webpage, ComponentSize.Large, ChangeType.Change, 75.00m);
        A(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.New, 50.00m);
        A(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.Change, 35.00m);
        A(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 100.00m);
        A(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.Change, 80.00m);
        A(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 200.00m);
        A(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.Change, 150.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New, 15.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.Change, 10.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.New, 50.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 35.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.New, 90.00m);
        A(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 75.00m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.New, 3.00m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.Change, 5.00m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New, 8.00m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.Change, 1.00m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.New, 2.50m);
        A(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.Change, 5.00m);
        A(ComponentType.MISC, ComponentSize.Small, ChangeType.New, 20.00m);
        A(ComponentType.MISC, ComponentSize.Small, ChangeType.Change, 10.00m);
        A(ComponentType.MISC, ComponentSize.Medium, ChangeType.New, 50.00m);
        A(ComponentType.MISC, ComponentSize.Medium, ChangeType.Change, 25.00m);
        A(ComponentType.MISC, ComponentSize.Large, ChangeType.New, 100.00m);
        A(ComponentType.MISC, ComponentSize.Large, ChangeType.Change, 50.00m);
        return v;
    }
}

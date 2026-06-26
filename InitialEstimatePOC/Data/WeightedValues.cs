using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Data;

/// <summary>
/// Weighted values lookup — reads from SQLite database (editable via Settings).
/// Falls back to hardcoded defaults if DB is not available.
/// </summary>
public static class WeightedValues
{
    private static Dictionary<(ComponentType, ComponentSize, ChangeType), decimal> _matrix = new()
    {
        // PowerBuilder Windows
        { (ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New), 25.00m },
        { (ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change), 20.94m },
        { (ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New), 75.00m },
        { (ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change), 60.63m },
        { (ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New), 125.00m },
        { (ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change), 100.00m },

        // Reports
        { (ComponentType.Reports, ComponentSize.Small, ChangeType.New), 17.00m },
        { (ComponentType.Reports, ComponentSize.Small, ChangeType.Change), 13.60m },
        { (ComponentType.Reports, ComponentSize.Medium, ChangeType.New), 51.00m },
        { (ComponentType.Reports, ComponentSize.Medium, ChangeType.Change), 40.80m },
        { (ComponentType.Reports, ComponentSize.Large, ChangeType.New), 85.00m },
        { (ComponentType.Reports, ComponentSize.Large, ChangeType.Change), 68.00m },

        // Programs/DB Stored Procs
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.New), 46.00m },
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.Change), 36.80m },
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New), 115.00m },
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.Change), 92.00m },
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New), 294.40m },
        { (ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change), 235.525m },

        // Support Modules
        { (ComponentType.SupportModules, ComponentSize.Small, ChangeType.New), 5.00m },
        { (ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change), 4.0625m },
        { (ComponentType.SupportModules, ComponentSize.Medium, ChangeType.New), 11.875m },
        { (ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change), 9.6875m },
        { (ComponentType.SupportModules, ComponentSize.Large, ChangeType.New), 26.875m },
        { (ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change), 21.5625m },

        // DB Manipulation
        { (ComponentType.DBManipulation, ComponentSize.Small, ChangeType.New), 5.9375m },
        { (ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change), 4.6875m },
        { (ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New), 15.00m },
        { (ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.Change), 11.875m },
        { (ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New), 31.875m },
        { (ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change), 25.625m },

        // Database Review
        { (ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New), 8.125m },
        { (ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.Change), 6.10m },
        { (ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.New), 8.125m },
        { (ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.Change), 6.10m },
        { (ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New), 8.125m },
        { (ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.Change), 6.10m },

        // Webpage
        { (ComponentType.Webpage, ComponentSize.Small, ChangeType.New), 20.00m },
        { (ComponentType.Webpage, ComponentSize.Small, ChangeType.Change), 16.00m },
        { (ComponentType.Webpage, ComponentSize.Medium, ChangeType.New), 60.00m },
        { (ComponentType.Webpage, ComponentSize.Medium, ChangeType.Change), 48.00m },
        { (ComponentType.Webpage, ComponentSize.Large, ChangeType.New), 90.00m },
        { (ComponentType.Webpage, ComponentSize.Large, ChangeType.Change), 75.00m },

        // K2 Workflow
        { (ComponentType.K2Workflow, ComponentSize.Small, ChangeType.New), 50.00m },
        { (ComponentType.K2Workflow, ComponentSize.Small, ChangeType.Change), 35.00m },
        { (ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New), 100.00m },
        { (ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.Change), 80.00m },
        { (ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New), 200.00m },
        { (ComponentType.K2Workflow, ComponentSize.Large, ChangeType.Change), 150.00m },

        // K2 Smart Form
        { (ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New), 15.00m },
        { (ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.Change), 10.00m },
        { (ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.New), 50.00m },
        { (ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change), 35.00m },
        { (ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.New), 90.00m },
        { (ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change), 75.00m },

        // Test Automation (UFT)
        { (ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.New), 3.00m },
        { (ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.Change), 5.00m },
        { (ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New), 8.00m },
        { (ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.Change), 1.00m },
        { (ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.New), 2.50m },
        { (ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.Change), 5.00m },

        // MISC
        { (ComponentType.MISC, ComponentSize.Small, ChangeType.New), 20.00m },
        { (ComponentType.MISC, ComponentSize.Small, ChangeType.Change), 10.00m },
        { (ComponentType.MISC, ComponentSize.Medium, ChangeType.New), 50.00m },
        { (ComponentType.MISC, ComponentSize.Medium, ChangeType.Change), 25.00m },
        { (ComponentType.MISC, ComponentSize.Large, ChangeType.New), 100.00m },
        { (ComponentType.MISC, ComponentSize.Large, ChangeType.Change), 50.00m },
    };

    /// <summary>
    /// Get the base hours per unit for a given component type, size, and change type.
    /// </summary>
    public static decimal GetBaseHours(ComponentType componentType, ComponentSize size, ChangeType changeType)
    {
        return _matrix.TryGetValue((componentType, size, changeType), out var value) ? value : 0m;
    }

    /// <summary>
    /// Load all weighted values from the SQLite database into the in-memory cache.
    /// Called on app startup after database is seeded.
    /// </summary>
    public static void LoadFromDatabase(EstimateDbContext db)
    {
        var dbValues = db.WeightedValues.ToList();
        if (dbValues.Count == 0) return;

        var newMatrix = new Dictionary<(ComponentType, ComponentSize, ChangeType), decimal>();
        foreach (var v in dbValues)
        {
            newMatrix[(v.ComponentType, v.Size, v.ChangeType)] = v.BaseHours;
        }
        _matrix = newMatrix;
    }

    /// <summary>
    /// Update a single weighted value in the database and refresh the cache.
    /// Used by the Settings page when a manager edits a value.
    /// </summary>
    public static void UpdateValue(EstimateDbContext db, ComponentType type, ComponentSize size, ChangeType change, decimal newHours, string modifiedBy = "Manager")
    {
        var entity = db.WeightedValues
            .FirstOrDefault(v => v.ComponentType == type && v.Size == size && v.ChangeType == change);

        if (entity != null)
        {
            entity.BaseHours = newHours;
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            db.SaveChanges();
            _matrix[(type, size, change)] = newHours;
        }
    }

    /// <summary>
    /// Event raised when weighted values are updated (so UI can refresh).
    /// </summary>
    public static event Action? ValuesChanged;

    public static void NotifyValuesChanged() => ValuesChanged?.Invoke();

    /// <summary>
    /// Display-friendly names for component types.
    /// </summary>
    public static string GetDisplayName(ComponentType type) => type switch
    {
        ComponentType.PowerBuilderWindows => "PowerBuilder Windows",
        ComponentType.Reports => "Reports",
        ComponentType.ProgramsDBStoredProcs => "Programs/DB Stored Procedures",
        ComponentType.SupportModules => "Support Modules/JOB/JIL",
        ComponentType.DBManipulation => "DB Manipulation (SQL, PL/SQL, etc.)",
        ComponentType.DatabaseReview => "Database Review",
        ComponentType.Webpage => "Webpage (Includes UI, Portal & Intranet)",
        ComponentType.K2Workflow => "K2 Workflow",
        ComponentType.K2SmartForm => "K2 Smart Form",
        ComponentType.TestAutomationUFT => "Test Automation Suites (UFT)",
        ComponentType.MISC => "MISC (Server Setup, Webserver Setup, Software Installation, etc.)",
        _ => type.ToString()
    };

    /// <summary>
    /// Determine T-Shirt size from grand total hours.
    /// </summary>
    public static string GetTShirtSize(decimal grandTotal) => grandTotal switch
    {
        <= 0 => "—",
        < 100 => "Small",
        < 300 => "Medium",
        < 750 => "Large",
        < 1000 => "X-Large",
        < 2000 => "XL1",
        < 3000 => "XL2",
        < 4000 => "XL3",
        < 5000 => "XL4",
        < 6000 => "XL5",
        < 7000 => "XL6",
        < 8000 => "XL7",
        _ => "XL8"
    };
}

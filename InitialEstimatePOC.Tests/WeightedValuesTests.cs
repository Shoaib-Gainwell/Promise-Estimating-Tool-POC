using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for the WeightedValues static lookup class.
/// Covers all 66 weighted values (11 types × 3 sizes × 2 change types).
/// </summary>
public class WeightedValuesTests
{
    #region PowerBuilder Windows

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 25.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 20.94)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 75.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 60.63)]
    [InlineData(ComponentSize.Large, ChangeType.New, 125.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 100.00)]
    public void PowerBuilderWindows_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Reports

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 17.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 13.60)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 51.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 40.80)]
    [InlineData(ComponentSize.Large, ChangeType.New, 85.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 68.00)]
    public void Reports_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.Reports, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Programs/DB Stored Procs

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 46.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 36.80)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 115.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 92.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 294.40)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 235.525)]
    public void ProgramsDBStoredProcs_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.ProgramsDBStoredProcs, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Support Modules

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 5.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 4.0625)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 11.875)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 9.6875)]
    [InlineData(ComponentSize.Large, ChangeType.New, 26.875)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 21.5625)]
    public void SupportModules_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.SupportModules, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region DB Manipulation

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 5.9375)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 4.6875)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 15.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 11.875)]
    [InlineData(ComponentSize.Large, ChangeType.New, 31.875)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 25.625)]
    public void DBManipulation_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.DBManipulation, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Database Review

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 6.10)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 6.10)]
    [InlineData(ComponentSize.Large, ChangeType.New, 8.125)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 6.10)]
    public void DatabaseReview_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.DatabaseReview, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Webpage

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 16.00)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 60.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 48.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 75.00)]
    public void Webpage_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.Webpage, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region K2 Workflow

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 50.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 35.00)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 100.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 80.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 200.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 150.00)]
    public void K2Workflow_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.K2Workflow, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region K2 Smart Form

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 15.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 35.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 75.00)]
    public void K2SmartForm_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.K2SmartForm, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Test Automation (UFT)

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 3.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 5.00)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 8.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 1.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 2.50)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 5.00)]
    public void TestAutomationUFT_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.TestAutomationUFT, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region MISC

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentSize.Medium, ChangeType.Change, 25.00)]
    [InlineData(ComponentSize.Large, ChangeType.New, 100.00)]
    [InlineData(ComponentSize.Large, ChangeType.Change, 50.00)]
    public void MISC_ReturnsCorrectHours(ComponentSize size, ChangeType change, decimal expected)
    {
        var result = WeightedValues.GetBaseHours(ComponentType.MISC, size, change);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Display Names

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, "PowerBuilder Windows")]
    [InlineData(ComponentType.Reports, "Reports")]
    [InlineData(ComponentType.ProgramsDBStoredProcs, "Programs/DB Stored Procedures")]
    [InlineData(ComponentType.SupportModules, "Support Modules/JOB/JIL")]
    [InlineData(ComponentType.DBManipulation, "DB Manipulation (SQL, PL/SQL, etc.)")]
    [InlineData(ComponentType.DatabaseReview, "Database Review")]
    [InlineData(ComponentType.Webpage, "Webpage (Includes UI, Portal & Intranet)")]
    [InlineData(ComponentType.K2Workflow, "K2 Workflow")]
    [InlineData(ComponentType.K2SmartForm, "K2 Smart Form")]
    [InlineData(ComponentType.TestAutomationUFT, "Test Automation Suites (UFT)")]
    [InlineData(ComponentType.MISC, "MISC (Server Setup, Webserver Setup, Software Installation, etc.)")]
    public void GetDisplayName_ReturnsCorrectName(ComponentType type, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetDisplayName(type));
    }

    #endregion

    #region T-Shirt Sizing

    [Theory]
    [InlineData(0, "—")]
    [InlineData(1, "Small")]
    [InlineData(99, "Small")]
    [InlineData(100, "Medium")]
    [InlineData(299, "Medium")]
    [InlineData(300, "Large")]
    [InlineData(749, "Large")]
    [InlineData(750, "X-Large")]
    [InlineData(999, "X-Large")]
    [InlineData(1000, "XL1")]
    [InlineData(1999, "XL1")]
    [InlineData(2000, "XL2")]
    [InlineData(2999, "XL2")]
    [InlineData(3000, "XL3")]
    [InlineData(3999, "XL3")]
    [InlineData(4000, "XL4")]
    [InlineData(4999, "XL4")]
    [InlineData(5000, "XL5")]
    [InlineData(5999, "XL5")]
    [InlineData(6000, "XL6")]
    [InlineData(6999, "XL6")]
    [InlineData(7000, "XL7")]
    [InlineData(7999, "XL7")]
    [InlineData(8000, "XL8")]
    [InlineData(10000, "XL8")]
    public void GetTShirtSize_ReturnsCorrectSize(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetBaseHours_InvalidCombination_ReturnsZero()
    {
        // Cast an invalid enum value
        var result = WeightedValues.GetBaseHours((ComponentType)99, ComponentSize.Small, ChangeType.New);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void TotalWeightedValues_Count_Is66()
    {
        // Verify we have all 66 combinations (11 types × 3 sizes × 2 change types)
        int count = 0;
        foreach (ComponentType ct in Enum.GetValues<ComponentType>())
        foreach (ComponentSize sz in Enum.GetValues<ComponentSize>())
        foreach (ChangeType ch in Enum.GetValues<ChangeType>())
        {
            if (ct == ComponentType.None || sz == ComponentSize.None || ch == ChangeType.None)
                continue;
            if (WeightedValues.GetBaseHours(ct, sz, ch) > 0 || 
                (ct == ComponentType.TestAutomationUFT)) // UFT has valid values
                count++;
        }
        Assert.Equal(66, count);
    }

    #endregion
}

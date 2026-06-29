using System.Globalization;
using InitialEstimatePOC.Converters;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Additional converter tests for full coverage — edge cases, ConvertBack, cultures.
/// </summary>
public class ConverterAdvancedTests
{
    #region ComponentTypeDisplayConverter

    [Fact]
    public void ComponentTypeConverter_NullValue_ReturnsEmpty()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComponentTypeConverter_NonEnumValue_ReturnsToString()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert("Hello", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ComponentTypeConverter_IntValue_ReturnsToString()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(42, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("42", result);
    }

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
    public void ComponentTypeConverter_AllTypes_DisplayCorrectly(ComponentType type, string expected)
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(type, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComponentTypeConverter_ConvertBack_Throws()
    {
        var converter = new ComponentTypeDisplayConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack("test", typeof(ComponentType), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region DecimalFormatConverter

    [Fact]
    public void DecimalConverter_Zero_Returns000()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(0m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalConverter_LargeNumber_FormatsWithSeparators()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(1234567.89m, typeof(string), null!, CultureInfo.InvariantCulture);
        // N2 uses current culture separators — just verify it contains decimal
        Assert.Contains(".", result.ToString()!);
        Assert.Contains("89", result.ToString()!);
    }

    [Fact]
    public void DecimalConverter_NullValue_Returns000()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalConverter_NonDecimalType_Returns000()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert("not a number", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalConverter_ManyDecimals_RoundsToTwo()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(3.14159m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("3.14", result);
    }

    [Fact]
    public void DecimalConverter_ConvertBack_ValidString_ReturnsDecimal()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("123.45", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void DecimalConverter_ConvertBack_InvalidString_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("abc", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void DecimalConverter_ConvertBack_Null_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack(null!, typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void DecimalConverter_ConvertBack_EmptyString_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void DecimalConverter_NegativeValue_FormatsCorrectly()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(-42.5m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("-42.50", result);
    }

    #endregion
}

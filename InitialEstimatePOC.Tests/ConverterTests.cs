using InitialEstimatePOC.Converters;
using InitialEstimatePOC.Models;
using System.Globalization;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for WPF value converters.
/// </summary>
public class ConverterTests
{
    #region ComponentTypeDisplayConverter

    [Fact]
    public void ComponentTypeDisplayConverter_ConvertsPowerBuilder()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(ComponentType.PowerBuilderWindows, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("PowerBuilder Windows", result);
    }

    [Fact]
    public void ComponentTypeDisplayConverter_ConvertsMISC()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(ComponentType.MISC, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("MISC (Server Setup, Webserver Setup, Software Installation, etc.)", result);
    }

    [Fact]
    public void ComponentTypeDisplayConverter_NullValue_ReturnsEmpty()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComponentTypeDisplayConverter_ConvertBack_Throws()
    {
        var converter = new ComponentTypeDisplayConverter();
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack("test", typeof(ComponentType), null!, CultureInfo.InvariantCulture));
    }

    #endregion

    #region DecimalFormatConverter

    [Fact]
    public void DecimalFormatConverter_FormatsCorrectly()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(1234.56m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("1,234.56", result);
    }

    [Fact]
    public void DecimalFormatConverter_Zero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(0m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalFormatConverter_NonDecimal_ReturnsZero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert("not a number", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalFormatConverter_ConvertBack_ValidString()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("42.50", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(42.50m, result);
    }

    [Fact]
    public void DecimalFormatConverter_ConvertBack_InvalidString()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("abc", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    #endregion

    #region AdjustedHoursConverter — Convert (decimal → string)

    [Fact]
    public void AdjustedHoursConverter_Convert_Zero_ReturnsEmpty()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(0m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_PositiveInteger_NoTrailingZeros()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(10m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("10", result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_PositiveDecimal_TwoPlaces()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(10.5m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("10.5", result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_TwoDecimalPlaces()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(1.17m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("1.17", result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_Negative_ReturnsFormatted()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(-5.5m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("-5.5", result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_NonDecimal_ReturnsEmpty()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert("not a decimal", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void AdjustedHoursConverter_Convert_Null_ReturnsEmpty()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region AdjustedHoursConverter — ConvertBack (string → decimal)

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_EmptyString_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_Whitespace_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("   ", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_Dash_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("-", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_Dot_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack(".", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_ValidPositive_ReturnsDecimal()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("10.5", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(10.5m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_ValidNegative_ReturnsDecimal()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("-3.25", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(-3.25m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_Integer_ReturnsDecimal()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("42", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(42m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_InvalidText_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("abc", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_Null_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack(null!, typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_NonStringType_ReturnsZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack(42, typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHoursConverter_ConvertBack_LeadingTrailingSpaces_Parsed()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("  7.5  ", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(7.5m, result);
    }

    #endregion
}

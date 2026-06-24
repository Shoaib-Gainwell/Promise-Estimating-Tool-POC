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
        Assert.Equal("MISC", result);
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
}

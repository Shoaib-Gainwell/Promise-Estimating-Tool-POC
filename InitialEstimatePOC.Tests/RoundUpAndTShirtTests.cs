using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Direct unit tests for the static RoundUp method and T-shirt size boundaries.
/// Ensures Excel ROUNDUP(x, 2) semantics are exact.
/// </summary>
public class RoundUpAndTShirtTests
{
    #region RoundUp — Exact Values (no rounding needed)

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1.00, 1.00)]
    [InlineData(22.50, 22.50)]
    [InlineData(100.00, 100.00)]
    [InlineData(5.10, 5.10)]
    public void RoundUp_ExactValues_NoChange(decimal input, decimal expected)
    {
        Assert.Equal(expected, MainViewModel.RoundUp(input));
    }

    #endregion

    #region RoundUp — Positive Values (round up)

    [Theory]
    [InlineData(47.651, 47.66)]
    [InlineData(47.659, 47.66)]
    [InlineData(3.001, 3.01)]
    [InlineData(3.009, 3.01)]
    [InlineData(0.001, 0.01)]
    [InlineData(0.999, 1.00)]
    [InlineData(99.999, 100.00)]
    [InlineData(6.5050001, 6.51)]
    [InlineData(19.375, 19.38)]
    [InlineData(4.6875, 4.69)]
    public void RoundUp_PositiveValues_RoundsAwayFromZero(decimal input, decimal expected)
    {
        Assert.Equal(expected, MainViewModel.RoundUp(input));
    }

    #endregion

    #region RoundUp — Negative Values (round away from zero = more negative)

    [Theory]
    [InlineData(-47.651, -47.66)]
    [InlineData(-3.001, -3.01)]
    [InlineData(-0.001, -0.01)]
    public void RoundUp_NegativeValues_RoundsAwayFromZero(decimal input, decimal expected)
    {
        Assert.Equal(expected, MainViewModel.RoundUp(input));
    }

    #endregion

    #region RoundUp — Small Fractions

    [Theory]
    [InlineData(0.011, 0.02)]
    [InlineData(0.019, 0.02)]
    [InlineData(0.10, 0.10)]
    [InlineData(0.101, 0.11)]
    public void RoundUp_SmallFractions(decimal input, decimal expected)
    {
        Assert.Equal(expected, MainViewModel.RoundUp(input));
    }

    #endregion

    #region RoundUp — Documentation Worked Example Values

    [Fact]
    public void RoundUp_WorkedExample_SystemTesting()
    {
        // 953.10 * 0.30 = 285.93 (exact)
        Assert.Equal(285.93m, MainViewModel.RoundUp(953.10m * 0.30m));
    }

    [Fact]
    public void RoundUp_WorkedExample_Analysis()
    {
        // (953.10 + 285.93) * 0.05 = 61.9515
        Assert.Equal(61.96m, MainViewModel.RoundUp((953.10m + 285.93m) * 0.05m));
    }

    [Fact]
    public void RoundUp_WorkedExample_BusinessDesign()
    {
        // (953.10 + 285.93) * 0.15 = 185.8545
        Assert.Equal(185.86m, MainViewModel.RoundUp((953.10m + 285.93m) * 0.15m));
    }

    [Fact]
    public void RoundUp_WorkedExample_Promotion()
    {
        // 953.10 * 0.05 = 47.655
        Assert.Equal(47.66m, MainViewModel.RoundUp(953.10m * 0.05m));
    }

    [Fact]
    public void RoundUp_WorkedExample_BASystemDoc()
    {
        // 953.10 * 0.05 = 47.655
        Assert.Equal(47.66m, MainViewModel.RoundUp(953.10m * 0.05m));
    }

    [Fact]
    public void RoundUp_WorkedExample_ProductionValidation()
    {
        // 285.93 * 0.20 = 57.186
        Assert.Equal(57.19m, MainViewModel.RoundUp(285.93m * 0.20m));
    }

    [Fact]
    public void RoundUp_WorkedExample_PMEffort()
    {
        // Sum = 953.10+285.93+61.96+185.86+47.66+47.66+57.19 = 1639.36
        // 1639.36 * 0.15 = 245.904
        decimal sum = 953.10m + 285.93m + 61.96m + 185.86m + 47.66m + 47.66m + 57.19m;
        Assert.Equal(245.91m, MainViewModel.RoundUp(sum * 0.15m));
    }

    #endregion

    #region T-Shirt Size — All Boundaries

    [Theory]
    [InlineData(0, "—")]
    [InlineData(-1, "—")]
    [InlineData(-100, "—")]
    public void TShirtSize_ZeroOrNegative_ReturnsDash(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(0.01, "Small")]
    [InlineData(1, "Small")]
    [InlineData(50, "Small")]
    [InlineData(99, "Small")]
    [InlineData(99.99, "Small")]
    public void TShirtSize_Small_1to99(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(100, "Medium")]
    [InlineData(100.01, "Medium")]
    [InlineData(200, "Medium")]
    [InlineData(299, "Medium")]
    [InlineData(299.99, "Medium")]
    public void TShirtSize_Medium_100to299(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(300, "Large")]
    [InlineData(500, "Large")]
    [InlineData(749, "Large")]
    [InlineData(749.99, "Large")]
    public void TShirtSize_Large_300to749(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(750, "X-Large")]
    [InlineData(900, "X-Large")]
    [InlineData(999, "X-Large")]
    [InlineData(999.99, "X-Large")]
    public void TShirtSize_XLarge_750to999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(1000, "XL1")]
    [InlineData(1500, "XL1")]
    [InlineData(1999, "XL1")]
    [InlineData(1999.99, "XL1")]
    public void TShirtSize_XL1_1000to1999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(2000, "XL2")]
    [InlineData(2999.99, "XL2")]
    public void TShirtSize_XL2_2000to2999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(3000, "XL3")]
    [InlineData(3999.99, "XL3")]
    public void TShirtSize_XL3_3000to3999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(4000, "XL4")]
    [InlineData(4999.99, "XL4")]
    public void TShirtSize_XL4_4000to4999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(5000, "XL5")]
    [InlineData(5999.99, "XL5")]
    public void TShirtSize_XL5_5000to5999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(6000, "XL6")]
    [InlineData(6999.99, "XL6")]
    public void TShirtSize_XL6_6000to6999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(7000, "XL7")]
    [InlineData(7999.99, "XL7")]
    public void TShirtSize_XL7_7000to7999(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    [Theory]
    [InlineData(8000, "XL8")]
    [InlineData(10000, "XL8")]
    [InlineData(100000, "XL8")]
    public void TShirtSize_XL8_8000Plus(decimal hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    #endregion

    #region T-Shirt Size — Integration with ViewModel

    [Fact]
    public void TShirtSize_EmptyProject_WithCollaboration_Small()
    {
        // No components → T-shirt shows "—" regardless of collaboration
        var vm = new MainViewModel();
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void TShirtSize_UpdatesWhenComponentsChange()
    {
        var vm = new MainViewModel();
        Assert.Equal("—", vm.TShirtSize);

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.ProgramsDBStoredProcs;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 5;

        // 294.40 * 5 = 1472 dev + derived tasks → Grand Total ~2912 → XL2
        Assert.Equal("XL2", vm.TShirtSize);
    }

    #endregion
}

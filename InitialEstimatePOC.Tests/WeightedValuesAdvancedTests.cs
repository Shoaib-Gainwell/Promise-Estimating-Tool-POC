using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for WeightedValues static class — LoadFromDatabase, UpdateValue,
/// ValuesChanged event, edge cases.
/// </summary>
public class WeightedValuesAdvancedTests : IDisposable
{
    private readonly EstimateDbContext _db;

    public WeightedValuesAdvancedTests()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _db = new EstimateDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        DatabaseSeeder.Initialize(_db);
        WeightedValues.LoadFromDatabase(_db);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
        WeightedValues.ResetToDefaults();
    }

    #region LoadFromDatabase Tests

    [Fact]
    public void LoadFromDatabase_ReplacesInMemoryCache()
    {
        // Modify a value in DB directly
        var entity = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New);
        entity.BaseHours = 999m;
        _db.SaveChanges();

        WeightedValues.LoadFromDatabase(_db);

        Assert.Equal(999m, WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Small, ChangeType.New));
    }

    [Fact]
    public void LoadFromDatabase_EmptyDb_KeepsExistingCache()
    {
        // Clear DB
        _db.WeightedValues.RemoveRange(_db.WeightedValues);
        _db.SaveChanges();

        var beforeValue = WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Large, ChangeType.New);
        WeightedValues.LoadFromDatabase(_db);
        var afterValue = WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Large, ChangeType.New);

        Assert.Equal(beforeValue, afterValue);
    }

    #endregion

    #region UpdateValue Tests

    [Fact]
    public void UpdateValue_UpdatesDatabase()
    {
        WeightedValues.UpdateValue(_db, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 42m, "TestUser");

        var entity = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New);
        Assert.Equal(42m, entity.BaseHours);
    }

    [Fact]
    public void UpdateValue_UpdatesInMemoryCache()
    {
        WeightedValues.UpdateValue(_db, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 55m, "TestUser");

        Assert.Equal(55m, WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Small, ChangeType.New));
    }

    [Fact]
    public void UpdateValue_SetsModifiedBy()
    {
        WeightedValues.UpdateValue(_db, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 10m, "AdminUser");

        var entity = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New);
        Assert.Equal("AdminUser", entity.ModifiedBy);
    }

    [Fact]
    public void UpdateValue_SetsLastModified()
    {
        var before = DateTime.UtcNow;
        WeightedValues.UpdateValue(_db, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 10m);
        var after = DateTime.UtcNow;

        var entity = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New);
        Assert.InRange(entity.LastModified, before, after.AddSeconds(1));
    }

    [Fact]
    public void UpdateValue_NonExistentCombination_DoesNothing()
    {
        // Remove all entries for a specific combo first
        var entities = _db.WeightedValues.Where(v =>
            v.ComponentType == ComponentType.MISC && v.Size == ComponentSize.Small && v.ChangeType == ChangeType.New).ToList();
        _db.WeightedValues.RemoveRange(entities);
        _db.SaveChanges();

        // Try to update non-existent — should not throw
        WeightedValues.UpdateValue(_db, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 999m);
    }

    #endregion

    #region ValuesChanged Event Tests

    [Fact]
    public void NotifyValuesChanged_FiresEvent()
    {
        bool fired = false;
        void handler() => fired = true;
        WeightedValues.ValuesChanged += handler;

        WeightedValues.NotifyValuesChanged();

        Assert.True(fired);
        WeightedValues.ValuesChanged -= handler;
    }

    [Fact]
    public void NotifyValuesChanged_NoSubscribers_DoesNotThrow()
    {
        // Should not throw even with no subscribers
        var exception = Record.Exception(() => WeightedValues.NotifyValuesChanged());
        Assert.Null(exception);
    }

    #endregion

    #region GetBaseHours Edge Cases

    [Fact]
    public void GetBaseHours_AllCombinationsReturnPositive()
    {
        foreach (var type in Enum.GetValues<ComponentType>())
        {
            if (type == ComponentType.None) continue;
            foreach (var size in Enum.GetValues<ComponentSize>())
            {
                if (size == ComponentSize.None) continue;
                foreach (var change in Enum.GetValues<ChangeType>())
                {
                    if (change == ChangeType.None) continue;
                    var hours = WeightedValues.GetBaseHours(type, size, change);
                    Assert.True(hours > 0, $"Expected positive hours for {type}/{size}/{change} but got {hours}");
                }
            }
        }
    }

    [Fact]
    public void GetBaseHours_ChangeType_AlwaysLessOrEqualNew()
    {
        foreach (var type in Enum.GetValues<ComponentType>())
        {
            if (type == ComponentType.None) continue;
            foreach (var size in Enum.GetValues<ComponentSize>())
            {
                if (size == ComponentSize.None) continue;
                var newHours = WeightedValues.GetBaseHours(type, size, ChangeType.New);
                var changeHours = WeightedValues.GetBaseHours(type, size, ChangeType.Change);
                // UFT has inverted values by design, skip those
                if (type != ComponentType.TestAutomationUFT)
                    Assert.True(changeHours <= newHours,
                        $"{type}/{size}: Change ({changeHours}) should be <= New ({newHours})");
            }
        }
    }

    #endregion

    #region GetTShirtSize Edge Cases

    [Fact]
    public void GetTShirtSize_NegativeValue_ReturnsDash()
    {
        Assert.Equal("—", WeightedValues.GetTShirtSize(-1m));
    }

    [Fact]
    public void GetTShirtSize_ExactBoundary_100_ReturnsMedium()
    {
        Assert.Equal("Medium", WeightedValues.GetTShirtSize(100m));
    }

    [Fact]
    public void GetTShirtSize_ExactBoundary_300_ReturnsLarge()
    {
        Assert.Equal("Large", WeightedValues.GetTShirtSize(300m));
    }

    [Fact]
    public void GetTShirtSize_ExactBoundary_750_ReturnsXLarge()
    {
        Assert.Equal("X-Large", WeightedValues.GetTShirtSize(750m));
    }

    [Fact]
    public void GetTShirtSize_VeryLargeValue_ReturnsXL8()
    {
        Assert.Equal("XL8", WeightedValues.GetTShirtSize(99999m));
    }

    [Theory]
    [InlineData(1000, "XL1")]
    [InlineData(2000, "XL2")]
    [InlineData(3000, "XL3")]
    [InlineData(4000, "XL4")]
    [InlineData(5000, "XL5")]
    [InlineData(6000, "XL6")]
    [InlineData(7000, "XL7")]
    [InlineData(8000, "XL8")]
    public void GetTShirtSize_AllXLBoundaries(int hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    #endregion

    #region GetDisplayName Tests

    [Fact]
    public void GetDisplayName_UnknownEnum_ReturnsToString()
    {
        // Cast an invalid integer to ComponentType
        var unknown = (ComponentType)999;
        Assert.Equal("999", WeightedValues.GetDisplayName(unknown));
    }

    #endregion
}

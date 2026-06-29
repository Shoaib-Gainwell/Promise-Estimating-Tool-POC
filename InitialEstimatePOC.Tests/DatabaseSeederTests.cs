using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for DatabaseSeeder — ensures all 66 weighted values are seeded correctly
/// and that idempotent initialization works.
/// </summary>
public class DatabaseSeederTests : IDisposable
{
    private readonly EstimateDbContext _db;

    public DatabaseSeederTests()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _db = new EstimateDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public void Initialize_EmptyDb_Seeds66Values()
    {
        DatabaseSeeder.Initialize(_db);

        Assert.Equal(66, _db.WeightedValues.Count());
    }

    [Fact]
    public void Initialize_CalledTwice_DoesNotDuplicate()
    {
        DatabaseSeeder.Initialize(_db);
        DatabaseSeeder.Initialize(_db);

        Assert.Equal(66, _db.WeightedValues.Count());
    }

    [Fact]
    public void Initialize_ExistingData_SkipsSeeding()
    {
        // Add a single record
        _db.WeightedValues.Add(new WeightedValueEntity
        {
            Id = 999,
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            BaseHours = 1.0m
        });
        _db.SaveChanges();

        DatabaseSeeder.Initialize(_db);

        // Should only have the 1 record, not 66
        Assert.Equal(1, _db.WeightedValues.Count());
    }

    [Fact]
    public void Initialize_AllComponentTypesCovered()
    {
        DatabaseSeeder.Initialize(_db);

        var types = _db.WeightedValues.Select(v => v.ComponentType).Distinct().ToList();
        Assert.Equal(11, types.Count);
        foreach (var enumVal in Enum.GetValues<ComponentType>().Where(v => v != ComponentType.None))
            Assert.Contains(enumVal, types);
    }

    [Fact]
    public void Initialize_AllSizesCovered()
    {
        DatabaseSeeder.Initialize(_db);

        var sizes = _db.WeightedValues.Select(v => v.Size).Distinct().ToList();
        Assert.Equal(3, sizes.Count);
        Assert.Contains(ComponentSize.Small, sizes);
        Assert.Contains(ComponentSize.Medium, sizes);
        Assert.Contains(ComponentSize.Large, sizes);
    }

    [Fact]
    public void Initialize_BothChangeTypesCovered()
    {
        DatabaseSeeder.Initialize(_db);

        var changes = _db.WeightedValues.Select(v => v.ChangeType).Distinct().ToList();
        Assert.Equal(2, changes.Count);
        Assert.Contains(ChangeType.New, changes);
        Assert.Contains(ChangeType.Change, changes);
    }

    [Fact]
    public void Initialize_AllHoursPositive()
    {
        DatabaseSeeder.Initialize(_db);

        Assert.All(_db.WeightedValues, v => Assert.True(v.BaseHours > 0));
    }

    [Fact]
    public void Initialize_SetsModifiedByToSystem()
    {
        DatabaseSeeder.Initialize(_db);

        Assert.All(_db.WeightedValues, v => Assert.Equal("System", v.ModifiedBy));
    }

    [Fact]
    public void Initialize_SetsLastModifiedDate()
    {
        var before = DateTime.UtcNow;
        DatabaseSeeder.Initialize(_db);
        var after = DateTime.UtcNow;

        Assert.All(_db.WeightedValues, v =>
        {
            Assert.InRange(v.LastModified, before.AddSeconds(-1), after.AddSeconds(1));
        });
    }

    [Fact]
    public void Initialize_SequentialIds()
    {
        DatabaseSeeder.Initialize(_db);

        var ids = _db.WeightedValues.Select(v => v.Id).OrderBy(x => x).ToList();
        for (int i = 0; i < ids.Count; i++)
            Assert.Equal(i + 1, ids[i]);
    }

    [Fact]
    public void Initialize_SpecificValue_PowerBuilderSmallNew()
    {
        DatabaseSeeder.Initialize(_db);

        var val = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.PowerBuilderWindows &&
            v.Size == ComponentSize.Small &&
            v.ChangeType == ChangeType.New);

        Assert.Equal(25.00m, val.BaseHours);
    }

    [Fact]
    public void Initialize_SpecificValue_MISCLargeNew()
    {
        DatabaseSeeder.Initialize(_db);

        var val = _db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.MISC &&
            v.Size == ComponentSize.Large &&
            v.ChangeType == ChangeType.New);

        Assert.Equal(100.00m, val.BaseHours);
    }

    [Fact]
    public void Initialize_EnsuresCreatedCalledBeforeQuery()
    {
        // Using a fresh in-memory DB, this should not throw
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var freshDb = new EstimateDbContext(options);
        freshDb.Database.OpenConnection();

        // EnsureCreated is called inside Initialize
        DatabaseSeeder.Initialize(freshDb);
        Assert.Equal(66, freshDb.WeightedValues.Count());

        freshDb.Database.CloseConnection();
    }
}

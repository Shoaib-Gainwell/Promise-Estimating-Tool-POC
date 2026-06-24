using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for EstimateDbContext — schema creation, constraints, relationships.
/// </summary>
public class EstimateDbContextTests : IDisposable
{
    private readonly EstimateDbContext _db;

    public EstimateDbContextTests()
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

    #region Table Creation

    [Fact]
    public void EnsureCreated_CreatesWeightedValuesTable()
    {
        var tables = GetTableNames();
        Assert.Contains("WeightedValues", tables);
    }

    [Fact]
    public void EnsureCreated_CreatesProjectEstimatesTable()
    {
        var tables = GetTableNames();
        Assert.Contains("PROJECT_ESTIMATES", tables);
    }

    [Fact]
    public void EnsureCreated_CreatesComponentEntriesTable()
    {
        var tables = GetTableNames();
        Assert.Contains("COMPONENT_ENTRIES", tables);
    }

    #endregion

    #region WeightedValues Unique Constraint

    [Fact]
    public void WeightedValues_UniqueConstraint_DuplicateThrows()
    {
        _db.WeightedValues.Add(new WeightedValueEntity
        {
            Id = 1, ComponentType = ComponentType.MISC, Size = ComponentSize.Small,
            ChangeType = ChangeType.New, BaseHours = 10m
        });
        _db.SaveChanges();

        _db.WeightedValues.Add(new WeightedValueEntity
        {
            Id = 2, ComponentType = ComponentType.MISC, Size = ComponentSize.Small,
            ChangeType = ChangeType.New, BaseHours = 20m
        });

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    #endregion

    #region ProjectEntity Constraints

    [Fact]
    public void Project_UniqueProjectName_EnforcedByDb()
    {
        _db.Projects.Add(new ProjectEntity { ProjectId = "id1", ProjectName = "Same Name" });
        _db.SaveChanges();

        _db.Projects.Add(new ProjectEntity { ProjectId = "id2", ProjectName = "Same Name" });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void Project_ProjectNameRequired()
    {
        _db.Projects.Add(new ProjectEntity { ProjectId = "id1", ProjectName = null! });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void Project_CanSaveAndRetrieve()
    {
        var project = new ProjectEntity
        {
            ProjectName = "Test Project",
            ChangeOrderId = "CO-001",
            ProjectDescription = "Description",
            PmEffortPercentage = 15m,
            TotalDevelopmentHours = 100m,
            GrandTotalHours = 200m,
            TShirtSize = "Medium"
        };
        _db.Projects.Add(project);
        _db.SaveChanges();

        var loaded = _db.Projects.First(p => p.ProjectName == "Test Project");
        Assert.Equal("CO-001", loaded.ChangeOrderId);
        Assert.Equal(15m, loaded.PmEffortPercentage);
        Assert.Equal(200m, loaded.GrandTotalHours);
    }

    #endregion

    #region ComponentEntry Constraints

    [Fact]
    public void ComponentEntry_RequiresProjectId()
    {
        _db.ComponentEntries.Add(new ComponentEntryEntity
        {
            ProjectId = null!,
            ComponentType = "MISC",
            ChangeType = "New",
            Size = "Small"
        });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    [Fact]
    public void ComponentEntry_RequiresComponentType()
    {
        _db.Projects.Add(new ProjectEntity { ProjectId = "p1", ProjectName = "P1" });
        _db.SaveChanges();

        _db.ComponentEntries.Add(new ComponentEntryEntity
        {
            ProjectId = "p1",
            ComponentType = null!,
            ChangeType = "New",
            Size = "Small"
        });
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }

    #endregion

    #region Cascade Delete

    [Fact]
    public void CascadeDelete_RemovingProject_DeletesComponents()
    {
        var project = new ProjectEntity { ProjectId = "cascade1", ProjectName = "Cascade Test" };
        project.Components.Add(new ComponentEntryEntity
        {
            ProjectId = "cascade1", ComponentType = "MISC", ChangeType = "New", Size = "Small"
        });
        project.Components.Add(new ComponentEntryEntity
        {
            ProjectId = "cascade1", ComponentType = "Reports", ChangeType = "Change", Size = "Large"
        });
        _db.Projects.Add(project);
        _db.SaveChanges();

        _db.Projects.Remove(project);
        _db.SaveChanges();

        Assert.Empty(_db.ComponentEntries.Where(c => c.ProjectId == "cascade1"));
    }

    #endregion

    #region Relationships

    [Fact]
    public void Project_NavigationProperty_LoadsComponents()
    {
        var project = new ProjectEntity { ProjectId = "nav1", ProjectName = "Nav Test" };
        project.Components.Add(new ComponentEntryEntity
        {
            ProjectId = "nav1", ComponentType = "MISC", ChangeType = "New", Size = "Large"
        });
        _db.Projects.Add(project);
        _db.SaveChanges();

        var loaded = _db.Projects.Include(p => p.Components).First(p => p.ProjectId == "nav1");
        Assert.Single(loaded.Components);
        Assert.Equal("MISC", loaded.Components[0].ComponentType);
    }

    [Fact]
    public void ComponentEntry_AutoIncrementId()
    {
        var project = new ProjectEntity { ProjectId = "auto1", ProjectName = "Auto ID" };
        project.Components.Add(new ComponentEntryEntity
        {
            ProjectId = "auto1", ComponentType = "MISC", ChangeType = "New", Size = "Small"
        });
        project.Components.Add(new ComponentEntryEntity
        {
            ProjectId = "auto1", ComponentType = "Reports", ChangeType = "New", Size = "Large"
        });
        _db.Projects.Add(project);
        _db.SaveChanges();

        var components = _db.ComponentEntries.Where(c => c.ProjectId == "auto1").OrderBy(c => c.ComponentId).ToList();
        Assert.True(components[1].ComponentId > components[0].ComponentId);
    }

    #endregion

    private List<string> GetTableNames()
    {
        var conn = _db.Database.GetDbConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'";
        using var reader = cmd.ExecuteReader();
        var tables = new List<string>();
        while (reader.Read())
            tables.Add(reader.GetString(0));
        return tables;
    }
}

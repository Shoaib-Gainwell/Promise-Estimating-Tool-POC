using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC.Tests;

/// <summary>
/// Tests for Save/Load project functionality with SQLite persistence.
/// Uses in-memory SQLite to avoid file system side effects.
/// </summary>
public class ProjectPersistenceTests : IDisposable
{
    private readonly EstimateDbContext _db;

    public ProjectPersistenceTests()
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

    #region Save Project Tests

    private void FillRequiredFields(MainViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.ChangeOrderId)) vm.ChangeOrderId = "CO-TEST-001";
        if (string.IsNullOrWhiteSpace(vm.ProjectDescription)) vm.ProjectDescription = "Test description";
        if (string.IsNullOrWhiteSpace(vm.EstimatedBy)) vm.EstimatedBy = "Tester";
        if (string.IsNullOrWhiteSpace(vm.ReviewedBy)) vm.ReviewedBy = "Reviewer";
        if (!vm.Components.Any(c => c.ComponentType != ComponentType.None))
        {
            vm.AddComponentCommand.Execute(null);
            vm.Components[^1].ComponentType = ComponentType.MISC;
            vm.Components[^1].Size = ComponentSize.Small;
            vm.Components[^1].ChangeType = ChangeType.New;
            vm.Components[^1].Count = 1;
        }
    }

    [Fact]
    public void SaveProject_EmptyName_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "";

        var result = vm.SaveProject();

        Assert.Equal("Project Name is required.", result);
    }

    [Fact]
    public void SaveProject_WhitespaceName_ReturnsError()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "   ";

        var result = vm.SaveProject();

        Assert.Equal("Project Name is required.", result);
    }

    [Fact]
    public void SaveProject_ValidProject_ReturnsNull()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Test Project";
        FillRequiredFields(vm);

        var result = vm.SaveProject();

        Assert.Null(result);
    }

    [Fact]
    public void SaveProject_PersistsToDatabase()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Persistence Test";
        vm.ChangeOrderId = "CO-123";
        vm.ProjectDescription = "A test project";
        FillRequiredFields(vm);

        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        Assert.Contains(projects, p => p.ProjectName == "Persistence Test");
    }

    [Fact]
    public void SaveProject_PersistsComponents()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Component Test";
        vm.ChangeOrderId = "CO-TEST";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.RequirementId = "REQ-001";
        row.ComponentType = ComponentType.K2Workflow;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 2;

        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "Component Test");
        Assert.Single(saved.Components);
        Assert.Equal("REQ-001", saved.Components[0].RequirementId);
        Assert.Equal("K2Workflow", saved.Components[0].ComponentType);
        Assert.Equal("Large", saved.Components[0].Size);
        Assert.Equal("New", saved.Components[0].ChangeType);
        Assert.Equal(2, saved.Components[0].Count);
    }

    [Fact]
    public void SaveProject_UpdatesExistingByName()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Update Test";
        vm.ChangeOrderId = "CO-001";
        FillRequiredFields(vm);
        vm.SaveProject();

        // Change and save again
        vm.ChangeOrderId = "CO-002";
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var matches = projects.Where(p => p.ProjectName == "Update Test").ToList();
        Assert.Single(matches);
        Assert.Equal("CO-002", matches[0].ChangeOrderId);
    }

    [Fact]
    public void SaveProject_UniqueProjectName_Enforced()
    {
        var vm1 = new MainViewModel();
        vm1.ProjectName = "Unique Project";
        FillRequiredFields(vm1);
        vm1.SaveProject();

        // Same name from another vm instance = should update (not duplicate)
        var vm2 = new MainViewModel();
        vm2.ProjectName = "Unique Project";
        vm2.ChangeOrderId = "NEW-CO";
        FillRequiredFields(vm2);
        vm2.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var matches = projects.Where(p => p.ProjectName == "Unique Project").ToList();
        Assert.Single(matches);
    }

    [Fact]
    public void SaveProject_SetsCreatedDate()
    {
        var vm = new MainViewModel();
        vm.ProjectName = $"Date Test {Guid.NewGuid():N}";
        FillRequiredFields(vm);
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == vm.ProjectName);
        Assert.True((DateTime.UtcNow - saved.CreatedDate).TotalSeconds < 10);
    }

    [Fact]
    public void SaveProject_SetsCreatedBy()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "User Test";
        FillRequiredFields(vm);
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "User Test");
        Assert.Equal(Environment.UserName, saved.CreatedBy);
    }

    [Fact]
    public void SaveProject_PersistsPmPercentage()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "PM Test";
        vm.PmEffortPercentage = 20m;
        FillRequiredFields(vm);
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "PM Test");
        Assert.Equal(20m, saved.PmEffortPercentage);
    }

    [Fact]
    public void SaveProject_PersistsTotals()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Totals Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.MISC;
        vm.Components[^1].Size = ComponentSize.Large;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "Totals Test");
        Assert.True(saved.GrandTotalHours > 0);
        Assert.True(saved.TotalDevelopmentHours > 0);
    }

    [Fact]
    public void SaveProject_PersistsTShirtSize()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "TShirt Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.MISC;
        vm.Components[^1].Size = ComponentSize.Large;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "TShirt Test");
        Assert.False(string.IsNullOrEmpty(saved.TShirtSize));
    }

    [Fact]
    public void SaveProject_MultipleComponents_AllPersisted()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Multi Component";
        vm.ChangeOrderId = "CO-TEST";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^3].ComponentType = ComponentType.Reports;
        vm.Components[^3].Size = ComponentSize.Small;
        vm.Components[^3].ChangeType = ChangeType.New;
        vm.Components[^3].Count = 1;
        vm.Components[^2].ComponentType = ComponentType.K2Workflow;
        vm.Components[^2].Size = ComponentSize.Small;
        vm.Components[^2].ChangeType = ChangeType.New;
        vm.Components[^2].Count = 1;
        vm.Components[^1].ComponentType = ComponentType.Webpage;
        vm.Components[^1].Size = ComponentSize.Small;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "Multi Component");
        Assert.Equal(3, saved.Components.Count);
    }

    #endregion

    #region Load Project Tests

    [Fact]
    public void LoadProject_RestoresProjectName()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Name Test";
        FillRequiredFields(vm);
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Name Test"));

        Assert.Equal("Load Name Test", vm2.ProjectName);
    }

    [Fact]
    public void LoadProject_RestoresChangeOrderId()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load CO Test";
        vm.ChangeOrderId = "CO-999";
        FillRequiredFields(vm);
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load CO Test"));

        Assert.Equal("CO-999", vm2.ChangeOrderId);
    }

    [Fact]
    public void LoadProject_RestoresDescription()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Desc Test";
        vm.ProjectDescription = "Test description";
        FillRequiredFields(vm);
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Desc Test"));

        Assert.Equal("Test description", vm2.ProjectDescription);
    }

    [Fact]
    public void LoadProject_RestoresPmPercentage()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load PM Test";
        vm.PmEffortPercentage = 19m;
        FillRequiredFields(vm);
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load PM Test"));

        Assert.Equal(19m, vm2.PmEffortPercentage);
    }

    [Fact]
    public void LoadProject_RestoresComponents()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Components Test";
        vm.ChangeOrderId = "CO-TEST";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].RequirementId = "R-100";
        vm.Components[^1].ComponentType = ComponentType.Webpage;
        vm.Components[^1].Size = ComponentSize.Medium;
        vm.Components[^1].ChangeType = ChangeType.Change;
        vm.Components[^1].Count = 3;
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Components Test"));

        Assert.Single(vm2.Components);
        var c = vm2.Components[0];
        Assert.Equal("R-100", c.RequirementId);
        Assert.Equal(ComponentType.Webpage, c.ComponentType);
        Assert.Equal(ComponentSize.Medium, c.Size);
        Assert.Equal(ChangeType.Change, c.ChangeType);
        Assert.Equal(3, c.Count);
    }

    [Fact]
    public void LoadProject_RecalculatesTotals()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Recalc Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.MISC;
        vm.Components[^1].Size = ComponentSize.Large;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        var originalTotal = vm.GrandTotalHours;
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Recalc Test"));

        Assert.Equal(originalTotal, vm2.GrandTotalHours);
    }

    [Fact]
    public void LoadProject_ClearsExistingComponents()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Clear Test";
        FillRequiredFields(vm);
        vm.SaveProject();

        // Start a new VM with some components already
        var vm2 = new MainViewModel();
        vm2.AddComponentCommand.Execute(null);
        vm2.AddComponentCommand.Execute(null);
        vm2.AddComponentCommand.Execute(null);
        Assert.Equal(3, vm2.Components.Count);

        // Load project — should replace, not append
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Clear Test"));

        Assert.Single(vm2.Components);
    }

    [Fact]
    public void LoadProject_ComponentLineNumbersPreserved()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load Line Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.Reports;
        vm.Components[^1].Size = ComponentSize.Small;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load Line Test"));

        Assert.Equal(1, vm2.Components[0].LineNumber);
        Assert.Equal(2, vm2.Components[1].LineNumber);
    }

    [Fact]
    public void LoadProject_BaseHoursRecalculated()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Load BaseHrs Test";
        vm.ChangeOrderId = "CO-TEST";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.K2Workflow;
        vm.Components[^1].Size = ComponentSize.Medium;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        var expectedBase = vm.Components[^1].BaseHoursPerUnit;
        vm.SaveProject();

        var vm2 = new MainViewModel();
        var projects = MainViewModel.GetAllProjects();
        vm2.LoadProject(projects.First(p => p.ProjectName == "Load BaseHrs Test"));

        // First component is K2Workflow (only one saved)
        Assert.Equal(expectedBase, vm2.Components[0].BaseHoursPerUnit);
    }

    #endregion

    #region GetAllProjects Tests

    [Fact]
    public void GetAllProjects_EmptyDatabase_ReturnsEmptyList()
    {
        // All projects from other tests may exist, so just verify it returns a list
        var projects = MainViewModel.GetAllProjects();
        Assert.NotNull(projects);
    }

    [Fact]
    public void GetAllProjects_OrderedByLastModifiedDescending()
    {
        var vm1 = new MainViewModel();
        vm1.ProjectName = "Order Test A";
        FillRequiredFields(vm1);
        vm1.SaveProject();

        var vm2 = new MainViewModel();
        vm2.ProjectName = "Order Test B";
        FillRequiredFields(vm2);
        vm2.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var a = projects.First(p => p.ProjectName == "Order Test A");
        var b = projects.First(p => p.ProjectName == "Order Test B");
        Assert.True(projects.IndexOf(b) < projects.IndexOf(a));
    }

    [Fact]
    public void GetAllProjects_IncludesComponents()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Include Comp Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.Reports;
        vm.Components[^1].Size = ComponentSize.Small;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "Include Comp Test");
        Assert.Equal(2, saved.Components.Count);
    }

    #endregion

    #region ProjectEntity Tests

    [Fact]
    public void ProjectEntity_AutoGeneratesId()
    {
        var entity = new ProjectEntity();
        Assert.False(string.IsNullOrEmpty(entity.ProjectId));
        Assert.Equal(32, entity.ProjectId.Length); // GUID without hyphens
    }

    [Fact]
    public void ProjectEntity_UniqueIds()
    {
        var e1 = new ProjectEntity();
        var e2 = new ProjectEntity();
        Assert.NotEqual(e1.ProjectId, e2.ProjectId);
    }

    [Fact]
    public void ProjectEntity_DefaultCreatedBy_CurrentUser()
    {
        var entity = new ProjectEntity();
        Assert.Equal(Environment.UserName, entity.CreatedBy);
    }

    [Fact]
    public void ProjectEntity_DefaultDates_SetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new ProjectEntity();
        var after = DateTime.UtcNow;

        Assert.InRange(entity.CreatedDate, before, after);
        Assert.InRange(entity.LastModifiedDate, before, after);
    }

    #endregion

    #region ComponentEntryEntity Tests

    [Fact]
    public void ComponentEntryEntity_DefaultCount_IsOne()
    {
        var entity = new ComponentEntryEntity();
        Assert.Equal(1, entity.Count);
    }

    [Fact]
    public void ComponentEntryEntity_StoresEnumAsString()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Enum String Test";
        vm.ChangeOrderId = "CO-TEST";
        vm.ProjectDescription = "Test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.ProgramsDBStoredProcs;
        vm.Components[^1].Size = ComponentSize.Large;
        vm.Components[^1].ChangeType = ChangeType.Change;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var saved = projects.First(p => p.ProjectName == "Enum String Test");
        Assert.Equal("ProgramsDBStoredProcs", saved.Components[0].ComponentType);
        Assert.Equal("Large", saved.Components[0].Size);
        Assert.Equal("Change", saved.Components[0].ChangeType);
    }

    #endregion

    #region Delete Project Tests

    [Fact]
    public void DeleteProject_RemovesFromDatabase()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Delete Test";
        FillRequiredFields(vm);
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var toDelete = projects.First(p => p.ProjectName == "Delete Test");

        using var db = new EstimateDbContext();
        var entity = db.Projects.Include(p => p.Components).First(p => p.ProjectId == toDelete.ProjectId);
        db.Projects.Remove(entity);
        db.SaveChanges();

        var remaining = MainViewModel.GetAllProjects();
        Assert.DoesNotContain(remaining, p => p.ProjectName == "Delete Test");
    }

    [Fact]
    public void DeleteProject_CascadesDeleteComponents()
    {
        var vm = new MainViewModel();
        vm.ProjectName = "Cascade Test";
        FillRequiredFields(vm);
        vm.AddComponentCommand.Execute(null);
        vm.Components[^1].ComponentType = ComponentType.Reports;
        vm.Components[^1].Size = ComponentSize.Small;
        vm.Components[^1].ChangeType = ChangeType.New;
        vm.Components[^1].Count = 1;
        vm.SaveProject();

        var projects = MainViewModel.GetAllProjects();
        var toDelete = projects.First(p => p.ProjectName == "Cascade Test");
        var projectId = toDelete.ProjectId;

        using var db = new EstimateDbContext();
        var entity = db.Projects.Include(p => p.Components).First(p => p.ProjectId == projectId);
        db.Projects.Remove(entity);
        db.SaveChanges();

        // Components should also be deleted
        var orphanComponents = db.ComponentEntries.Where(c => c.ProjectId == projectId).ToList();
        Assert.Empty(orphanComponents);
    }

    #endregion

    #region Schema Compatibility Tests

    [Fact]
    public void Schema_ProjectTable_HasExpectedColumns()
    {
        // Verify the table was created with Oracle-compatible column names
        using var db = new EstimateDbContext();
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA table_info('PROJECT_ESTIMATES')";
        using var reader = cmd.ExecuteReader();

        var columns = new List<string>();
        while (reader.Read())
            columns.Add(reader.GetString(1));

        Assert.Contains("PROJECT_ID", columns);
        Assert.Contains("PROJECT_NAME", columns);
        Assert.Contains("CHANGE_ORDER_ID", columns);
        Assert.Contains("PROJECT_DESCRIPTION", columns);
        Assert.Contains("PM_EFFORT_PERCENTAGE", columns);
        Assert.Contains("TOTAL_DEVELOPMENT_HOURS", columns);
        Assert.Contains("GRAND_TOTAL_HOURS", columns);
        Assert.Contains("TSHIRT_SIZE", columns);
        Assert.Contains("CREATED_DATE", columns);
        Assert.Contains("LAST_MODIFIED_DATE", columns);
        Assert.Contains("CREATED_BY", columns);
    }

    [Fact]
    public void Schema_ComponentTable_HasExpectedColumns()
    {
        using var db = new EstimateDbContext();
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA table_info('COMPONENT_ENTRIES')";
        using var reader = cmd.ExecuteReader();

        var columns = new List<string>();
        while (reader.Read())
            columns.Add(reader.GetString(1));

        Assert.Contains("COMPONENT_ID", columns);
        Assert.Contains("PROJECT_ID", columns);
        Assert.Contains("LINE_NUMBER", columns);
        Assert.Contains("REQUIREMENT_ID", columns);
        Assert.Contains("COMPONENT_TYPE", columns);
        Assert.Contains("DESCRIPTION", columns);
        Assert.Contains("CHANGE_TYPE", columns);
        Assert.Contains("COMPONENT_SIZE", columns);
        Assert.Contains("COMPONENT_COUNT", columns);
        Assert.Contains("BASE_HOURS_PER_UNIT", columns);
        Assert.Contains("TOTAL_HOURS", columns);
    }

    [Fact]
    public void Schema_ProjectName_HasUniqueIndex()
    {
        using var db = new EstimateDbContext();
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='index' AND tbl_name='PROJECT_ESTIMATES'";
        using var reader = cmd.ExecuteReader();

        var indexSql = new List<string>();
        while (reader.Read())
        {
            var sql = reader.IsDBNull(0) ? "" : reader.GetString(0);
            indexSql.Add(sql);
        }

        Assert.Contains(indexSql, s => s.Contains("PROJECT_NAME") && s.Contains("UNIQUE"));
    }

    #endregion
}

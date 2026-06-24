using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Models;
using System.IO;

namespace InitialEstimatePOC.Data;

public class EstimateDbContext : DbContext
{
    public DbSet<WeightedValueEntity> WeightedValues { get; set; } = null!;
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<ComponentEntryEntity> ComponentEntries { get; set; } = null!;
    public DbSet<CollaborationItemEntity> CollaborationItems { get; set; } = null!;

    private readonly string _dbPath;

    public EstimateDbContext()
    {
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        _dbPath = Path.Combine(folder, "estimates.db");
    }

    public EstimateDbContext(DbContextOptions<EstimateDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeightedValueEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ComponentType, e.Size, e.ChangeType }).IsUnique();
            entity.Property(e => e.BaseHours).HasColumnType("REAL");
        });

        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("PROJECT_ESTIMATES");
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36);
            entity.Property(e => e.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.ProjectName).IsUnique();
            entity.Property(e => e.ChangeOrderId).HasColumnName("CHANGE_ORDER_ID").HasMaxLength(100);
            entity.Property(e => e.ProjectDescription).HasColumnName("PROJECT_DESCRIPTION").HasMaxLength(500);
            entity.Property(e => e.EstimatedBy).HasColumnName("ESTIMATED_BY").HasMaxLength(100);
            entity.Property(e => e.ReviewedBy).HasColumnName("REVIEWED_BY").HasMaxLength(100);
            entity.Property(e => e.PmEffortPercentage).HasColumnName("PM_EFFORT_PERCENTAGE").HasColumnType("REAL");
            entity.Property(e => e.PmReservePercentage).HasColumnName("PM_RESERVE_PERCENTAGE").HasColumnType("REAL");
            entity.Property(e => e.TotalDevelopmentHours).HasColumnName("TOTAL_DEVELOPMENT_HOURS").HasColumnType("REAL");
            entity.Property(e => e.GrandTotalHours).HasColumnName("GRAND_TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.TShirtSize).HasColumnName("TSHIRT_SIZE").HasMaxLength(20);
            entity.Property(e => e.CollaborationHours).HasColumnName("COLLABORATION_HOURS").HasColumnType("REAL");
            entity.Property(e => e.SeAdjustedHours).HasColumnName("SE_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BaAdjustedHours).HasColumnName("BA_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.CollaborationAdjustedHours).HasColumnName("COLLAB_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.DevelopmentAdjustedHours).HasColumnName("DEV_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.AnalysisAdjustedHours).HasColumnName("ANALYSIS_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BusinessDesignAdjustedHours).HasColumnName("BIZ_DESIGN_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.SystemTestingAdjustedHours).HasColumnName("SYS_TEST_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.PromotionAdjustedHours).HasColumnName("PROMOTION_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BaSystemDocAdjustedHours).HasColumnName("BA_DOC_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ProductionValidationAdjustedHours).HasColumnName("PROD_VAL_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ProjectManagementAdjustedHours).HasColumnName("PM_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.SeAssumptions).HasColumnName("SE_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.BaAssumptions).HasColumnName("BA_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.CollaborationAssumptions).HasColumnName("COLLAB_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.GeneralAssumptions).HasColumnName("GENERAL_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.AdjustedHoursComments).HasColumnName("ADJUSTED_HOURS_COMMENTS").HasMaxLength(4000);
            entity.Property(e => e.TotalActualHours).HasColumnName("TOTAL_ACTUAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ActualHoursAsOfDate).HasColumnName("ACTUAL_HOURS_AS_OF_DATE");
            entity.Property(e => e.TimeForEstimates).HasColumnName("TIME_FOR_ESTIMATES").HasColumnType("REAL");
            entity.Property(e => e.UseTestCasesForEstimate).HasColumnName("USE_TEST_CASES");
            entity.Property(e => e.TestCasesSimple).HasColumnName("TEST_CASES_SIMPLE");
            entity.Property(e => e.TestCasesMedium).HasColumnName("TEST_CASES_MEDIUM");
            entity.Property(e => e.TestCasesComplex).HasColumnName("TEST_CASES_COMPLEX");
            entity.Property(e => e.TestCasesVeryComplex).HasColumnName("TEST_CASES_VERY_COMPLEX");
            entity.Property(e => e.TestCaseIterations).HasColumnName("TEST_CASE_ITERATIONS");
            entity.Property(e => e.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(e => e.LastModifiedDate).HasColumnName("LAST_MODIFIED_DATE");
            entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100);
            entity.Property(e => e.VersionNumber).HasColumnName("VERSION_NUMBER");
            entity.HasMany(e => e.Components).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CollaborationItems).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComponentEntryEntity>(entity =>
        {
            entity.ToTable("COMPONENT_ENTRIES");
            entity.HasKey(e => e.ComponentId);
            entity.Property(e => e.ComponentId).HasColumnName("COMPONENT_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36).IsRequired();
            entity.Property(e => e.LineNumber).HasColumnName("LINE_NUMBER");
            entity.Property(e => e.RequirementId).HasColumnName("REQUIREMENT_ID").HasMaxLength(50);
            entity.Property(e => e.ComponentType).HasColumnName("COMPONENT_TYPE").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
            entity.Property(e => e.ChangeType).HasColumnName("CHANGE_TYPE").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Size).HasColumnName("COMPONENT_SIZE").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Count).HasColumnName("COMPONENT_COUNT");
            entity.Property(e => e.BaseHoursPerUnit).HasColumnName("BASE_HOURS_PER_UNIT").HasColumnType("REAL");
            entity.Property(e => e.TotalHours).HasColumnName("TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.Notes).HasColumnName("NOTES").HasMaxLength(1000);
        });

        modelBuilder.Entity<CollaborationItemEntity>(entity =>
        {
            entity.ToTable("COLLABORATION_ITEMS");
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).HasColumnName("ITEM_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36).IsRequired();
            entity.Property(e => e.LineNumber).HasColumnName("LINE_NUMBER");
            entity.Property(e => e.TaskName).HasColumnName("TASK_NAME").HasMaxLength(200);
            entity.Property(e => e.CollaborationType).HasColumnName("COLLABORATION_TYPE").HasMaxLength(50);
            entity.Property(e => e.NumberOfMeetings).HasColumnName("NUMBER_OF_MEETINGS");
            entity.Property(e => e.MeetingDurationMinutes).HasColumnName("MEETING_DURATION_MINUTES");
            entity.Property(e => e.NumberOfParticipants).HasColumnName("NUMBER_OF_PARTICIPANTS");
            entity.Property(e => e.ParticipantPrepTimeMinutes).HasColumnName("PARTICIPANT_PREP_TIME_MINUTES");
            entity.Property(e => e.TotalHours).HasColumnName("TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.Notes).HasColumnName("NOTES").HasMaxLength(1000);
        });
    }
}

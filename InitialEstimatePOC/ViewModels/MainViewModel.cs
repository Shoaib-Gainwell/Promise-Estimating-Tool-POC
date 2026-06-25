using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // === Project Header ===
    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _changeOrderId = string.Empty;

    [ObservableProperty]
    private string _projectDescription = string.Empty;

    [ObservableProperty]
    private string _estimatedBy = Environment.UserName;

    [ObservableProperty]
    private string _reviewedBy = string.Empty;

    // === Configuration ===
    [ObservableProperty]
    private decimal _pmEffortPercentage = 15m;

    [ObservableProperty]
    private decimal _pmReservePercentage = 5m;

    // Tracks the loaded project's ID (null = new unsaved project)
    private string? _currentProjectId;

    // Suppresses intermediate Recalculate calls during ClearAll
    private bool _suppressRecalculate;

    // === Calculated: Development & Derived Tasks ===
    [ObservableProperty]
    private decimal _totalDevelopmentHours;

    [ObservableProperty]
    private decimal _systemTestingHours;

    [ObservableProperty]
    private decimal _analysisHours;

    [ObservableProperty]
    private decimal _businessDesignHours;

    [ObservableProperty]
    private decimal _promotionHours;

    [ObservableProperty]
    private decimal _baSystemDocHours;

    [ObservableProperty]
    private decimal _productionValidationHours;

    [ObservableProperty]
    private decimal _projectManagementHours;

    [ObservableProperty]
    private decimal _pmReserveHours;

    [ObservableProperty]
    private decimal _subtotalHours;

    [ObservableProperty]
    private decimal _grandTotalHours;

    [ObservableProperty]
    private string _tShirtSize = "\u2014";

    [ObservableProperty]
    private int _componentCount;

    // === Collaboration ===
    [ObservableProperty]
    private int _collaborationCount;

    [ObservableProperty]
    private decimal _totalCollaborationHours;

    // === Adjusted Hours (per task type, Mid-Project Re-estimation) ===
    [ObservableProperty]
    private decimal _developmentAdjustedHours;

    [ObservableProperty]
    private decimal _analysisAdjustedHours;

    [ObservableProperty]
    private decimal _businessDesignAdjustedHours;

    [ObservableProperty]
    private decimal _systemTestingAdjustedHours;

    [ObservableProperty]
    private decimal _promotionAdjustedHours;

    [ObservableProperty]
    private decimal _baSystemDocAdjustedHours;

    [ObservableProperty]
    private decimal _productionValidationAdjustedHours;

    [ObservableProperty]
    private decimal _projectManagementAdjustedHours;

    [ObservableProperty]
    private decimal _collaborationAdjustedHours;

    // Per-task totals (Calculated + Adjusted)
    [ObservableProperty]
    private decimal _developmentTotalHours;

    [ObservableProperty]
    private decimal _analysisTotalHours;

    [ObservableProperty]
    private decimal _businessDesignTotalHours;

    [ObservableProperty]
    private decimal _systemTestingTotalHours;

    [ObservableProperty]
    private decimal _promotionTotalHours;

    [ObservableProperty]
    private decimal _baSystemDocTotalHours;

    [ObservableProperty]
    private decimal _productionValidationTotalHours;

    [ObservableProperty]
    private decimal _projectManagementTotalHours;

    [ObservableProperty]
    private decimal _collaborationTotalHours;

    // === Assumptions ===
    [ObservableProperty]
    private string _seAssumptions = string.Empty;

    [ObservableProperty]
    private string _baAssumptions = string.Empty;

    [ObservableProperty]
    private string _collaborationAssumptions = string.Empty;

    [ObservableProperty]
    private string _generalAssumptions = string.Empty;

    // === Adjusted Hours Comments ===
    [ObservableProperty]
    private string _adjustedHoursComments = string.Empty;

    // === Total Actual Hours (tracking hours already spent) ===
    [ObservableProperty]
    private decimal _totalActualHours;

    [ObservableProperty]
    private DateTime? _actualHoursAsOfDate;

    // === Time for Estimates (hours spent creating Detailed and Final estimates) ===
    [ObservableProperty]
    private decimal _timeForEstimates;

    // === Test Cases for System Testing (alternative to 30% formula) ===
    [ObservableProperty]
    private bool _useTestCasesForEstimate;

    [ObservableProperty]
    private int _testCasesSimple;

    [ObservableProperty]
    private int _testCasesMedium;

    [ObservableProperty]
    private int _testCasesComplex;

    [ObservableProperty]
    private int _testCasesVeryComplex;

    [ObservableProperty]
    private int _testCaseIterations = 1;

    // === Role Breakout ===
    [ObservableProperty]
    private decimal _baRoleHours;

    [ObservableProperty]
    private decimal _seRoleHours;

    [ObservableProperty]
    private decimal _testerRoleHours;

    [ObservableProperty]
    private decimal _pmRoleHours;

    [ObservableProperty]
    private decimal _collaborationRoleHours;

    // === Collections ===
    public ObservableCollection<ComponentRowViewModel> Components { get; } = new();
    public ObservableCollection<CollaborationRowViewModel> CollaborationItems { get; } = new();

    public Array ComponentTypes => Enum.GetValues<ComponentType>();
    public Array ChangeTypes => Enum.GetValues<ChangeType>();
    public Array Sizes => Enum.GetValues<ComponentSize>();
    public Array CollaborationTypes => Enum.GetValues<CollaborationType>();

    /// <summary>PM Effort % dropdown options matching Excel: 5, 10, 15, 20, 25</summary>
    public decimal[] PmEffortOptions => new decimal[] { 5m, 10m, 15m, 20m, 25m };

    public MainViewModel()
    {
        Components.CollectionChanged += (_, _) => Recalculate();
        CollaborationItems.CollectionChanged += (_, _) => Recalculate();
    }

    /// <summary>
    /// Pre-populates the 5 fixed collaboration rows matching the Excel layout:
    /// WPRs (10), Client Meetings (5), Internal Meetings (5), Automation Test Collaboration (5), Consultant/Mentor Effort (0)
    /// </summary>
    private void InitializeDefaultCollaborationItems()
    {
        var defaults = new[]
        {
            new { Type = CollaborationType.WPRs, Name = "WPRs", Meetings = 10, Duration = 60, Participants = 3, PrepTime = 15 },
            new { Type = CollaborationType.ClientMeetings, Name = "Client Meetings", Meetings = 5, Duration = 60, Participants = 3, PrepTime = 15 },
            new { Type = CollaborationType.InternalMeetings, Name = "Internal Meetings", Meetings = 5, Duration = 60, Participants = 3, PrepTime = 15 },
            new { Type = CollaborationType.AutomationTestCollaboration, Name = "Automation Test Collaboration", Meetings = 5, Duration = 60, Participants = 3, PrepTime = 15 },
            new { Type = CollaborationType.ConsultantMentorEffort, Name = "Consultant/Mentor Effort", Meetings = 0, Duration = 0, Participants = 0, PrepTime = 0 },
        };

        for (int i = 0; i < defaults.Length; i++)
        {
            var d = defaults[i];
            var row = new CollaborationRowViewModel
            {
                LineNumber = i + 1,
                TaskName = d.Name,
                CollabType = d.Type,
                NumberOfMeetings = d.Meetings,
                MeetingDurationMinutes = d.Duration,
                NumberOfParticipants = d.Participants,
                ParticipantPrepTimeMinutes = d.PrepTime
            };
            row.PropertyChanged += OnCollaborationChanged;
            CollaborationItems.Add(row);
        }
    }

    // === Component Commands ===
    [RelayCommand]
    private void AddComponent()
    {
        var row = new ComponentRowViewModel
        {
            LineNumber = Components.Count + 1
        };
        row.UpdateBaseHours();
        row.PropertyChanged += OnComponentChanged;
        Components.Add(row);
        Recalculate();
    }

    [RelayCommand]
    private void RemoveComponent(ComponentRowViewModel? component)
    {
        if (component is null) return;
        component.PropertyChanged -= OnComponentChanged;
        Components.Remove(component);
        for (int i = 0; i < Components.Count; i++)
            Components[i].LineNumber = i + 1;
        Recalculate();
    }

    [RelayCommand]
    private void ClearAll()
    {
        _suppressRecalculate = true;

        foreach (var c in Components)
            c.PropertyChanged -= OnComponentChanged;
        Components.Clear();
        foreach (var item in CollaborationItems)
            item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Clear();

        // Reset header fields
        ProjectName = string.Empty;
        ChangeOrderId = string.Empty;
        ProjectDescription = string.Empty;
        EstimatedBy = string.Empty;
        ReviewedBy = string.Empty;
        _currentProjectId = null;

        // Reset PM configuration to defaults
        PmEffortPercentage = 15m;
        PmReservePercentage = 5m;

        // Reset adjusted hours
        DevelopmentAdjustedHours = 0m;
        AnalysisAdjustedHours = 0m;
        BusinessDesignAdjustedHours = 0m;
        SystemTestingAdjustedHours = 0m;
        PromotionAdjustedHours = 0m;
        BaSystemDocAdjustedHours = 0m;
        ProductionValidationAdjustedHours = 0m;
        ProjectManagementAdjustedHours = 0m;
        CollaborationAdjustedHours = 0m;
        AdjustedHoursComments = string.Empty;

        // Reset assumptions
        SeAssumptions = string.Empty;
        BaAssumptions = string.Empty;
        CollaborationAssumptions = string.Empty;
        GeneralAssumptions = string.Empty;

        // Reset test case estimation
        UseTestCasesForEstimate = false;
        TestCasesSimple = 0;
        TestCasesMedium = 0;
        TestCasesComplex = 0;
        TestCasesVeryComplex = 0;
        TestCaseIterations = 1;

        // Reset actual hours
        TotalActualHours = 0m;
        TimeForEstimates = 0m;

        _suppressRecalculate = false;
        Recalculate();
    }

    // === Collaboration Commands ===
    [RelayCommand]
    private void AddCollaborationItem()
    {
        var row = new CollaborationRowViewModel
        {
            LineNumber = CollaborationItems.Count + 1,
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 15
        };
        row.PropertyChanged += OnCollaborationChanged;
        CollaborationItems.Add(row);
        Recalculate();
    }

    [RelayCommand]
    private void RemoveCollaborationItem(CollaborationRowViewModel? item)
    {
        if (item is null) return;
        item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Remove(item);
        for (int i = 0; i < CollaborationItems.Count; i++)
            CollaborationItems[i].LineNumber = i + 1;
        Recalculate();
    }

    // === Event Handlers ===
    private void OnComponentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ComponentRowViewModel.ComponentType)
            or nameof(ComponentRowViewModel.Size)
            or nameof(ComponentRowViewModel.ChangeType)
            or nameof(ComponentRowViewModel.Count))
        {
            if (sender is ComponentRowViewModel row)
                row.UpdateBaseHours();
            Recalculate();
        }
    }

    private void OnCollaborationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CollaborationRowViewModel.NumberOfMeetings)
            or nameof(CollaborationRowViewModel.MeetingDurationMinutes)
            or nameof(CollaborationRowViewModel.NumberOfParticipants)
            or nameof(CollaborationRowViewModel.ParticipantPrepTimeMinutes))
        {
            Recalculate();
        }
    }

    partial void OnPmEffortPercentageChanged(decimal value) => Recalculate();
    partial void OnPmReservePercentageChanged(decimal value) => Recalculate();
    partial void OnDevelopmentAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnAnalysisAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBusinessDesignAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnSystemTestingAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnPromotionAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBaSystemDocAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProductionValidationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProjectManagementAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnCollaborationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnUseTestCasesForEstimateChanged(bool value) => Recalculate();
    partial void OnTestCasesSimpleChanged(int value) => Recalculate();
    partial void OnTestCasesMediumChanged(int value) => Recalculate();
    partial void OnTestCasesComplexChanged(int value) => Recalculate();
    partial void OnTestCasesVeryComplexChanged(int value) => Recalculate();
    partial void OnTestCaseIterationsChanged(int value) => Recalculate();
    partial void OnTotalActualHoursChanged(decimal value) => Recalculate();
    partial void OnTimeForEstimatesChanged(decimal value) => Recalculate();

    // === Core Calculation Engine (matches Excel exactly) ===
    private void Recalculate()
    {
        if (_suppressRecalculate) return;
        // Step 1: Development = sum of all component hours
        decimal dev = 0m;
        foreach (var c in Components)
            dev += c.TotalHours;

        TotalDevelopmentHours = dev;
        ComponentCount = Components.Count;
        CollaborationCount = CollaborationItems.Count;

        // All adjustments cascade: each task uses effective (calculated + adjusted) values
        // from upstream tasks for its own calculation.

        // Effective Development = component hours + adjustment
        decimal effectiveDev = dev + DevelopmentAdjustedHours;
        TotalDevelopmentHours = effectiveDev;

        // Step 2: System Testing (depends on effectiveDev)
        if (UseTestCasesForEstimate)
        {
            decimal testHours = (TestCasesSimple * 0.5m + TestCasesMedium * 1.0m 
                               + TestCasesComplex * 2.0m + TestCasesVeryComplex * 4.0m) 
                               * Math.Max(1, TestCaseIterations);
            SystemTestingHours = RoundUp(testHours);
        }
        else
        {
            SystemTestingHours = RoundUp(effectiveDev * 0.30m);
        }
        decimal effectiveSysTest = SystemTestingHours + SystemTestingAdjustedHours;

        // Step 3: Analysis (depends on effectiveDev + effectiveSysTest)
        AnalysisHours = RoundUp((effectiveDev + effectiveSysTest) * 0.05m);
        decimal effectiveAnalysis = AnalysisHours + AnalysisAdjustedHours;

        // Step 4: Business Design (depends on effectiveDev + effectiveSysTest)
        BusinessDesignHours = RoundUp((effectiveDev + effectiveSysTest) * 0.15m);
        decimal effectiveBizDesign = BusinessDesignHours + BusinessDesignAdjustedHours;

        // Step 5: Promotion (depends on effectiveDev)
        PromotionHours = RoundUp(effectiveDev * 0.05m);
        decimal effectivePromotion = PromotionHours + PromotionAdjustedHours;

        // Step 6: BA System Documentation (depends on effectiveDev)
        BaSystemDocHours = RoundUp(effectiveDev * 0.05m);
        decimal effectiveBaSysDoc = BaSystemDocHours + BaSystemDocAdjustedHours;

        // Step 7: Production Validation (depends on effectiveSysTest)
        ProductionValidationHours = RoundUp(effectiveSysTest * 0.20m);
        decimal effectiveProdVal = ProductionValidationHours + ProductionValidationAdjustedHours;

        // Step 8: PM Effort (depends on all effective task hours)
        decimal allEffectiveTasks = effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                                  + effectivePromotion + effectiveBaSysDoc + effectiveProdVal;
        ProjectManagementHours = RoundUp(allEffectiveTasks * (PmEffortPercentage / 100m));
        decimal effectivePM = ProjectManagementHours + ProjectManagementAdjustedHours;

        // Step 9: Collaboration = sum of all collaboration items
        decimal collab = 0m;
        foreach (var item in CollaborationItems)
            collab += item.TotalHours;
        TotalCollaborationHours = collab;
        decimal effectiveCollab = collab + CollaborationAdjustedHours;

        // Per-task totals (effective = calculated + adjusted, shown in UI)
        DevelopmentTotalHours = effectiveDev;
        SystemTestingTotalHours = effectiveSysTest;
        AnalysisTotalHours = effectiveAnalysis;
        BusinessDesignTotalHours = effectiveBizDesign;
        PromotionTotalHours = effectivePromotion;
        BaSystemDocTotalHours = effectiveBaSysDoc;
        ProductionValidationTotalHours = effectiveProdVal;
        ProjectManagementTotalHours = effectivePM;
        CollaborationTotalHours = effectiveCollab;

        // Step 10: Subtotal = sum of all effective task totals + Time for Estimates + Actual Hours
        SubtotalHours = effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                      + effectivePromotion + effectiveBaSysDoc + effectiveProdVal
                      + effectivePM + effectiveCollab + TimeForEstimates + TotalActualHours;

        // When no components and no adjusted dev hours exist, don't show totals in summary
        if (ComponentCount == 0 && effectiveDev == 0m)
        {
            PmReserveHours = 0m;
            GrandTotalHours = 0m;
            TShirtSize = "—";
        }
        else
        {
            // Step 11: PM Reserve = ROUNDUP(Subtotal * Reserve%, 2)
            PmReserveHours = RoundUp(SubtotalHours * (PmReservePercentage / 100m));

            // Step 12: Grand Total = Subtotal + PM Reserve, rounded up to whole number
            GrandTotalHours = Math.Ceiling(SubtotalHours + PmReserveHours);

            // T-Shirt Size
            TShirtSize = WeightedValues.GetTShirtSize(GrandTotalHours);
        }

        // === Role Breakout (uses effective values) ===
        // BA = Analysis/2 + BusinessDesign + BADoc + ProdValidation + PM/2
        BaRoleHours = RoundUp(effectiveAnalysis / 2m + effectiveBizDesign + effectiveBaSysDoc
                     + effectiveProdVal + effectivePM / 2m);

        // SE = Development + Analysis/2 + Promotion + PM/2
        SeRoleHours = RoundUp(effectiveDev + effectiveAnalysis / 2m + effectivePromotion + effectivePM / 2m);

        // Tester = System Testing (effective)
        TesterRoleHours = effectiveSysTest;

        // PM = PM Effort (effective)
        PmRoleHours = effectivePM;

        // Collaboration = Collaboration total (effective)
        CollaborationRoleHours = effectiveCollab;
    }

    /// <summary>
    /// Excel ROUNDUP(x, 2) — always rounds away from zero at 3rd decimal.
    /// </summary>
    public static decimal RoundUp(decimal value)
    {
        if (value == 0) return 0;
        decimal shifted = value * 100m;
        decimal truncated = Math.Truncate(shifted);
        if (shifted > truncated)
            return (truncated + 1m) / 100m;
        else if (shifted < truncated)
            return (truncated - 1m) / 100m;
        return truncated / 100m;
    }

    // === Persistence ===
    public string? SaveProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
            return "Project Name is required.";

        using var db = new EstimateDbContext();
        try { db.Database.EnsureCreated(); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists")) { }

        ProjectEntity? existing = null;
        if (_currentProjectId != null)
            existing = db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                        .FirstOrDefault(p => p.ProjectId == _currentProjectId);

        if (existing == null)
            existing = db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                        .FirstOrDefault(p => p.ProjectName == ProjectName);

        if (existing != null)
        {
            existing.ProjectName = ProjectName;
            existing.ChangeOrderId = ChangeOrderId;
            existing.ProjectDescription = ProjectDescription;
            existing.EstimatedBy = EstimatedBy;
            existing.ReviewedBy = ReviewedBy;
            existing.PmEffortPercentage = PmEffortPercentage;
            existing.PmReservePercentage = PmReservePercentage;
            existing.TotalDevelopmentHours = TotalDevelopmentHours;
            existing.GrandTotalHours = GrandTotalHours;
            existing.TShirtSize = TShirtSize;
            existing.CollaborationHours = TotalCollaborationHours;
            existing.DevelopmentAdjustedHours = DevelopmentAdjustedHours;
            existing.AnalysisAdjustedHours = AnalysisAdjustedHours;
            existing.BusinessDesignAdjustedHours = BusinessDesignAdjustedHours;
            existing.SystemTestingAdjustedHours = SystemTestingAdjustedHours;
            existing.PromotionAdjustedHours = PromotionAdjustedHours;
            existing.BaSystemDocAdjustedHours = BaSystemDocAdjustedHours;
            existing.ProductionValidationAdjustedHours = ProductionValidationAdjustedHours;
            existing.ProjectManagementAdjustedHours = ProjectManagementAdjustedHours;
            existing.CollaborationAdjustedHours = CollaborationAdjustedHours;
            existing.SeAdjustedHours = DevelopmentAdjustedHours; // backward compat
            existing.BaAdjustedHours = AnalysisAdjustedHours; // backward compat
            existing.SeAssumptions = SeAssumptions;
            existing.BaAssumptions = BaAssumptions;
            existing.CollaborationAssumptions = CollaborationAssumptions;
            existing.GeneralAssumptions = GeneralAssumptions;
            existing.AdjustedHoursComments = AdjustedHoursComments;
            existing.TotalActualHours = TotalActualHours;
            existing.ActualHoursAsOfDate = ActualHoursAsOfDate;
            existing.TimeForEstimates = TimeForEstimates;
            existing.UseTestCasesForEstimate = UseTestCasesForEstimate;
            existing.TestCasesSimple = TestCasesSimple;
            existing.TestCasesMedium = TestCasesMedium;
            existing.TestCasesComplex = TestCasesComplex;
            existing.TestCasesVeryComplex = TestCasesVeryComplex;
            existing.TestCaseIterations = TestCaseIterations;
            existing.LastModifiedDate = DateTime.UtcNow;
            existing.VersionNumber++;

            db.ComponentEntries.RemoveRange(existing.Components);
            existing.Components = MapComponentsToEntities(existing.ProjectId);

            db.CollaborationItems.RemoveRange(existing.CollaborationItems);
            existing.CollaborationItems = MapCollaborationToEntities(existing.ProjectId);

            _currentProjectId = existing.ProjectId;
        }
        else
        {
            var project = new ProjectEntity
            {
                ProjectName = ProjectName,
                ChangeOrderId = ChangeOrderId,
                ProjectDescription = ProjectDescription,
                EstimatedBy = EstimatedBy,
                ReviewedBy = ReviewedBy,
                PmEffortPercentage = PmEffortPercentage,
                PmReservePercentage = PmReservePercentage,
                TotalDevelopmentHours = TotalDevelopmentHours,
                GrandTotalHours = GrandTotalHours,
                TShirtSize = TShirtSize,
                CollaborationHours = TotalCollaborationHours,
                DevelopmentAdjustedHours = DevelopmentAdjustedHours,
                AnalysisAdjustedHours = AnalysisAdjustedHours,
                BusinessDesignAdjustedHours = BusinessDesignAdjustedHours,
                SystemTestingAdjustedHours = SystemTestingAdjustedHours,
                PromotionAdjustedHours = PromotionAdjustedHours,
                BaSystemDocAdjustedHours = BaSystemDocAdjustedHours,
                ProductionValidationAdjustedHours = ProductionValidationAdjustedHours,
                ProjectManagementAdjustedHours = ProjectManagementAdjustedHours,
                CollaborationAdjustedHours = CollaborationAdjustedHours,
                SeAdjustedHours = DevelopmentAdjustedHours, // backward compat
                BaAdjustedHours = AnalysisAdjustedHours, // backward compat
                SeAssumptions = SeAssumptions,
                BaAssumptions = BaAssumptions,
                CollaborationAssumptions = CollaborationAssumptions,
                GeneralAssumptions = GeneralAssumptions,
                AdjustedHoursComments = AdjustedHoursComments,
                TotalActualHours = TotalActualHours,
                ActualHoursAsOfDate = ActualHoursAsOfDate,
                TimeForEstimates = TimeForEstimates,
                UseTestCasesForEstimate = UseTestCasesForEstimate,
                TestCasesSimple = TestCasesSimple,
                TestCasesMedium = TestCasesMedium,
                TestCasesComplex = TestCasesComplex,
                TestCasesVeryComplex = TestCasesVeryComplex,
                TestCaseIterations = TestCaseIterations,
            };
            project.Components = MapComponentsToEntities(project.ProjectId);
            project.CollaborationItems = MapCollaborationToEntities(project.ProjectId);
            db.Projects.Add(project);
            _currentProjectId = project.ProjectId;
        }

        db.SaveChanges();
        return null;
    }

    private List<ComponentEntryEntity> MapComponentsToEntities(string projectId)
    {
        var list = new List<ComponentEntryEntity>();
        foreach (var c in Components)
        {
            list.Add(new ComponentEntryEntity
            {
                ProjectId = projectId,
                LineNumber = c.LineNumber,
                RequirementId = c.RequirementId,
                ComponentType = c.ComponentType.ToString(),
                Description = c.Description,
                ChangeType = c.ChangeType.ToString(),
                Size = c.Size.ToString(),
                Count = c.Count,
                BaseHoursPerUnit = c.BaseHoursPerUnit,
                TotalHours = c.TotalHours,
                Notes = c.Notes
            });
        }
        return list;
    }

    private List<CollaborationItemEntity> MapCollaborationToEntities(string projectId)
    {
        var list = new List<CollaborationItemEntity>();
        foreach (var item in CollaborationItems)
        {
            list.Add(new CollaborationItemEntity
            {
                ProjectId = projectId,
                LineNumber = item.LineNumber,
                TaskName = item.TaskName,
                CollaborationType = item.CollabType.ToString(),
                NumberOfMeetings = item.NumberOfMeetings,
                MeetingDurationMinutes = item.MeetingDurationMinutes,
                NumberOfParticipants = item.NumberOfParticipants,
                ParticipantPrepTimeMinutes = item.ParticipantPrepTimeMinutes,
                TotalHours = item.TotalHours,
                Notes = item.Notes
            });
        }
        return list;
    }

    public void LoadProject(ProjectEntity project)
    {
        foreach (var c in Components)
            c.PropertyChanged -= OnComponentChanged;
        Components.Clear();

        foreach (var item in CollaborationItems)
            item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Clear();

        _currentProjectId = project.ProjectId;
        ProjectName = project.ProjectName;
        ChangeOrderId = project.ChangeOrderId;
        ProjectDescription = project.ProjectDescription;
        EstimatedBy = project.EstimatedBy;
        ReviewedBy = project.ReviewedBy;
        PmEffortPercentage = project.PmEffortPercentage;
        PmReservePercentage = project.PmReservePercentage;
        DevelopmentAdjustedHours = project.DevelopmentAdjustedHours != 0 ? project.DevelopmentAdjustedHours : project.SeAdjustedHours;
        AnalysisAdjustedHours = project.AnalysisAdjustedHours != 0 ? project.AnalysisAdjustedHours : project.BaAdjustedHours;
        BusinessDesignAdjustedHours = project.BusinessDesignAdjustedHours;
        SystemTestingAdjustedHours = project.SystemTestingAdjustedHours;
        PromotionAdjustedHours = project.PromotionAdjustedHours;
        BaSystemDocAdjustedHours = project.BaSystemDocAdjustedHours;
        ProductionValidationAdjustedHours = project.ProductionValidationAdjustedHours;
        ProjectManagementAdjustedHours = project.ProjectManagementAdjustedHours;
        CollaborationAdjustedHours = project.CollaborationAdjustedHours;
        SeAssumptions = project.SeAssumptions;
        BaAssumptions = project.BaAssumptions;
        CollaborationAssumptions = project.CollaborationAssumptions;
        GeneralAssumptions = project.GeneralAssumptions;
        AdjustedHoursComments = project.AdjustedHoursComments;
        TotalActualHours = project.TotalActualHours;
        ActualHoursAsOfDate = project.ActualHoursAsOfDate;
        TimeForEstimates = project.TimeForEstimates;
        UseTestCasesForEstimate = project.UseTestCasesForEstimate;
        TestCasesSimple = project.TestCasesSimple;
        TestCasesMedium = project.TestCasesMedium;
        TestCasesComplex = project.TestCasesComplex;
        TestCasesVeryComplex = project.TestCasesVeryComplex;
        TestCaseIterations = project.TestCaseIterations;

        foreach (var entry in project.Components.OrderBy(c => c.LineNumber))
        {
            var row = new ComponentRowViewModel
            {
                LineNumber = entry.LineNumber,
                RequirementId = entry.RequirementId,
                ComponentType = Enum.Parse<ComponentType>(entry.ComponentType),
                Description = entry.Description,
                ChangeType = Enum.Parse<ChangeType>(entry.ChangeType),
                Size = Enum.Parse<ComponentSize>(entry.Size),
                Count = entry.Count,
                Notes = entry.Notes
            };
            row.UpdateBaseHours();
            row.PropertyChanged += OnComponentChanged;
            Components.Add(row);
        }

        foreach (var entry in project.CollaborationItems.OrderBy(c => c.LineNumber))
        {
            var row = new CollaborationRowViewModel
            {
                LineNumber = entry.LineNumber,
                TaskName = entry.TaskName,
                CollabType = Enum.TryParse<CollaborationType>(entry.CollaborationType, out var ct) ? ct : CollaborationType.WPRs,
                NumberOfMeetings = entry.NumberOfMeetings,
                MeetingDurationMinutes = entry.MeetingDurationMinutes,
                NumberOfParticipants = entry.NumberOfParticipants,
                ParticipantPrepTimeMinutes = entry.ParticipantPrepTimeMinutes,
                Notes = entry.Notes
            };
            row.PropertyChanged += OnCollaborationChanged;
            CollaborationItems.Add(row);
        }

        Recalculate();
    }

    public static List<ProjectEntity> GetAllProjects()
    {
        using var db = new EstimateDbContext();
        try { db.Database.EnsureCreated(); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists")) { }
        return db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                 .OrderByDescending(p => p.LastModifiedDate)
                 .ToList();
    }
}

// === Component Row ViewModel ===
public partial class ComponentRowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _lineNumber;

    [ObservableProperty]
    private string _requirementId = string.Empty;

    [ObservableProperty]
    private ComponentType _componentType;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ChangeType _changeType;

    [ObservableProperty]
    private ComponentSize _size;

    [ObservableProperty]
    private int _count = 1;

    [ObservableProperty]
    private decimal _baseHoursPerUnit;

    [ObservableProperty]
    private string _notes = string.Empty;

    public decimal TotalHours => BaseHoursPerUnit * Count;

    public void UpdateBaseHours()
    {
        BaseHoursPerUnit = WeightedValues.GetBaseHours(ComponentType, Size, ChangeType);
        OnPropertyChanged(nameof(TotalHours));
    }

    partial void OnComponentTypeChanged(ComponentType value) => UpdateBaseHours();
    partial void OnSizeChanged(ComponentSize value) => UpdateBaseHours();
    partial void OnChangeTypeChanged(ChangeType value) => UpdateBaseHours();
    partial void OnCountChanged(int value) => OnPropertyChanged(nameof(TotalHours));
}

// === Collaboration Row ViewModel ===
/// <summary>
/// Matches Excel formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × NumParticipants
/// Excel columns: J=Number of Meetings/WPRs, K=Meeting Duration (In Mins), L=Number of Participants, M=Participant Prep Time (In Mins)
/// </summary>
public partial class CollaborationRowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _lineNumber;

    [ObservableProperty]
    private string _taskName = string.Empty;

    [ObservableProperty]
    private CollaborationType _collabType = CollaborationType.WPRs;

    /// <summary>Column J: Number of Meetings / WPRs</summary>
    [ObservableProperty]
    private int _numberOfMeetings = 1;

    /// <summary>Column K: Meeting Duration (In Mins)</summary>
    [ObservableProperty]
    private int _meetingDurationMinutes = 60;

    /// <summary>Column L: Number of Participants</summary>
    [ObservableProperty]
    private int _numberOfParticipants = 3;

    /// <summary>Column M: Participant Prep Time (In Mins)</summary>
    [ObservableProperty]
    private int _participantPrepTimeMinutes = 15;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// Excel formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × NumParticipants
    /// Example: 5 meetings × (60/60 + 15/60) × 3 participants = 5 × 1.25 × 3 = 18.75 hours
    /// </summary>
    public decimal TotalHours => NumberOfMeetings * ((MeetingDurationMinutes / 60m) + (ParticipantPrepTimeMinutes / 60m)) * NumberOfParticipants;

    partial void OnNumberOfMeetingsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnMeetingDurationMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnNumberOfParticipantsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnParticipantPrepTimeMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
}

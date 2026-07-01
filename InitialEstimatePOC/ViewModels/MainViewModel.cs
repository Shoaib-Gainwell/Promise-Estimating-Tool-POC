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

    // Per-type collaboration hours (calculated from Collaboration tab)
    [ObservableProperty]
    private decimal _wprsHours;

    [ObservableProperty]
    private decimal _clientMeetingsHours;

    [ObservableProperty]
    private decimal _internalMeetingsHours;

    [ObservableProperty]
    private decimal _automationTestCollabHours;

    [ObservableProperty]
    private decimal _consultantMentorHours;

    // Per-type collaboration adjusted hours
    [ObservableProperty]
    private decimal _wprsAdjustedHours;

    [ObservableProperty]
    private decimal _clientMeetingsAdjustedHours;

    [ObservableProperty]
    private decimal _internalMeetingsAdjustedHours;

    [ObservableProperty]
    private decimal _automationTestCollabAdjustedHours;

    [ObservableProperty]
    private decimal _consultantMentorAdjustedHours;

    // Per-type collaboration totals (calculated + adjusted)
    [ObservableProperty]
    private decimal _wprsTotalHours;

    [ObservableProperty]
    private decimal _clientMeetingsTotalHours;

    [ObservableProperty]
    private decimal _internalMeetingsTotalHours;

    [ObservableProperty]
    private decimal _automationTestCollabTotalHours;

    [ObservableProperty]
    private decimal _consultantMentorTotalHours;

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
    private decimal _testCasesSimple;

    [ObservableProperty]
    private decimal _testCasesMedium;

    [ObservableProperty]
    private decimal _testCasesComplex;

    [ObservableProperty]
    private decimal _testCasesVeryComplex;

    [ObservableProperty]
    private decimal _testCaseIterations = 0;

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

    /// <summary>PM Effort % dropdown options matching Excel J34: 1–20</summary>
    public decimal[] PmEffortOptions => Enumerable.Range(1, 20).Select(i => (decimal)i).ToArray();

    /// <summary>Number of Meetings / WPRs dropdown matching Excel J36:J42 validation: 0–20</summary>
    public int[] MeetingCountOptions => Enumerable.Range(0, 21).ToArray();

    /// <summary>Meeting Duration (In Mins) dropdown matching Excel K36:K39 validation: 0,15,30,45,60</summary>
    public int[] MeetingDurationOptions => new[] { 0, 15, 30, 45, 60 };

    /// <summary>Number of Participants dropdown matching Excel L36:L42 validation: 0–20</summary>
    public int[] ParticipantCountOptions => Enumerable.Range(0, 21).ToArray();

    /// <summary>Participant Prep Time (In Mins) dropdown matching Excel M36:M39 validation: 0,15,30,...,180</summary>
    public int[] PrepTimeOptions => Enumerable.Range(0, 13).Select(i => i * 15).ToArray();

    /// <summary>
    /// True when at least one component row exists in the grid.
    /// Used to enable/disable Collaboration and Adjustments tabs.
    /// </summary>
    public bool HasComponents => Components.Count > 0;

    /// <summary>
    /// True when at least one component row has all required columns filled:
    /// Req#, Component Type, New/Change, Size, and Count > 0.
    /// Used to enable the Collaboration tab.
    /// </summary>
    public bool HasValidComponents => Components.Any(c =>
        !string.IsNullOrWhiteSpace(c.RequirementId) &&
        c.ComponentType != ComponentType.None &&
        c.ChangeType != ChangeType.None &&
        c.Size != ComponentSize.None &&
        c.Count > 0);

    public MainViewModel()
    {
        Components.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
                foreach (ComponentRowViewModel row in e.NewItems)
                    row.PropertyChanged += OnComponentRowPropertyChanged;
            if (e.OldItems != null)
                foreach (ComponentRowViewModel row in e.OldItems)
                    row.PropertyChanged -= OnComponentRowPropertyChanged;
            Recalculate();
            OnPropertyChanged(nameof(HasComponents));
            OnPropertyChanged(nameof(HasValidComponents));
        };
        CollaborationItems.CollectionChanged += (_, _) => Recalculate();
        InitializeDefaultCollaborationItems();
    }

    private void OnComponentRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ComponentRowViewModel.RequirementId) ||
            e.PropertyName == nameof(ComponentRowViewModel.ComponentType) ||
            e.PropertyName == nameof(ComponentRowViewModel.ChangeType) ||
            e.PropertyName == nameof(ComponentRowViewModel.Size) ||
            e.PropertyName == nameof(ComponentRowViewModel.Count))
            OnPropertyChanged(nameof(HasValidComponents));
    }

    /// <summary>
    /// Pre-populates the 5 fixed collaboration rows matching the Excel layout:
    /// WPRs (10), Client Meetings (5), Internal Meetings (5), Automation Test Collaboration (5), Consultant/Mentor Effort (0)
    /// </summary>
    private void InitializeDefaultCollaborationItems()
    {
        var defaults = new[]
        {
            new { Type = CollaborationType.WPRs, Name = "WPRs" },
            new { Type = CollaborationType.ClientMeetings, Name = "Client Meetings" },
            new { Type = CollaborationType.InternalMeetings, Name = "Internal Meetings" },
            new { Type = CollaborationType.AutomationTestCollaboration, Name = "Automation Test Collaboration" },
        };

        for (int i = 0; i < defaults.Length; i++)
        {
            var d = defaults[i];
            var row = new CollaborationRowViewModel
            {
                LineNumber = i + 1,
                TaskName = d.Name,
                CollabType = d.Type,
                NumberOfMeetings = 0,
                MeetingDurationMinutes = 0,
                NumberOfParticipants = 0,
                ParticipantPrepTimeMinutes = 0
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
        InitializeDefaultCollaborationItems();

        // Reset header fields
        ProjectName = string.Empty;
        ChangeOrderId = string.Empty;
        ProjectDescription = string.Empty;
        EstimatedBy = string.Empty;
        ReviewedBy = string.Empty;
        _currentProjectId = null;

        // Reset PM configuration to defaults
        PmEffortPercentage = 15m;

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
        WprsAdjustedHours = 0m;
        ClientMeetingsAdjustedHours = 0m;
        InternalMeetingsAdjustedHours = 0m;
        AutomationTestCollabAdjustedHours = 0m;
        ConsultantMentorAdjustedHours = 0m;
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
        TestCaseIterations = 0m;

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
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 0
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
    partial void OnDevelopmentAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnAnalysisAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBusinessDesignAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnSystemTestingAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnPromotionAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBaSystemDocAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProductionValidationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProjectManagementAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnCollaborationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnWprsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnClientMeetingsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnInternalMeetingsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnAutomationTestCollabAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnConsultantMentorAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnUseTestCasesForEstimateChanged(bool value) => Recalculate();
    partial void OnTestCasesSimpleChanged(decimal value) => Recalculate();
    partial void OnTestCasesMediumChanged(decimal value) => Recalculate();
    partial void OnTestCasesComplexChanged(decimal value) => Recalculate();
    partial void OnTestCasesVeryComplexChanged(decimal value) => Recalculate();
    partial void OnTestCaseIterationsChanged(decimal value) => Recalculate();
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

        // Step 2: System Testing (depends on effectiveDev)
        if (UseTestCasesForEstimate)
        {
            // Test case-based formula matching Excel IntialEstWeightedValues rows 31 & 32:
            // Row 31 = Write TC + Data Prep + ALM Tasks + Test Execution (per complexity)
            // Row 32 = Write TC + ALM Tasks + Test Execution (excl. Data Prep) × 10% defect correction
            const decimal r31Simple = 2.1925m, r31Medium = 4.065m, r31Complex = 8.76m, r31VeryComplex = 14.38m;
            const decimal r32Simple = 1.5675m, r32Medium = 3.44m,  r32Complex = 7.51m, r32VeryComplex = 13.13m;
            decimal mainHours   = TestCasesSimple * r31Simple + TestCasesMedium * r31Medium
                                + TestCasesComplex * r31Complex + TestCasesVeryComplex * r31VeryComplex;
            decimal defectHours = (TestCasesSimple * r32Simple + TestCasesMedium * r32Medium
                                + TestCasesComplex * r32Complex + TestCasesVeryComplex * r32VeryComplex) * 0.1m;
            SystemTestingHours = RoundUp((mainHours + defectHours) * Math.Max(1m, TestCaseIterations));
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

        // Step 9: Collaboration = sum of all collaboration items (also split by type)
        decimal collab = 0m;
        decimal wprs = 0m, clientMtg = 0m, internalMtg = 0m, autoTest = 0m, consultant = 0m;
        foreach (var item in CollaborationItems)
        {
            collab += item.TotalHours;
            switch (item.CollabType)
            {
                case CollaborationType.WPRs:                      wprs       += item.TotalHours; break;
                case CollaborationType.ClientMeetings:            clientMtg  += item.TotalHours; break;
                case CollaborationType.InternalMeetings:          internalMtg+= item.TotalHours; break;
                case CollaborationType.AutomationTestCollaboration:autoTest  += item.TotalHours; break;
            }
        }
        TotalCollaborationHours   = collab;
        WprsHours                 = wprs;
        ClientMeetingsHours       = clientMtg;
        InternalMeetingsHours     = internalMtg;
        AutomationTestCollabHours = autoTest;
        ConsultantMentorHours     = consultant;

        // Per-type totals = calculated + per-type adjusted
        WprsTotalHours                 = wprs     + WprsAdjustedHours;
        ClientMeetingsTotalHours       = clientMtg + ClientMeetingsAdjustedHours;
        InternalMeetingsTotalHours     = internalMtg + InternalMeetingsAdjustedHours;
        AutomationTestCollabTotalHours = autoTest  + AutomationTestCollabAdjustedHours;
        ConsultantMentorTotalHours     = consultant + ConsultantMentorAdjustedHours;

        // Effective collab = sum of all per-type totals (replaces old collab + CollaborationAdjustedHours)
        decimal effectiveCollab = WprsTotalHours + ClientMeetingsTotalHours + InternalMeetingsTotalHours
                                + AutomationTestCollabTotalHours + ConsultantMentorTotalHours;

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

        // Step 10: Subtotal = ROUNDUP(SUM of all effective task totals + Time for Estimates + Actual Hours, 2)
        // Matches Excel I43 = ROUNDUP(SUM(I27:I42), 2)
        SubtotalHours = RoundUp(effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                      + effectivePromotion + effectiveBaSysDoc + effectiveProdVal
                      + effectivePM + effectiveCollab + TimeForEstimates + TotalActualHours);

        // When no components and no adjusted dev hours exist, don't show totals in summary
        if (ComponentCount == 0 && effectiveDev == 0m)
        {
            GrandTotalHours = 0m;
            TShirtSize = "—";
        }
        else
        {
            // Grand Total = Subtotal rounded up to whole number (matches Excel I3 = ROUNDUP(I43,0))
            GrandTotalHours = Math.Ceiling(SubtotalHours);

            // T-Shirt Size
            TShirtSize = WeightedValues.GetTShirtSize(GrandTotalHours);
        }

        // === Role Breakout (Excel rows 47-51) ===
        // BA = ROUNDUP(Analysis/2 + BizDesign + BADoc + ProdVal + ActualHours/2 + TimeForEstimates/2, 2)
        // Excel B47: ROUNDUP((I28/2)+I29+I32+I33+(I41/2)+(I42/2),2) — PM is NOT included
        BaRoleHours = RoundUp(effectiveAnalysis / 2m + effectiveBizDesign + effectiveBaSysDoc
                     + effectiveProdVal + TotalActualHours / 2m + TimeForEstimates / 2m);

        // SE = ROUNDUP(Dev + Analysis/2 + Promotion + ActualHours/2 + TimeForEstimates/2, 2)
        // Excel B48: ROUNDUP(I27+(I28/2)+I31+(I41/2)+(I42/2),2) — PM is NOT included
        SeRoleHours = RoundUp(effectiveDev + effectiveAnalysis / 2m + effectivePromotion
                     + TotalActualHours / 2m + TimeForEstimates / 2m);

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
        if (string.IsNullOrWhiteSpace(ChangeOrderId))
            return "CO / Defect # is required.";
        if (string.IsNullOrWhiteSpace(ProjectDescription))
            return "Description is required.";
        if (string.IsNullOrWhiteSpace(EstimatedBy))
            return "Estimated By is required.";
        if (string.IsNullOrWhiteSpace(ReviewedBy))
            return "Reviewed By is required.";
        if (!Components.Any(c => c.ComponentType != ComponentType.None))
            return "At least one component must be added before saving.";

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
            existing.WprsAdjustedHours = WprsAdjustedHours;
            existing.ClientMeetingsAdjustedHours = ClientMeetingsAdjustedHours;
            existing.InternalMeetingsAdjustedHours = InternalMeetingsAdjustedHours;
            existing.AutomationTestCollabAdjustedHours = AutomationTestCollabAdjustedHours;
            existing.ConsultantMentorAdjustedHours = ConsultantMentorAdjustedHours;
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
                WprsAdjustedHours = WprsAdjustedHours,
                ClientMeetingsAdjustedHours = ClientMeetingsAdjustedHours,
                InternalMeetingsAdjustedHours = InternalMeetingsAdjustedHours,
                AutomationTestCollabAdjustedHours = AutomationTestCollabAdjustedHours,
                ConsultantMentorAdjustedHours = ConsultantMentorAdjustedHours,
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
            if (c.ComponentType == ComponentType.None) continue; // skip unselected placeholder rows
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
        DevelopmentAdjustedHours = project.DevelopmentAdjustedHours != 0 ? project.DevelopmentAdjustedHours : project.SeAdjustedHours;
        AnalysisAdjustedHours = project.AnalysisAdjustedHours != 0 ? project.AnalysisAdjustedHours : project.BaAdjustedHours;
        BusinessDesignAdjustedHours = project.BusinessDesignAdjustedHours;
        SystemTestingAdjustedHours = project.SystemTestingAdjustedHours;
        PromotionAdjustedHours = project.PromotionAdjustedHours;
        BaSystemDocAdjustedHours = project.BaSystemDocAdjustedHours;
        ProductionValidationAdjustedHours = project.ProductionValidationAdjustedHours;
        ProjectManagementAdjustedHours = project.ProjectManagementAdjustedHours;
        CollaborationAdjustedHours = project.CollaborationAdjustedHours;
        WprsAdjustedHours = project.WprsAdjustedHours;
        ClientMeetingsAdjustedHours = project.ClientMeetingsAdjustedHours;
        InternalMeetingsAdjustedHours = project.InternalMeetingsAdjustedHours;
        AutomationTestCollabAdjustedHours = project.AutomationTestCollabAdjustedHours;
        ConsultantMentorAdjustedHours = project.ConsultantMentorAdjustedHours;
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

        var savedCollab = project.CollaborationItems.OrderBy(c => c.LineNumber).ToList();
        if (savedCollab.Count > 0)
        {
            foreach (var entry in savedCollab)
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
        }
        else
        {
            InitializeDefaultCollaborationItems();
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
    private ComponentType _componentType = ComponentType.None;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ChangeType _changeType = ChangeType.None;

    [ObservableProperty]
    private ComponentSize _size = ComponentSize.None;

    [ObservableProperty]
    private int _count = 0;

    [ObservableProperty]
    private decimal _baseHoursPerUnit;

    [ObservableProperty]
    private string _notes = string.Empty;

    public decimal TotalHours => BaseHoursPerUnit * Count;

    public void UpdateBaseHours()
    {
        if (ComponentType == ComponentType.None || Size == ComponentSize.None || ChangeType == ChangeType.None || Count <= 0)
        {
            BaseHoursPerUnit = 0m;
            OnPropertyChanged(nameof(TotalHours));
            return;
        }
        BaseHoursPerUnit = WeightedValues.GetBaseHours(ComponentType, Size, ChangeType);
        OnPropertyChanged(nameof(TotalHours));
    }

    partial void OnComponentTypeChanged(ComponentType value) => UpdateBaseHours();
    partial void OnSizeChanged(ComponentSize value) => UpdateBaseHours();
    partial void OnChangeTypeChanged(ChangeType value) => UpdateBaseHours();
    partial void OnCountChanged(int value) => UpdateBaseHours();
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
    /// Excel formula: ROUNDUP((Meetings × Participants × Duration/60) + (Meetings × Participants × PrepTime/60), 2)
    /// Matches Excel G36 = ROUNDUP((J36*L36*(K36/60))+(J36*L36*(M36/60)),2)
    /// </summary>
    public decimal TotalHours => MainViewModel.RoundUp(NumberOfMeetings * ((MeetingDurationMinutes / 60m) + (ParticipantPrepTimeMinutes / 60m)) * NumberOfParticipants);

    partial void OnNumberOfMeetingsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnMeetingDurationMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnNumberOfParticipantsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnParticipantPrepTimeMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
}

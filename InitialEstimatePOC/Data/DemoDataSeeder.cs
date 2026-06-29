using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Data;

/// <summary>
/// Seeds demo project data for presentation purposes.
/// Creates sample projects covering ALL T-shirt sizes (Small → XL8).
/// Every field is populated — no blanks.
/// </summary>
public static class DemoDataSeeder
{
    public static void SeedDemoProjects(EstimateDbContext db)
    {
        if (db.Projects.Any()) return;

        var projects = new List<ProjectEntity>
        {
            CreateSmallProject(),
            CreateMediumProject(),
            CreateLargeProject(),
            CreateXLargeProject(),
            CreateXL1Project(),
            CreateXL2Project(),
            CreateXL3Project(),
            CreateXL4Project(),
            CreateXL5Project(),
            CreateXL6Project(),
            CreateXL7Project(),
            CreateXL8Project(),
        };

        db.Projects.AddRange(projects);
        db.SaveChanges();
    }

    private static ProjectEntity CreateSmallProject() => new()
    {
        ProjectName = "Provider Address Update Fix",
        ChangeOrderId = "DEF-2026-0012",
        ProjectDescription = "Fix provider address validation on the network maintenance screen. Corrects ZIP+4 parsing and state dropdown population.",
        EstimatedBy = "RWilliams", ReviewedBy = "KChen",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 25.00m, GrandTotalHours = 65.28m, TShirtSize = "Small",
        CollaborationHours = 18.75m,
        DevelopmentAdjustedHours = 0m, AnalysisAdjustedHours = 0m, BusinessDesignAdjustedHours = 0m,
        SystemTestingAdjustedHours = 0m, PromotionAdjustedHours = 0m, BaSystemDocAdjustedHours = 0m,
        ProductionValidationAdjustedHours = 0m, ProjectManagementAdjustedHours = 0m, CollaborationAdjustedHours = 0m,
        SeAssumptions = "Existing PowerBuilder source code is available. No third-party library changes needed.",
        BaAssumptions = "Requirements are well-defined in defect ticket. No BRD needed.",
        CollaborationAssumptions = "2 meetings with onshore team for defect clarification.",
        GeneralAssumptions = "Standard defect fix turnaround. No environment setup required.",
        AdjustedHoursComments = "No adjustments — straightforward defect fix.",
        TotalActualHours = 8.5m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-1), TimeForEstimates = 2.0m,
        UseTestCasesForEstimate = false, TestCasesSimple = 0, TestCasesMedium = 0, TestCasesComplex = 0, TestCasesVeryComplex = 0, TestCaseIterations = 1,
        CreatedDate = DateTime.UtcNow.AddDays(-5), LastModifiedDate = DateTime.UtcNow.AddDays(-1), CreatedBy = "RWilliams", VersionNumber = 2,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-001", ComponentType = "PowerBuilderWindows", Description = "Provider address validation fix on network maintenance screen", ChangeType = "Change", Size = "Small", Count = 1, BaseHoursPerUnit = 20.94m, TotalHours = 20.94m, Notes = "Fix ZIP+4 regex and state dropdown" },
            new() { LineNumber = 2, RequirementId = "REQ-002", ComponentType = "SupportModules", Description = "Address format utility — update validation rules", ChangeType = "Change", Size = "Small", Count = 1, BaseHoursPerUnit = 4.0625m, TotalHours = 4.0625m, Notes = "Shared by 3 other screens" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 3, MeetingDurationMinutes = 30, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 10, TotalHours = 6.00m, Notes = "Initial, Code, Test Results" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 2, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 7.50m, Notes = "Defect review with client" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 2, MeetingDurationMinutes = 30, NumberOfParticipants = 2, ParticipantPrepTimeMinutes = 10, TotalHours = 2.67m, Notes = "Daily standup coverage" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 1, MeetingDurationMinutes = 45, NumberOfParticipants = 2, ParticipantPrepTimeMinutes = 15, TotalHours = 2.00m, Notes = "Regression test update" },
        }
    };

    private static ProjectEntity CreateMediumProject() => new()
    {
        ProjectName = "Authorization Letter Generation",
        ChangeOrderId = "CO-2026-0198",
        ProjectDescription = "Automated prior authorization letter generation with configurable templates. Supports PDF output, member/provider letters, and batch printing.",
        EstimatedBy = "KChen", ReviewedBy = "MJohnson",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 145.13m, GrandTotalHours = 215.45m, TShirtSize = "Medium",
        CollaborationHours = 37.50m,
        DevelopmentAdjustedHours = 5m, AnalysisAdjustedHours = 0m, BusinessDesignAdjustedHours = 0m,
        SystemTestingAdjustedHours = 3m, PromotionAdjustedHours = 0m, BaSystemDocAdjustedHours = 0m,
        ProductionValidationAdjustedHours = 0m, ProjectManagementAdjustedHours = 0m, CollaborationAdjustedHours = 0m,
        SeAssumptions = "Crystal Reports runtime available on all servers. PDF library licensed. Existing letter templates can be migrated.",
        BaAssumptions = "BRD approved by compliance. Letter content reviewed by legal. No new regulatory requirements.",
        CollaborationAssumptions = "Weekly status with client. 5 WPRs scheduled (BRD, Estimate, TDD, Code, Test Results).",
        GeneralAssumptions = "No infrastructure changes required. Existing print queue infrastructure reused.",
        AdjustedHoursComments = "Added 5 SE hrs for legacy template migration complexity. Added 3 testing hrs for PDF rendering edge cases.",
        TotalActualHours = 42.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-3), TimeForEstimates = 4.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 8, TestCasesMedium = 5, TestCasesComplex = 2, TestCasesVeryComplex = 0, TestCaseIterations = 2,
        CreatedDate = DateTime.UtcNow.AddDays(-12), LastModifiedDate = DateTime.UtcNow.AddDays(-3), CreatedBy = "KChen", VersionNumber = 3,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-050", ComponentType = "PowerBuilderWindows", Description = "Letter template configuration screen — add/edit/delete templates", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 25m, TotalHours = 25m, Notes = "Uses existing config framework" },
            new() { LineNumber = 2, RequirementId = "REQ-051", ComponentType = "Reports", Description = "Prior auth letter PDF generation — member and provider versions", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 51m, TotalHours = 51m, Notes = "Crystal Reports with subreport" },
            new() { LineNumber = 3, RequirementId = "REQ-052", ComponentType = "ProgramsDBStoredProcs", Description = "Letter queue batch processor — nightly batch for bulk letters", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 46m, TotalHours = 46m, Notes = "Scheduled via Control-M" },
            new() { LineNumber = 4, RequirementId = "REQ-053", ComponentType = "DBManipulation", Description = "Letter template storage tables and seed data", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 15m, TotalHours = 15m, Notes = "3 new tables + lookup data" },
            new() { LineNumber = 5, RequirementId = "REQ-054", ComponentType = "DatabaseReview", Description = "Letter archive schema review", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 8.125m, TotalHours = 8.125m, Notes = "Archival after 90 days" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "BRD, Estimate, TDD, Code, Test Results" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 3, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 11.25m, Notes = "Kickoff, mid-point, sign-off" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 2, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 7.50m, Notes = "Sprint planning" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 0, MeetingDurationMinutes = 0, NumberOfParticipants = 0, ParticipantPrepTimeMinutes = 0, TotalHours = 0m, Notes = "Manual testing only" },
        }
    };

    private static ProjectEntity CreateLargeProject() => new()
    {
        ProjectName = "Executive Reporting Dashboard",
        ChangeOrderId = "CO-2026-0289",
        ProjectDescription = "Real-time KPI dashboard for claims volume, turnaround times, and provider network metrics with drill-down and export.",
        EstimatedBy = "MJohnson", ReviewedBy = "APatel",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 187.88m, GrandTotalHours = 490.75m, TShirtSize = "Large",
        CollaborationHours = 56.25m,
        DevelopmentAdjustedHours = 10m, AnalysisAdjustedHours = 0m, BusinessDesignAdjustedHours = 5m,
        SystemTestingAdjustedHours = 0m, PromotionAdjustedHours = 0m, BaSystemDocAdjustedHours = 0m,
        ProductionValidationAdjustedHours = 2m, ProjectManagementAdjustedHours = 0m, CollaborationAdjustedHours = 0m,
        SeAssumptions = "D3.js charting library approved. REST API endpoints exist for claims data. Server-side rendering not required.",
        BaAssumptions = "KPI definitions finalized by executive sponsor. No new data sources — all from existing warehouse.",
        CollaborationAssumptions = "Bi-weekly demo to stakeholders. 8 WPRs total. Cross-team coordination with DBA for ETL.",
        GeneralAssumptions = "Dev environment provisioned. Access to claims data warehouse. UAT users identified.",
        AdjustedHoursComments = "Added 10 SE hrs for complex chart interactions. Added 5 BA for KPI workshops. Added 2 PV for exec demo prep.",
        TotalActualHours = 95.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-4), TimeForEstimates = 6.0m,
        UseTestCasesForEstimate = false, TestCasesSimple = 0, TestCasesMedium = 0, TestCasesComplex = 0, TestCasesVeryComplex = 0, TestCaseIterations = 1,
        CreatedDate = DateTime.UtcNow.AddDays(-20), LastModifiedDate = DateTime.UtcNow.AddDays(-4), CreatedBy = "MJohnson", VersionNumber = 4,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-301", ComponentType = "Reports", Description = "Claims volume daily/weekly/monthly report with trend lines", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 85m, TotalHours = 85m, Notes = "SSRS with date range params" },
            new() { LineNumber = 2, RequirementId = "REQ-302", ComponentType = "Reports", Description = "Provider network utilization and geo-access report", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 51m, TotalHours = 51m, Notes = "Map visualization" },
            new() { LineNumber = 3, RequirementId = "REQ-303", ComponentType = "DBManipulation", Description = "ETL jobs for KPI aggregation — hourly refresh", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 31.875m, TotalHours = 31.875m, Notes = "Incremental load" },
            new() { LineNumber = 4, RequirementId = "REQ-304", ComponentType = "Webpage", Description = "Dashboard web interface — responsive D3.js charts", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 20m, TotalHours = 20m, Notes = "SPA with drill-down" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 8, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 30.00m, Notes = "BRD through Test Results" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 4, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 15.00m, Notes = "Bi-weekly executive demos" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 3, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 11.25m, Notes = "Sprint + DBA coordination" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 0, MeetingDurationMinutes = 0, NumberOfParticipants = 0, ParticipantPrepTimeMinutes = 0, TotalHours = 0m, Notes = "Manual testing only" },
        }
    };

    private static ProjectEntity CreateXLargeProject() => new()
    {
        ProjectName = "Member Self-Service Portal",
        ChangeOrderId = "CO-2026-0523",
        ProjectDescription = "Web-based member portal for eligibility verification, ID card requests, benefit inquiries, and claims status tracking.",
        EstimatedBy = "APatel", ReviewedBy = "JSmith",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 428.63m, GrandTotalHours = 952.18m, TShirtSize = "X-Large",
        CollaborationHours = 75.00m,
        DevelopmentAdjustedHours = 15m, AnalysisAdjustedHours = 3m, BusinessDesignAdjustedHours = 5m,
        SystemTestingAdjustedHours = 8m, PromotionAdjustedHours = 0m, BaSystemDocAdjustedHours = 0m,
        ProductionValidationAdjustedHours = 0m, ProjectManagementAdjustedHours = 5m, CollaborationAdjustedHours = 0m,
        SeAssumptions = "Angular framework approved. Member API gateway exists. OAuth 2.0 authentication. Mobile-responsive required.",
        BaAssumptions = "BRD covers all self-service features. Compliance review for PHI display completed. WCAG 2.1 AA required.",
        CollaborationAssumptions = "10 WPRs. Weekly client status. Bi-weekly accessibility review with compliance.",
        GeneralAssumptions = "AWS hosting approved. SSL provisioned. Performance: <3s page load. 99.9% SLA.",
        AdjustedHoursComments = "Added 15 SE for accessibility. Added 3 analysis for PHI rules. Added 5 design for UX. Added 8 testing for browsers. Added 5 PM for vendor coord.",
        TotalActualHours = 180.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-2), TimeForEstimates = 8.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 15, TestCasesMedium = 12, TestCasesComplex = 8, TestCasesVeryComplex = 3, TestCaseIterations = 2,
        CreatedDate = DateTime.UtcNow.AddDays(-21), LastModifiedDate = DateTime.UtcNow.AddDays(-2), CreatedBy = "APatel", VersionNumber = 5,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-201", ComponentType = "Webpage", Description = "Member login and dashboard — SSO with claims summary", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 90m, TotalHours = 90m, Notes = "OAuth 2.0 + MFA" },
            new() { LineNumber = 2, RequirementId = "REQ-202", ComponentType = "Webpage", Description = "Eligibility search and benefit summary pages", ChangeType = "New", Size = "Medium", Count = 2, BaseHoursPerUnit = 60m, TotalHours = 120m, Notes = "Real-time API" },
            new() { LineNumber = 3, RequirementId = "REQ-203", ComponentType = "Webpage", Description = "ID card request form — digital and physical", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 20m, TotalHours = 20m, Notes = "PDF for digital" },
            new() { LineNumber = 4, RequirementId = "REQ-204", ComponentType = "ProgramsDBStoredProcs", Description = "Member data API services — eligibility, claims, benefits", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 115m, TotalHours = 115m, Notes = "RESTful + FHIR" },
            new() { LineNumber = 5, RequirementId = "REQ-205", ComponentType = "SupportModules", Description = "Email/SMS notification service", ChangeType = "New", Size = "Medium", Count = 3, BaseHoursPerUnit = 11.875m, TotalHours = 35.625m, Notes = "SendGrid integration" },
            new() { LineNumber = 6, RequirementId = "REQ-206", ComponentType = "DatabaseReview", Description = "Member portal schema — session and audit tables", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 8.125m, TotalHours = 8.125m, Notes = "HIPAA audit trail" },
            new() { LineNumber = 7, RequirementId = "REQ-207", ComponentType = "TestAutomationUFT", Description = "Automated regression — login, eligibility, cards", ChangeType = "New", Size = "Medium", Count = 5, BaseHoursPerUnit = 8m, TotalHours = 40m, Notes = "Selenium + UFT" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 37.50m, Notes = "Full lifecycle" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Weekly + demos" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 3, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 11.25m, Notes = "Sprint + arch review" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 2, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 7.50m, Notes = "Test strategy" },
        }
    };

    private static ProjectEntity CreateXL1Project() => new()
    {
        ProjectName = "Claims Processing Enhancement",
        ChangeOrderId = "CO-2026-0451",
        ProjectDescription = "Modernize claims adjudication workflow with automated rules engine, K2 integration, and real-time eligibility verification.",
        EstimatedBy = "JSmith", ReviewedBy = "LThompson",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 674.13m, GrandTotalHours = 1525.80m, TShirtSize = "XL1",
        CollaborationHours = 93.75m,
        DevelopmentAdjustedHours = 20m, AnalysisAdjustedHours = 5m, BusinessDesignAdjustedHours = 10m,
        SystemTestingAdjustedHours = 15m, PromotionAdjustedHours = 3m, BaSystemDocAdjustedHours = 2m,
        ProductionValidationAdjustedHours = 5m, ProjectManagementAdjustedHours = 10m, CollaborationAdjustedHours = 5m,
        SeAssumptions = "K2 v5.4 platform available. Existing claims tables extensible. Rules engine uses BRMS pattern.",
        BaAssumptions = "BRD and BDD approved. Business rules for all 47 claim types documented.",
        CollaborationAssumptions = "Full 10 WPRs. Weekly client. Monthly steering. Cross-team with eligibility.",
        GeneralAssumptions = "3 environments available. Performance testing for rules engine. Parallel run 2 weeks.",
        AdjustedHoursComments = "Added 20 SE for legacy compat. Added 15 testing for 47 claim types. Added 10 PM for K2 vendor. Added 5 collab for steering.",
        TotalActualHours = 320.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-5), TimeForEstimates = 12.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 25, TestCasesMedium = 20, TestCasesComplex = 15, TestCasesVeryComplex = 5, TestCaseIterations = 3,
        CreatedDate = DateTime.UtcNow.AddDays(-30), LastModifiedDate = DateTime.UtcNow.AddDays(-5), CreatedBy = "JSmith", VersionNumber = 6,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-101", ComponentType = "PowerBuilderWindows", Description = "Claims Entry — automated rules results display", ChangeType = "Change", Size = "Large", Count = 2, BaseHoursPerUnit = 100m, TotalHours = 200m, Notes = "Modify existing" },
            new() { LineNumber = 2, RequirementId = "REQ-102", ComponentType = "K2Workflow", Description = "Claims approval — 5-step routing with auto-adjudication", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 200m, TotalHours = 200m, Notes = "Parallel paths" },
            new() { LineNumber = 3, RequirementId = "REQ-103", ComponentType = "K2SmartForm", Description = "Claims review form with attachments and history", ChangeType = "New", Size = "Medium", Count = 2, BaseHoursPerUnit = 50m, TotalHours = 100m, Notes = "Document viewer" },
            new() { LineNumber = 4, RequirementId = "REQ-104", ComponentType = "ProgramsDBStoredProcs", Description = "Rules engine — 47 claim type procedures", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 115m, TotalHours = 115m, Notes = "BRMS pattern" },
            new() { LineNumber = 5, RequirementId = "REQ-105", ComponentType = "DatabaseReview", Description = "Claims schema — rules tables, audit, workflow state", ChangeType = "New", Size = "Small", Count = 1, BaseHoursPerUnit = 8.125m, TotalHours = 8.125m, Notes = "3 new tables" },
            new() { LineNumber = 6, RequirementId = "REQ-106", ComponentType = "Reports", Description = "Claims metrics — auto-adjudication rate, cycle time", ChangeType = "New", Size = "Medium", Count = 1, BaseHoursPerUnit = 51m, TotalHours = 51m, Notes = "Dashboard feed" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 37.50m, Notes = "Full lifecycle" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Weekly + steering" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Sprint + arch" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Regression strategy" },
        }
    };

    private static ProjectEntity CreateXL2Project() => new()
    {
        ProjectName = "Provider Network Management System",
        ChangeOrderId = "CO-2026-0612",
        ProjectDescription = "End-to-end provider credentialing, contracting, and network adequacy management with CAQH integration.",
        EstimatedBy = "LThompson", ReviewedBy = "DGarcia",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 1427.53m, GrandTotalHours = 2850.00m, TShirtSize = "XL2",
        CollaborationHours = 93.75m,
        DevelopmentAdjustedHours = 30m, AnalysisAdjustedHours = 8m, BusinessDesignAdjustedHours = 15m,
        SystemTestingAdjustedHours = 20m, PromotionAdjustedHours = 5m, BaSystemDocAdjustedHours = 3m,
        ProductionValidationAdjustedHours = 8m, ProjectManagementAdjustedHours = 15m, CollaborationAdjustedHours = 10m,
        SeAssumptions = "CAQH API available. Provider DB extensible. K2 v5.4 for credentialing. GIS for geo-access.",
        BaAssumptions = "CMS network adequacy rules documented. State credentialing checklists finalized.",
        CollaborationAssumptions = "Full WPRs. Weekly client + steering. Monthly compliance. Cross-team with claims.",
        GeneralAssumptions = "GIS server provisioned. CAQH sandbox access. Load test for 50K providers.",
        AdjustedHoursComments = "Added 30 SE for CAQH. Added 20 testing for geo-access. Added 15 PM for multi-vendor.",
        TotalActualHours = 650.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-7), TimeForEstimates = 16.0m,
        UseTestCasesForEstimate = false, TestCasesSimple = 0, TestCasesMedium = 0, TestCasesComplex = 0, TestCasesVeryComplex = 0, TestCaseIterations = 1,
        CreatedDate = DateTime.UtcNow.AddDays(-45), LastModifiedDate = DateTime.UtcNow.AddDays(-7), CreatedBy = "LThompson", VersionNumber = 7,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-401", ComponentType = "PowerBuilderWindows", Description = "Provider credentialing — intake and tracking", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 125m, TotalHours = 375m, Notes = "15+ screens" },
            new() { LineNumber = 2, RequirementId = "REQ-402", ComponentType = "K2Workflow", Description = "Credentialing approval — committee review routing", ChangeType = "New", Size = "Large", Count = 2, BaseHoursPerUnit = 200m, TotalHours = 400m, Notes = "Multi-level" },
            new() { LineNumber = 3, RequirementId = "REQ-403", ComponentType = "Webpage", Description = "Provider self-service — application and docs", ChangeType = "New", Size = "Large", Count = 2, BaseHoursPerUnit = 90m, TotalHours = 180m, Notes = "Angular + upload" },
            new() { LineNumber = 4, RequirementId = "REQ-404", ComponentType = "ProgramsDBStoredProcs", Description = "Network adequacy GIS engine — drive time calcs", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 294.40m, TotalHours = 294.40m, Notes = "PostGIS" },
            new() { LineNumber = 5, RequirementId = "REQ-405", ComponentType = "Reports", Description = "Provider directory and compliance dashboards", ChangeType = "New", Size = "Large", Count = 2, BaseHoursPerUnit = 85m, TotalHours = 170m, Notes = "CMS formats" },
            new() { LineNumber = 6, RequirementId = "REQ-406", ComponentType = "DatabaseReview", Description = "Provider schema — credentialing, contracts, geo", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 8.125m, TotalHours = 8.125m, Notes = "Major redesign" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 37.50m, Notes = "Full lifecycle" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Weekly status" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Architecture" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "E2E coord" },
        }
    };

    private static ProjectEntity CreateXL3Project() => new()
    {
        ProjectName = "Pharmacy Benefits Modernization",
        ChangeOrderId = "CO-2026-0701",
        ProjectDescription = "Complete pharmacy claims, formulary management, and PBM integration overhaul with real-time drug interaction checking.",
        EstimatedBy = "DGarcia", ReviewedBy = "SAnderson",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 2258.18m, GrandTotalHours = 3575.00m, TShirtSize = "XL3",
        CollaborationHours = 93.75m,
        DevelopmentAdjustedHours = 40m, AnalysisAdjustedHours = 10m, BusinessDesignAdjustedHours = 20m,
        SystemTestingAdjustedHours = 25m, PromotionAdjustedHours = 5m, BaSystemDocAdjustedHours = 5m,
        ProductionValidationAdjustedHours = 10m, ProjectManagementAdjustedHours = 20m, CollaborationAdjustedHours = 10m,
        SeAssumptions = "PBM EDI NCPDP D.0 specs available. Drug database (Medi-Span) licensed. Real-time DUR approved.",
        BaAssumptions = "Formulary tiers (6) defined. Step therapy for top 200 drugs. PA criteria for specialty drugs.",
        CollaborationAssumptions = "Full WPRs. Weekly client + PBM vendor. Bi-weekly pharmacy committee.",
        GeneralAssumptions = "PBM switch vendor timeline aligned. Drug file monthly updates. <2s claim adjudication.",
        AdjustedHoursComments = "Added 40 SE for DUR engine. Added 25 testing for drug validation. Added 20 PM for PBM vendor.",
        TotalActualHours = 1100.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-10), TimeForEstimates = 20.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 40, TestCasesMedium = 30, TestCasesComplex = 20, TestCasesVeryComplex = 10, TestCaseIterations = 3,
        CreatedDate = DateTime.UtcNow.AddDays(-60), LastModifiedDate = DateTime.UtcNow.AddDays(-10), CreatedBy = "DGarcia", VersionNumber = 8,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-501", ComponentType = "PowerBuilderWindows", Description = "Pharmacy claims adjudication — real-time DUR", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 125m, TotalHours = 500m, Notes = "Entry + override" },
            new() { LineNumber = 2, RequirementId = "REQ-502", ComponentType = "K2Workflow", Description = "Prior auth and step therapy workflows", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 200m, TotalHours = 600m, Notes = "Pharmacy routing" },
            new() { LineNumber = 3, RequirementId = "REQ-503", ComponentType = "ProgramsDBStoredProcs", Description = "Formulary rules engine and DUR checking", ChangeType = "New", Size = "Large", Count = 2, BaseHoursPerUnit = 294.40m, TotalHours = 588.80m, Notes = "Real-time + batch" },
            new() { LineNumber = 4, RequirementId = "REQ-504", ComponentType = "Webpage", Description = "Pharmacy portal — drug lookup and cost estimator", ChangeType = "New", Size = "Large", Count = 2, BaseHoursPerUnit = 90m, TotalHours = 180m, Notes = "Drug search + tiers" },
            new() { LineNumber = 5, RequirementId = "REQ-505", ComponentType = "Reports", Description = "Drug utilization and rebate analysis reports", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 85m, TotalHours = 255m, Notes = "CMS PDE format" },
            new() { LineNumber = 6, RequirementId = "REQ-506", ComponentType = "SupportModules", Description = "PBM EDI modules (NCPDP D.0, 835, 837P)", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 26.875m, TotalHours = 134.375m, Notes = "RelayHealth" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 37.50m, Notes = "Full lifecycle" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Weekly + pharma committee" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Sprint + architecture" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "DUR test data" },
        }
    };

    private static ProjectEntity CreateXL4Project() => new()
    {
        ProjectName = "Medicaid Eligibility & Enrollment Platform",
        ChangeOrderId = "CO-2026-0815",
        ProjectDescription = "State Medicaid eligibility determination, enrollment processing, MAGI income calculation with federal hub integration.",
        EstimatedBy = "SAnderson", ReviewedBy = "BNguyen",
        PmEffortPercentage = 15m,
        TotalDevelopmentHours = 3278.20m, GrandTotalHours = 4750.00m, TShirtSize = "XL4",
        CollaborationHours = 93.75m,
        DevelopmentAdjustedHours = 50m, AnalysisAdjustedHours = 15m, BusinessDesignAdjustedHours = 25m,
        SystemTestingAdjustedHours = 30m, PromotionAdjustedHours = 10m, BaSystemDocAdjustedHours = 8m,
        ProductionValidationAdjustedHours = 15m, ProjectManagementAdjustedHours = 25m, CollaborationAdjustedHours = 15m,
        SeAssumptions = "Federal hub connectivity approved. MAGI per ACA. Batch and real-time paths. Oracle 19c.",
        BaAssumptions = "42 CFR Part 435 documented. State policies finalized. 72 eligibility categories mapped.",
        CollaborationAssumptions = "Full WPRs. Weekly client + CMS calls. Monthly steering. Quarterly federal review.",
        GeneralAssumptions = "Federal hub sandbox. MAGI test scenarios from CMS. 15M checks/day. DR site required.",
        AdjustedHoursComments = "Added 50 SE for federal hub. Added 30 testing for 72 categories. Added 25 PM for CMS gates.",
        TotalActualHours = 1800.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-14), TimeForEstimates = 24.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 60, TestCasesMedium = 45, TestCasesComplex = 30, TestCasesVeryComplex = 15, TestCaseIterations = 3,
        CreatedDate = DateTime.UtcNow.AddDays(-90), LastModifiedDate = DateTime.UtcNow.AddDays(-14), CreatedBy = "SAnderson", VersionNumber = 9,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-601", ComponentType = "PowerBuilderWindows", Description = "Eligibility determination — MAGI and non-MAGI", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 125m, TotalHours = 625m, Notes = "Rule-driven UI" },
            new() { LineNumber = 2, RequirementId = "REQ-602", ComponentType = "K2Workflow", Description = "Enrollment and redetermination workflows", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 200m, TotalHours = 800m, Notes = "CMS timelines" },
            new() { LineNumber = 3, RequirementId = "REQ-603", ComponentType = "ProgramsDBStoredProcs", Description = "MAGI income calculation — household rules", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 294.40m, TotalHours = 883.20m, Notes = "IRS verification" },
            new() { LineNumber = 4, RequirementId = "REQ-604", ComponentType = "Webpage", Description = "Member enrollment portal — application wizard", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 90m, TotalHours = 270m, Notes = "Save/resume" },
            new() { LineNumber = 5, RequirementId = "REQ-605", ComponentType = "K2SmartForm", Description = "Eligibility application intake — 12 program types", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 90m, TotalHours = 360m, Notes = "Dynamic fields" },
            new() { LineNumber = 6, RequirementId = "REQ-606", ComponentType = "Reports", Description = "Eligibility, enrollment, and T-MSIS extracts", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 85m, TotalHours = 340m, Notes = "Federal formats" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 37.50m, Notes = "Full lifecycle" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Weekly + CMS" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Sprint + arch" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Federal hub" },
        }
    };

    private static ProjectEntity CreateXL5Project() => new()
    {
        ProjectName = "Enterprise Data Warehouse Migration",
        ChangeOrderId = "CO-2026-0920",
        ProjectDescription = "Full DW redesign with real-time feeds, HEDIS reporting, predictive analytics, and CMS Star Ratings.",
        EstimatedBy = "BNguyen", ReviewedBy = "TReddy",
        PmEffortPercentage = 18m,
        TotalDevelopmentHours = 3318.95m, GrandTotalHours = 5500.00m, TShirtSize = "XL5",
        CollaborationHours = 112.50m,
        DevelopmentAdjustedHours = 60m, AnalysisAdjustedHours = 15m, BusinessDesignAdjustedHours = 30m,
        SystemTestingAdjustedHours = 40m, PromotionAdjustedHours = 10m, BaSystemDocAdjustedHours = 10m,
        ProductionValidationAdjustedHours = 20m, ProjectManagementAdjustedHours = 30m, CollaborationAdjustedHours = 15m,
        SeAssumptions = "Snowflake approved. Informatica for ETL. Kafka for streaming. Sources documented.",
        BaAssumptions = "HEDIS 2026 specs finalized. Star Rating methodology documented. 200+ mappings.",
        CollaborationAssumptions = "Full WPRs. Weekly client. Monthly governance. Quarterly CMS. 6 source teams.",
        GeneralAssumptions = "Snowflake provisioned. Kafka cluster sized. 10-year historical migration. 90-day parallel.",
        AdjustedHoursComments = "Added 60 SE for Kafka. Added 40 testing for 200 mappings. Added 30 PM for 6 teams.",
        TotalActualHours = 2200.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-14), TimeForEstimates = 28.0m,
        UseTestCasesForEstimate = false, TestCasesSimple = 0, TestCasesMedium = 0, TestCasesComplex = 0, TestCasesVeryComplex = 0, TestCaseIterations = 1,
        CreatedDate = DateTime.UtcNow.AddDays(-120), LastModifiedDate = DateTime.UtcNow.AddDays(-14), CreatedBy = "BNguyen", VersionNumber = 10,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-701", ComponentType = "ProgramsDBStoredProcs", Description = "ETL pipelines — 200 source-to-target mappings", ChangeType = "New", Size = "Large", Count = 8, BaseHoursPerUnit = 294.40m, TotalHours = 2355.20m, Notes = "Informatica + SQL" },
            new() { LineNumber = 2, RequirementId = "REQ-702", ComponentType = "Reports", Description = "HEDIS quality + CMS Star Ratings", ChangeType = "New", Size = "Large", Count = 6, BaseHoursPerUnit = 85m, TotalHours = 510m, Notes = "NCQA-certified" },
            new() { LineNumber = 3, RequirementId = "REQ-703", ComponentType = "Webpage", Description = "Analytics dashboard — self-service BI", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 90m, TotalHours = 270m, Notes = "Tableau + custom" },
            new() { LineNumber = 4, RequirementId = "REQ-704", ComponentType = "DBManipulation", Description = "Data lake ingestion — Kafka + batch", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 31.875m, TotalHours = 159.375m, Notes = "Real-time + daily" },
            new() { LineNumber = 5, RequirementId = "REQ-705", ComponentType = "DatabaseReview", Description = "Enterprise schema — star + data vault", ChangeType = "New", Size = "Large", Count = 3, BaseHoursPerUnit = 8.125m, TotalHours = 24.375m, Notes = "Snowflake + dbt" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 50.00m, Notes = "Larger team" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 25.00m, Notes = "Weekly + governance" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Sprint + data sync" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 18.75m, Notes = "Data quality" },
        }
    };

    private static ProjectEntity CreateXL6Project() => new()
    {
        ProjectName = "Multi-State Claims Platform Consolidation",
        ChangeOrderId = "CO-2026-1005",
        ProjectDescription = "Consolidate 3 state Medicaid claims platforms into unified multi-tenant architecture with state configs.",
        EstimatedBy = "TReddy", ReviewedBy = "JDavis",
        PmEffortPercentage = 18m,
        TotalDevelopmentHours = 4404.48m, GrandTotalHours = 6500.00m, TShirtSize = "XL6",
        CollaborationHours = 131.25m,
        DevelopmentAdjustedHours = 80m, AnalysisAdjustedHours = 20m, BusinessDesignAdjustedHours = 35m,
        SystemTestingAdjustedHours = 50m, PromotionAdjustedHours = 15m, BaSystemDocAdjustedHours = 12m,
        ProductionValidationAdjustedHours = 25m, ProjectManagementAdjustedHours = 40m, CollaborationAdjustedHours = 20m,
        SeAssumptions = "Multi-tenant with state isolation. Shared codebase, config-driven. Unique fee schedules per state.",
        BaAssumptions = "State A, B, C rules documented. Gap analysis complete. 350 consolidated rules.",
        CollaborationAssumptions = "Per-state WPRs (30 total). Weekly per-state. Monthly cross-state. Quarterly gates.",
        GeneralAssumptions = "Phased: State A (month 8), B (10), C (12). Parallel processing. Rollback required.",
        AdjustedHoursComments = "Added 80 SE for multi-tenant. Added 50 testing for cross-state. Added 40 PM for 3-state coordination.",
        TotalActualHours = 3100.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-20), TimeForEstimates = 32.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 80, TestCasesMedium = 60, TestCasesComplex = 40, TestCasesVeryComplex = 20, TestCaseIterations = 3,
        CreatedDate = DateTime.UtcNow.AddDays(-150), LastModifiedDate = DateTime.UtcNow.AddDays(-20), CreatedBy = "TReddy", VersionNumber = 12,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-801", ComponentType = "PowerBuilderWindows", Description = "Claims screens — multi-state config-driven UI", ChangeType = "New", Size = "Large", Count = 8, BaseHoursPerUnit = 125m, TotalHours = 1000m, Notes = "Shared + overrides" },
            new() { LineNumber = 2, RequirementId = "REQ-802", ComponentType = "K2Workflow", Description = "State-specific claims routing workflows", ChangeType = "New", Size = "Large", Count = 6, BaseHoursPerUnit = 200m, TotalHours = 1200m, Notes = "Per-state rules" },
            new() { LineNumber = 3, RequirementId = "REQ-803", ComponentType = "ProgramsDBStoredProcs", Description = "Multi-state pricing and edit engine", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 294.40m, TotalHours = 1177.60m, Notes = "350 rules" },
            new() { LineNumber = 4, RequirementId = "REQ-804", ComponentType = "Reports", Description = "Per-state compliance and financial reports", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 85m, TotalHours = 425m, Notes = "State formats" },
            new() { LineNumber = 5, RequirementId = "REQ-805", ComponentType = "Webpage", Description = "Multi-state provider portals with branding", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 90m, TotalHours = 360m, Notes = "White-label" },
            new() { LineNumber = 6, RequirementId = "REQ-806", ComponentType = "SupportModules", Description = "EDI 837/835 per-state companion guides", ChangeType = "New", Size = "Large", Count = 9, BaseHoursPerUnit = 26.875m, TotalHours = 241.875m, Notes = "3×3 types" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 50.00m, Notes = "Multi-state team" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 8, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 40.00m, Notes = "Per-state + cross" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 5, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 25.00m, Notes = "Arch + migration" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 4, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 15.00m, Notes = "Cross-state regression" },
        }
    };

    private static ProjectEntity CreateXL7Project() => new()
    {
        ProjectName = "Integrated Care Management Platform",
        ChangeOrderId = "CO-2026-1110",
        ProjectDescription = "Population health, care coordination, utilization management, and predictive risk scoring with HIE integration.",
        EstimatedBy = "JDavis", ReviewedBy = "CWilson",
        PmEffortPercentage = 20m,
        TotalDevelopmentHours = 4757.60m, GrandTotalHours = 7250.00m, TShirtSize = "XL7",
        CollaborationHours = 150.00m,
        DevelopmentAdjustedHours = 100m, AnalysisAdjustedHours = 25m, BusinessDesignAdjustedHours = 40m,
        SystemTestingAdjustedHours = 60m, PromotionAdjustedHours = 15m, BaSystemDocAdjustedHours = 15m,
        ProductionValidationAdjustedHours = 30m, ProjectManagementAdjustedHours = 50m, CollaborationAdjustedHours = 25m,
        SeAssumptions = "HIE HL7 FHIR R4 APIs. HCC risk model. EHR via SMART on FHIR. Mobile for care managers.",
        BaAssumptions = "15 chronic condition protocols. InterQual licensed. HEDIS/Stars mapped to interventions.",
        CollaborationAssumptions = "Full WPRs. Weekly with 3 clinical teams. Monthly medical director. HIE vendor.",
        GeneralAssumptions = "FHIR server deployed. Clinical data normalized. 2-week training. Phased by condition.",
        AdjustedHoursComments = "Added 100 SE for HIE FHIR. Added 60 testing for clinical scenarios. Added 50 PM for clinical stakeholders.",
        TotalActualHours = 3500.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-30), TimeForEstimates = 36.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 100, TestCasesMedium = 75, TestCasesComplex = 50, TestCasesVeryComplex = 25, TestCaseIterations = 3,
        CreatedDate = DateTime.UtcNow.AddDays(-180), LastModifiedDate = DateTime.UtcNow.AddDays(-30), CreatedBy = "JDavis", VersionNumber = 14,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-901", ComponentType = "PowerBuilderWindows", Description = "Care management — assessments, plans, interventions", ChangeType = "New", Size = "Large", Count = 10, BaseHoursPerUnit = 125m, TotalHours = 1250m, Notes = "15 conditions" },
            new() { LineNumber = 2, RequirementId = "REQ-902", ComponentType = "K2Workflow", Description = "Care plan approval and referral routing", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 200m, TotalHours = 1000m, Notes = "Clinical decision" },
            new() { LineNumber = 3, RequirementId = "REQ-903", ComponentType = "ProgramsDBStoredProcs", Description = "HCC risk stratification and alert engine", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 294.40m, TotalHours = 1177.60m, Notes = "ML scoring" },
            new() { LineNumber = 4, RequirementId = "REQ-904", ComponentType = "Webpage", Description = "Care coordination portal — shared plans", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 90m, TotalHours = 450m, Notes = "FHIR timeline" },
            new() { LineNumber = 5, RequirementId = "REQ-905", ComponentType = "K2SmartForm", Description = "Clinical assessment forms — condition-specific", ChangeType = "New", Size = "Large", Count = 6, BaseHoursPerUnit = 90m, TotalHours = 540m, Notes = "Validated instruments" },
            new() { LineNumber = 6, RequirementId = "REQ-906", ComponentType = "Reports", Description = "Population health and outcomes dashboards", ChangeType = "New", Size = "Large", Count = 4, BaseHoursPerUnit = 85m, TotalHours = 340m, Notes = "HEDIS + KPIs" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 5, ParticipantPrepTimeMinutes = 15, TotalHours = 62.50m, Notes = "Large clinical team" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 8, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 40.00m, Notes = "Weekly + medical dir" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 6, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 30.00m, Notes = "Sprint + clinical" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 4, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 15.00m, Notes = "Clinical scenarios" },
        }
    };

    private static ProjectEntity CreateXL8Project() => new()
    {
        ProjectName = "Next-Gen MMIS Platform Replacement",
        ChangeOrderId = "CO-2026-1200",
        ProjectDescription = "Complete MMIS replacement — claims, eligibility, pharmacy, financials, provider, and member services.",
        EstimatedBy = "CWilson", ReviewedBy = "JSmith",
        PmEffortPercentage = 20m,
        TotalDevelopmentHours = 6510.75m, GrandTotalHours = 9800.00m, TShirtSize = "XL8",
        CollaborationHours = 187.50m,
        DevelopmentAdjustedHours = 150m, AnalysisAdjustedHours = 40m, BusinessDesignAdjustedHours = 60m,
        SystemTestingAdjustedHours = 80m, PromotionAdjustedHours = 25m, BaSystemDocAdjustedHours = 20m,
        ProductionValidationAdjustedHours = 40m, ProjectManagementAdjustedHours = 75m, CollaborationAdjustedHours = 30m,
        SeAssumptions = "Microservices on Azure. COBOL migration to .NET. CMS MECT certification. 99.99% uptime.",
        BaAssumptions = "All 7 MMIS modules (MECT). Federal 90/10 DDI funding approved. IV&V vendor selected.",
        CollaborationAssumptions = "Full WPRs per module. Weekly per-module. Monthly CMS gates. Quarterly IV&V.",
        GeneralAssumptions = "CMS gate reviews. Phased 18-month rollout. DR/BC tested. FedRAMP moderate.",
        AdjustedHoursComments = "Added 150 SE for COBOL migration. Added 80 testing for CMS cert. Added 75 PM for federal/state + IV&V.",
        TotalActualHours = 4500.0m, ActualHoursAsOfDate = DateTime.UtcNow.AddDays(-45), TimeForEstimates = 48.0m,
        UseTestCasesForEstimate = true, TestCasesSimple = 150, TestCasesMedium = 100, TestCasesComplex = 75, TestCasesVeryComplex = 40, TestCaseIterations = 4,
        CreatedDate = DateTime.UtcNow.AddDays(-240), LastModifiedDate = DateTime.UtcNow.AddDays(-45), CreatedBy = "CWilson", VersionNumber = 18,
        Components = new List<ComponentEntryEntity>
        {
            new() { LineNumber = 1, RequirementId = "REQ-1001", ComponentType = "PowerBuilderWindows", Description = "Core MMIS — claims, eligibility, provider, member, financial", ChangeType = "New", Size = "Large", Count = 12, BaseHoursPerUnit = 125m, TotalHours = 1500m, Notes = "7 modules" },
            new() { LineNumber = 2, RequirementId = "REQ-1002", ComponentType = "K2Workflow", Description = "Business process workflows — all 7 modules", ChangeType = "New", Size = "Large", Count = 8, BaseHoursPerUnit = 200m, TotalHours = 1600m, Notes = "CMS-mandated" },
            new() { LineNumber = 3, RequirementId = "REQ-1003", ComponentType = "ProgramsDBStoredProcs", Description = "Claims pricing, edits, financial, reconciliation", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 294.40m, TotalHours = 1472m, Notes = "COBOL conversion" },
            new() { LineNumber = 4, RequirementId = "REQ-1004", ComponentType = "Webpage", Description = "Provider, member, and state agency portals", ChangeType = "New", Size = "Large", Count = 6, BaseHoursPerUnit = 90m, TotalHours = 540m, Notes = "3 user bases" },
            new() { LineNumber = 5, RequirementId = "REQ-1005", ComponentType = "Reports", Description = "Federal (CMS-64, T-MSIS, PERM) and state reports", ChangeType = "New", Size = "Large", Count = 8, BaseHoursPerUnit = 85m, TotalHours = 680m, Notes = "Mandatory formats" },
            new() { LineNumber = 6, RequirementId = "REQ-1006", ComponentType = "K2SmartForm", Description = "Enrollment, appeals, grievances, prior auth forms", ChangeType = "New", Size = "Large", Count = 5, BaseHoursPerUnit = 90m, TotalHours = 450m, Notes = "Public + internal" },
            new() { LineNumber = 7, RequirementId = "REQ-1007", ComponentType = "SupportModules", Description = "EDI gateway (837/835/270/271/276/277) and batch", ChangeType = "New", Size = "Large", Count = 10, BaseHoursPerUnit = 26.875m, TotalHours = 268.75m, Notes = "HIPAA 5010" },
        },
        CollaborationItems = new List<CollaborationItemEntity>
        {
            new() { LineNumber = 1, TaskName = "WPRs", CollaborationType = "WPRs", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 5, ParticipantPrepTimeMinutes = 15, TotalHours = 62.50m, Notes = "Per-module WPRs" },
            new() { LineNumber = 2, TaskName = "Client Meetings", CollaborationType = "ClientMeetings", NumberOfMeetings = 10, MeetingDurationMinutes = 60, NumberOfParticipants = 5, ParticipantPrepTimeMinutes = 15, TotalHours = 62.50m, Notes = "Weekly + CMS + steering" },
            new() { LineNumber = 3, TaskName = "Internal Meetings", CollaborationType = "InternalMeetings", NumberOfMeetings = 8, MeetingDurationMinutes = 60, NumberOfParticipants = 4, ParticipantPrepTimeMinutes = 15, TotalHours = 40.00m, Notes = "Sprint + arch + integration" },
            new() { LineNumber = 4, TaskName = "Automation Test Collaboration", CollaborationType = "AutomationTestCollaboration", NumberOfMeetings = 4, MeetingDurationMinutes = 60, NumberOfParticipants = 3, ParticipantPrepTimeMinutes = 15, TotalHours = 15.00m, Notes = "CMS cert testing" },
        }
    };
}

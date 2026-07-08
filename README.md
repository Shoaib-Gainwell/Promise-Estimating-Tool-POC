# Gainwell Estimation Tool

> **Component-based project estimation tool** replicating and extending the Excel "PROMISe Estimating Tool" with Initial Estimate and Detailed Estimate workflows.

## Overview

A WPF desktop application (.NET 10, C#) that calculates development effort estimates using weighted component-based methodology. All calculations match the Excel reference exactly - verified by 945+ automated tests.

## Features

- **Component-Based Estimation** - 11 component types (PowerBuilder Windows, Reports, Stored Procs, Webpages, K2 Workflows, etc.) with size/change-type weighted values
- **Automatic Task Derivation** - System Testing (30%), Analysis (5%), Business Design (15%), Promotion (5%), BA System Doc (5%), Production Validation (20%) using Excel ROUNDUP(x, 2)
- **Project Management** - Configurable PM effort % (1-20%) applied to all effective task totals
- **Test Case Estimation** - Alternative system testing calculation using Simple/Medium/Complex/Very Complex test cases with iteration multiplier and defect correction factor
- **Collaboration Tracking** - WPRs, Client Meetings, Internal Meetings, Automation Test Collaboration with per-item adjusted hours
- **Adjusted Hours** - Mid-project re-estimation with per-task-type adjustments, notes, and cascading recalculation
- **Role Breakout** - BA, SE, Tester, PM, and Collaboration hour distribution matching Excel formulas
- **T-Shirt Sizing** - Automatic classification (Small to XL8) based on grand total
- **Project Persistence** - Save/Load via SQLite (Entity Framework Core)
- **Project History** - Browse and reload past estimates
- **Settings UI** - Edit weighted values per component type directly in-app
- **Keyboard Shortcuts** - Ctrl+S (Save), Ctrl+N (New Component), Ctrl+Z (Undo), Delete

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10, WPF (XAML) |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Data Access | Entity Framework Core + SQLite |
| Testing | xUnit (945 tests) |
| Brand | Gainwell Style Guide 2024 colors |

## Project Structure

```
+-- .github/                    # Copilot instructions and project guidelines
+-- References/                 # Excel files for calculation verification
|   +-- CO 23327 002 Final Estimate V1.0.xlsm
+-- InitialEstimatePOC/         # Main WPF application
|   +-- App.xaml                # Global styles
|   +-- MainWindow.xaml         # Initial Estimate UI
|   +-- DetailedEstimateWindow.xaml  # Detailed Estimate UI
|   +-- WelcomeWindow.xaml      # Landing/navigation page
|   +-- ViewModels/             # MVVM ViewModels
|   +-- Models/                 # Data models and enums
|   +-- Data/                   # EF Core context, seeders, weighted values
|   +-- Converters/             # WPF value converters
|   +-- Assets/                 # Logo, icons
+-- InitialEstimatePOC.Tests/   # xUnit test project (945 tests)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows 10/11 (WPF requirement)

## Build and Run

```powershell
cd InitialEstimatePOC
dotnet build
dotnet run
```

## Run Tests

```powershell
cd InitialEstimatePOC.Tests
dotnet test
```

## Calculation Rules (Excel-Verified)

| Task | Formula |
|------|---------|
| Development | Sum of component hours (BaseHrs x Count) |
| System Testing | ROUNDUP(Development x 30%, 2) or Test Case formula |
| Analysis | ROUNDUP((Development + System Testing) x 5%, 2) |
| Business Design | ROUNDUP((Development + System Testing) x 15%, 2) |
| Promotion | ROUNDUP(Development x 5%, 2) |
| BA System Doc | ROUNDUP(Development x 5%, 2) |
| Production Validation | ROUNDUP(System Testing x 20%, 2) |
| PM Effort | ROUNDUP(AllEffectiveTasks x PM%, 2) |
| Grand Total | ROUNDUP(Subtotal, 0) - ceiling to whole number |

## Component Types

| Component | Sizes | Change Types |
|-----------|-------|-------------|
| PowerBuilder Windows | S / M / L | New / Change |
| Reports | S / M / L | New / Change |
| Programs/DB Stored Procs | S / M / L | New / Change |
| Support Modules | S / M / L | New / Change |
| DB Manipulation | S / M / L | New / Change |
| Database Review | S / M / L | New / Change |
| Webpage | S / M / L | New / Change |
| K2 Workflow | S / M / L | New / Change |
| K2 Smart Form | S / M / L | New / Change |
| Test Automation (UFT) | S / M / L | New / Change |
| MISC | S / M / L | New / Change |

## License

Internal use only - Gainwell Technologies.

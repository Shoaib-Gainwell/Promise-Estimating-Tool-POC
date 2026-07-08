# PROMISe Initial Estimate Tool — POC

> **⚠️ This is a Proof of Concept (POC) only — not intended for production use.**

## Overview

A WPF desktop application for generating initial project effort estimates. The tool uses weighted component-based estimation with configurable parameters to calculate development hours across multiple task categories.

## Features

- **Component-Based Estimation** — Add components (PowerBuilder Windows, Reports, Stored Procs, Webpages, K2 Workflows, etc.) with size/change-type to auto-calculate base hours using weighted values
- **Automatic Task Derivation** — System Testing, Analysis, Business Design, Promotion, BA/System Doc, and Production Validation hours derived from development totals
- **Project Management & Reserve** — Configurable PM effort % and PM reserve % applied to subtotals
- **T-Shirt Sizing** — Automatic S/M/L/XL classification based on total hours
- **Collaboration Tracking** — WPRs, Client Meetings, Internal Meetings, Automation Test Collaboration, Consultant/Mentor Effort
- **Adjusted Hours** — Mid-project re-estimation with per-task-type adjustments and comments
- **Test Case Estimation** — Alternative system testing calculation based on Simple/Medium/Complex/Very Complex test cases with iteration multiplier
- **Role Breakout** — BA, SE, Tester, PM hour distribution
- **Assumptions Capture** — SE, BA, Collaboration, and General assumptions fields
- **Project Persistence** — Save/Load projects via SQLite (Entity Framework Core)
- **Project History** — Browse and reload past estimates
- **Settings UI** — Edit weighted values per component type directly in-app
- **Keyboard Shortcuts** — Ctrl+S (Save), Ctrl+N (New Component), Ctrl+Z (Undo), Delete
- **Undo Support** — Revert last action

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | WPF (.NET 10) |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Data Access | Entity Framework Core + SQLite |
| Testing | xUnit + Coverlet |
| IDE | Visual Studio 2022+ |

## Project Structure

```
InitialEstimatePOC/          — Main WPF application
├── Models/                  — ComponentEntry, enums, entity models
├── ViewModels/              — MainViewModel, SettingsViewModel
├── Data/                    — EF Core DbContext, WeightedValues, Seeders
├── Converters/              — WPF value converters
├── Assets/                  — App icon, logo
├── MainWindow.xaml          — Primary estimation UI
├── SettingsWindow.xaml       — Weighted values editor
└── HistoryWindow.xaml        — Saved project browser

InitialEstimatePOC.Tests/    — xUnit test project
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Preview)
- Windows 10/11 (WPF requirement)

## Build & Run

```bash
cd InitialEstimatePOC
dotnet build
dotnet run
```

## Run Tests

```bash
cd InitialEstimatePOC.Tests
dotnet test
```

## Component Types Supported

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

Internal use only — Gainwell Technologies.

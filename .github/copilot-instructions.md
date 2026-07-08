# Gainwell Estimation Tool — Copilot Instructions

## Project Overview
This is the **Gainwell Estimation Tool** — a WPF desktop application (.NET 10, C#) for component-based project estimation. It replicates and extends the Excel "PROMISe Estimating Tool" with Initial Estimate and Detailed Estimate workflows.

## Gainwell Brand — MANDATORY Color & Typography Rules

All UI work MUST use ONLY the colors and fonts from the Gainwell Style Guide 2024. Never invent colors. Never use Tailwind, Material, or generic palette colors.

### Primary Colors
| Name | HEX | RGB | Usage |
|------|-----|-----|-------|
| Vibrant Green | `#00EEAE` | 0, 238, 174 | Accent on dark backgrounds, subtitle text on headers, decorative borders |
| Vibrant Blue | `#1931E3` | 25, 49, 227 | Primary buttons, CTA actions, KPI values, active states |
| Soft Black | `#2B3A44` | 43, 58, 68 | Body text, headings, dark text on light backgrounds |

### Accent Colors
| Name | HEX | RGB | Usage |
|------|-----|-----|-------|
| Dark Green | `#019681` | 1, 150, 129 | SE section accent, totals, header gradient start, positive indicators |
| Teal | `#2199B0` | 33, 153, 176 | Collaboration section labels, secondary accents |
| Light Blue | `#00B1EF` | 0, 177, 239 | Available for informational highlights |
| Lavender | `#8C5CED` | 140, 92, 237 | Help icons, SE role indicators |
| Purple | `#55206E` | 85, 32, 110 | PM Effort accent, danger hover state |
| Navy | `#2E307A` | 46, 48, 122 | BA section accent, button hover, header gradient end |
| Gold | `#E5AB4F` | 229, 171, 79 | PM role indicator, background accents (NOT for text on white — poor contrast) |
| Magenta | `#CC1AC7` | 203, 26, 198 | Danger/delete actions, Collaboration role indicator |

### Neutral Colors
| Name | HEX | RGB | Usage |
|------|-----|-----|-------|
| White | `#FFFFFF` | 255, 255, 255 | Card backgrounds, content areas |
| Gray 1 | `#EBEDF5` | 235, 237, 245 | Page background, subtle hover states, light fills |
| Gray 2 | `#C2C6D2` | 194, 198, 210 | Borders, dividers, selected row state |
| Gray 3 | `#8E99A8` | 142, 153, 168 | Placeholder text, secondary/disabled text |
| Gray 4 | `#5A6978` | 90, 105, 120 | Labels, muted text, field descriptions |

### Approved Gradients (for header banners)
Use 3-stop gradients for smooth transitions:
- **Standard Header**: `#019681` (0%) → `#1A6478` (50%) → `#2E307A` (100%) — Dark Green → blend → Navy
- **Gradient 1** (decorative): `#00EEAE` → `#1931E3` — Green to Blue
- **Gradient 2** (decorative): `#CB1AC6` → `#1931E3` — Magenta to Blue
- **Gradient 3** (decorative): `#00EEAE` → `#2B3A44` — Green to Soft Black

### Typography
- Use the current application font as-is
- **Icon Font**: `Segoe Fluent Icons` or `Segoe MDL2 Assets` — for icon glyphs only

### Color Usage Rules
1. **Primary buttons**: Vibrant Blue (`#1931E3`) background, White text. Hover → Navy (`#2E307A`).
2. **Section differentiation**: SE = Dark Green, BA = Navy, Collaboration = Teal, PM = Purple/Gold.
3. **Text on white backgrounds**: Use Soft Black (`#2B3A44`) for headings, Gray 4 (`#5A6978`) for labels.
4. **Text on dark backgrounds**: Use White for primary text, Vibrant Green (`#00EEAE`) for subtitles.
5. **Borders**: Use `#D4D7E0` (softened Gray 2) for card/grid borders.
6. **Grid lines**: Use `#E8EAF0` (subtle separator).
7. **Danger actions**: Magenta (`#CC1AC7`), never generic red.
8. **Gold (`#E5AB4F`) must NOT be used as foreground text** on white — insufficient contrast. Use only for dots, badges, or tinted backgrounds.

### Derived Tints (acceptable for visual hierarchy)
These are NOT in the brand guide but are derived at ~5-8% opacity for section backgrounds:
- SE sections: `#E6F5F2` (light Dark Green tint)
- BA sections: `#EDF0F8` (light Navy tint)
- Collab sections: `#FDF8EF` (light Gold tint)
- Input areas: `#F5F6FA` (very light neutral)

## Iconography
- Icons must be simple, purposeful, and functional — never decorative
- Use only brand colors for icon fills
- Prefer `Segoe Fluent Icons` glyphs in WPF

## Logo Rules
- Color logo on light/white backgrounds
- Reverse logo on Soft Black or Gray 4 backgrounds
- NEVER alter logo colors, apply effects, twist, or recreate the logo

---

## Architecture & Code Standards

### Technology Stack
- .NET 10, WPF (XAML), C#, Entity Framework Core (SQLite)
- xUnit for testing
- MVVM pattern (ViewModels in `ViewModels/`, Models in `Models/`, Data in `Data/`)

### Calculation Rules (must match Excel exactly)
- All derived percentages use `ROUNDUP(value, 2)` — implemented as `Math.Ceiling(value * 100) / 100`
- System Testing = ROUNDUP(Development × 30%, 2)
- Analysis = ROUNDUP((Development + System Testing) × 5%, 2)
- Business Design = ROUNDUP((Development + System Testing) × 15%, 2)
- Promotion = ROUNDUP(Development × 5%, 2)
- BA System Doc = ROUNDUP(Development × 5%, 2)
- Production Validation = ROUNDUP(System Testing × 20%, 2)
- PM Effort = ROUNDUP(base × PM%, 2) where base includes all effective (calculated + adjusted) values
- Grand Total = ROUNDUP(Subtotal, 0) — ceiling to whole number
- Collaboration = Meetings × (Duration/60 + PrepTime/60) × Participants

### Testing Requirements
- All calculation changes MUST have corresponding unit tests verifying against Excel values
- Reference folder: `References/` — contains Excel files used for verification
- Any `.xlsm` or `.xlsx` file in `References/` should be used to validate calculation accuracy
- Primary reference: `References/CO 23327 002 Final Estimate V1.0.xlsm`
- Current test count: 945 (must not decrease)

### File Structure
```
├── .github/                    # Copilot instructions & project guidelines
├── References/                 # Excel files for calculation verification
│   └── CO 23327 002 Final Estimate V1.0.xlsm
├── InitialEstimatePOC/         # Main WPF application
│   ├── App.xaml                # Global styles
│   ├── MainWindow.xaml         # Initial Estimate UI
│   ├── DetailedEstimateWindow.xaml  # Detailed Estimate UI
│   ├── WelcomeWindow.xaml      # Landing/navigation page
│   ├── ViewModels/             # MVVM ViewModels
│   ├── Models/                 # Data models and enums
│   ├── Data/                   # EF Core context, seeders, weighted values
│   ├── Converters/             # WPF value converters
│   └── Assets/                 # Logo, icons
└── InitialEstimatePOC.Tests/   # xUnit test project
```

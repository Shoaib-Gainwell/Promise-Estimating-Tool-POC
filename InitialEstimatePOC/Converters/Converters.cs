using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC.Converters;

public class ComponentTypeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComponentType ct)
            return WeightedValues.GetDisplayName(ct);
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DecimalFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("N2");
        return "0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && decimal.TryParse(s, out var result))
            return result;
        return 0m;
    }
}

/// <summary>
/// Two-way converter for adjusted hours fields that allows empty/intermediate input.
/// Empty or whitespace → 0m. Displays non-zero values without trailing zeros.
/// </summary>
public class AdjustedHoursConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d && d != 0m)
            return d.ToString("0.##");
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var trimmed = s.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed == "-" || trimmed == ".")
                return 0m;
            if (decimal.TryParse(trimmed, NumberStyles.Number, culture, out var result))
                return result;
        }
        return 0m;
    }
}

/// <summary>
/// Returns Visible when value is 0 (for empty state), Collapsed otherwise.
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i)
            return i == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts enum values to display-friendly strings with spaces.
/// Works for ComponentType, CollaborationType, ChangeType, ComponentSize.
/// </summary>
public class EnumDisplayConverter : IValueConverter
{
    private static readonly Dictionary<ComponentType, string> ComponentTypeNames = new()
    {
        [ComponentType.None] = "— Select —",
        [ComponentType.PowerBuilderWindows] = "PowerBuilder Windows",
        [ComponentType.Reports] = "Reports",
        [ComponentType.ProgramsDBStoredProcs] = "Programs/DB Stored Procedures",
        [ComponentType.SupportModules] = "Support Modules/JOB/JIL.c,.sc,.ctl,.mak,.pl,.ar,.h,.l,.pkb,.pks,.y,.addidx,.awk,.ctl,.cfg,.conf,.html,.lst,.out,.pls,.par,.ps,.prm,.prn,.sql,.shl,.srt,.txt,.xml,.xsd,.xsl",
        [ComponentType.DBManipulation] = "DB Manipulation (SQL, PL/SQL, etc.)",
        [ComponentType.DatabaseReview] = "Database Review",
        [ComponentType.Webpage] = "Webpage (Includes UI, Portal & Intranet)",
        [ComponentType.K2Workflow] = "K2 Workflow",
        [ComponentType.K2SmartForm] = "K2 Smart Form",
        [ComponentType.TestAutomationUFT] = "Test Automation Suites (UFT)",
        [ComponentType.MISC] = "MISC (Server Setup, Webserver Setup, Software Installation, etc.)",
    };

    private static readonly Dictionary<CollaborationType, string> CollaborationTypeNames = new()
    {
        [CollaborationType.WPRs] = "WPRs",
        [CollaborationType.ClientMeetings] = "Client Meetings",
        [CollaborationType.InternalMeetings] = "Internal Meetings",
        [CollaborationType.AutomationTestCollaboration] = "Automation Test Collaboration",
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComponentType ct && ComponentTypeNames.TryGetValue(ct, out var ctName))
            return ctName;
        if (value is CollaborationType clt && CollaborationTypeNames.TryGetValue(clt, out var cltName))
            return cltName;
        if (value is ChangeType chg)
            return chg == ChangeType.None ? "— Select —" : chg.ToString();
        if (value is ComponentSize sz)
            return sz == ComponentSize.None ? "— Select —" : sz.ToString();
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>Converts a notes string to "N/1000" or a limit-warning message.</summary>
public class NotesCharCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int max = parameter is string p && int.TryParse(p, out int parsed) ? parsed : 1000;
        int len = value is string s ? s.Length : 0;
        return len >= max
            ? $"⚠ Character limit reached — cannot exceed {max} characters"
            : $"{len}/{max}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Returns Red when notes string is at the limit (from ConverterParameter, default 1000), otherwise Gray.</summary>
public class NotesCharCountColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int max = parameter is string p && int.TryParse(p, out int parsed) ? parsed : 1000;
        return (value is string s && s.Length >= max)
            ? System.Windows.Media.Brushes.Red
            : System.Windows.Media.Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

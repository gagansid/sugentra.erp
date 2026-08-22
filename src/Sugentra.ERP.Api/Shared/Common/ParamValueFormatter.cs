using System.Collections;
using System.Globalization;

namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>Generic DataType+FormatString value formatter. Not tied to email rendering - any feature that needs to
/// turn a raw value into display text using the same DataType/FormatString metadata (Setting_EmailTemplateParameters,
/// print layouts, PDF export, ...) should call this instead of re-implementing formatting rules.</summary>
public static class ParamValueFormatter
{
    public static string Format(string? dataType, string? formatString, object? value)
    {
        if (value is null) return string.Empty;

        return (dataType ?? "String").Trim().ToLowerInvariant() switch
        {
            "int" => FormatInt(value, formatString),
            "decimal" or "float" => FormatDecimal(value, formatString),
            "date" => FormatDate(value, formatString),
            "month" => FormatMonth(value, formatString),
            "day" => FormatDay(value, formatString),
            "bool" => FormatBool(value, formatString),
            "list" => FormatList(value, formatString),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatInt(object value, string? formatString) =>
        long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var number)
            ? number.ToString(string.IsNullOrWhiteSpace(formatString) ? "0" : formatString, CultureInfo.InvariantCulture)
            : value.ToString() ?? string.Empty;

    private static string FormatDecimal(object value, string? formatString) =>
        decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var number)
            ? number.ToString(string.IsNullOrWhiteSpace(formatString) ? "0.##" : formatString, CultureInfo.InvariantCulture)
            : value.ToString() ?? string.Empty;

    private static string FormatDate(object value, string? formatString)
    {
        var date = value as DateTime? ?? (DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null);
        return date is null
            ? value.ToString() ?? string.Empty
            : date.Value.ToString(string.IsNullOrWhiteSpace(formatString) ? "yyyy-MM-dd" : formatString, CultureInfo.InvariantCulture);
    }

    // FormatString drives both the numeric width (MM/M) and the language of named formats (MMMM/MMM = English, MMMM_id/MMM_id = Indonesian).
    private static string FormatMonth(object value, string? formatString)
    {
        int month;
        if (value is DateTime dt) month = dt.Month;
        else if (!int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out month)) return value.ToString() ?? string.Empty;

        var refDate = new DateTime(2000, month, 1);
        return (string.IsNullOrWhiteSpace(formatString) ? "MM" : formatString) switch
        {
            "MM" => month.ToString("00"),
            "M" => month.ToString(),
            "MMMM" => refDate.ToString("MMMM", CultureInfo.GetCultureInfo("en-US")),
            "MMM" => refDate.ToString("MMM", CultureInfo.GetCultureInfo("en-US")),
            "MMMM_id" => refDate.ToString("MMMM", CultureInfo.GetCultureInfo("id-ID")),
            "MMM_id" => refDate.ToString("MMM", CultureInfo.GetCultureInfo("id-ID")),
            _ => month.ToString("00")
        };
    }

    // Weekday-name formats (dddd/ddd, incl. _id) require a real date to know the day of week; falls back to the
    // numeric day-of-month for plain int values passed with those formats.
    private static string FormatDay(object value, string? formatString)
    {
        var date = value as DateTime? ?? (DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null);
        var format = string.IsNullOrWhiteSpace(formatString) ? "dd" : formatString;

        if (date is null)
        {
            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var dayNumber)
                ? format == "d" ? dayNumber.ToString() : dayNumber.ToString("00")
                : value.ToString() ?? string.Empty;
        }

        return format switch
        {
            "dd" => date.Value.Day.ToString("00"),
            "d" => date.Value.Day.ToString(),
            "dddd" => date.Value.ToString("dddd", CultureInfo.GetCultureInfo("en-US")),
            "ddd" => date.Value.ToString("ddd", CultureInfo.GetCultureInfo("en-US")),
            "dddd_id" => date.Value.ToString("dddd", CultureInfo.GetCultureInfo("id-ID")),
            "ddd_id" => date.Value.ToString("ddd", CultureInfo.GetCultureInfo("id-ID")),
            _ => date.Value.Day.ToString("00")
        };
    }

    // FormatString "TrueText;FalseText" (e.g. "Ya;Tidak") overrides the default Yes/No labels.
    private static string FormatBool(object value, string? formatString)
    {
        var flag = value as bool? ?? (bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var parsed) && parsed);
        if (string.IsNullOrWhiteSpace(formatString) || !formatString.Contains(';')) return flag ? "Yes" : "No";

        var parts = formatString.Split(';');
        return flag ? parts[0] : (parts.Length > 1 ? parts[1] : "No");
    }

    private static string FormatList(object value, string? formatString)
    {
        var separator = string.IsNullOrWhiteSpace(formatString) ? ", " : formatString;
        return value is IEnumerable items and not string
            ? string.Join(separator, items.Cast<object?>().Select(i => i?.ToString() ?? string.Empty))
            : value.ToString() ?? string.Empty;
    }
}

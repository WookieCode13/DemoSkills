namespace EmployeeAPI.Application.Logging;

public static class LogRedaction
{
    public static string MaskEmail(string email)
    {
        var parts = email.Split('@', 2);
        if (parts.Length != 2 || parts[0].Length == 0) return "***";
        var local = parts[0];
        var maskedLocal = local.Length == 1 ? "*" : $"{local[0]}***";
        return $"{maskedLocal}@{parts[1]}";
    }
}
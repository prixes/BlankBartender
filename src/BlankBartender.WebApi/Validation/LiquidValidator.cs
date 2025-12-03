using System.Text.RegularExpressions;

namespace BlankBartender.WebApi.Validation;

public class LiquidValidator : ILiquidValidator
{
    // simple validation: non-empty and no control characters
    private static readonly Regex _invalidChars = new("[\u0000-\u001F]");

    public bool IsValid(string? liquidName)
    {
        if (string.IsNullOrWhiteSpace(liquidName))
            return false;

        if (_invalidChars.IsMatch(liquidName))
            return false;

        return true;
    }
}

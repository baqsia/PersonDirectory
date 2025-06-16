using System.Text.RegularExpressions;

namespace Task.PersonDirectory.Application.Common.ValidationPipeline;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class ValidationRegexUtil
{
    public static bool BeOnlyGeorgianOrOnlyLatin(string value)
    {
        var georgian = GeorgianAlphabetRegex().IsMatch(value);
        var latin = EnglishAlphabetRegex().IsMatch(value);
        return georgian ^ latin; // XOR: only one must be true
    }

    public static bool BeAtLeast18(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }

    [GeneratedRegex("^[ა-ჰ]+$")]
    private static partial Regex GeorgianAlphabetRegex();

    [GeneratedRegex("^[a-zA-Z]+$")]
    private static partial Regex EnglishAlphabetRegex();
}
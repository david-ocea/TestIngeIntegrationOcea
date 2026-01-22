using System.Text.RegularExpressions;

namespace TestIngeIntegrationOceaConsole.Utils;

/// <summary>
/// Utilitaire sur les chaines de caractère.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Remplace les multiples espaces dans une chaine de caractère.
    /// </summary>
    public static string NormalizeWhitespace(string input)
    {
        return Regex.Replace(input ?? string.Empty, "\\s+", " ").Trim();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MediaOrganizerApp.Application.Utilities;

/// <summary>
/// Matches author names across sources that order/spell them differently
/// ("Apelido, Nome" vs "Nome Apelido", accents, middle initials, punctuation).
/// Used to keep provider results relevant when a search operator is a loose match
/// (e.g. the Google Books <c>inauthor:</c> operator without phrase quotes).
/// </summary>
public static class AuthorNameMatcher
{
    /// <summary>
    /// True when at least one of the credited authors is the requested author.
    /// Candidate lists that carry no author information at all are treated as a
    /// match, so a missing field never hides results.
    /// </summary>
    public static bool Matches(string? requested, IEnumerable<string?>? creditedAuthors)
    {
        if (creditedAuthors is null)
            return true;

        var candidates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in creditedAuthors)
        {
            var normalized = Normalize(name);
            if (normalized.Length > 0)
                candidates.Add(normalized);
        }

        // No author data to filter on: keep the item.
        if (candidates.Count == 0)
            return true;

        var wanted = Normalize(requested);
        if (wanted.Length == 0)
            return true;

        foreach (var candidate in candidates)
        {
            if (candidate.Equals(wanted, StringComparison.Ordinal))
                return true;

            // One name containing the other (e.g. an author qualified as "J. Smith").
            if (candidate.Contains(wanted, StringComparison.Ordinal) ||
                wanted.Contains(candidate, StringComparison.Ordinal))
                return true;

            // Same name written with different spacing ("DelBono" / "Del Bono").
            if (candidate.Replace(" ", string.Empty)
                .Equals(wanted.Replace(" ", string.Empty), StringComparison.Ordinal))
                return true;

            // Same person written with a middle name/initial
            // ("James M. Mccaffrey" vs "James McCaffrey"): first + last token.
            if (SameFirstAndLastName(candidate, wanted))
                return true;
        }

        // "Apelido, Nome" vs "Nome Apelido"
        var swapped = Normalize(SwapSurnameOrder(requested));
        return swapped.Length > 0 && candidates.Contains(swapped);
    }

    /// <summary>
    /// Lowercases, removes accents and punctuation, and collapses whitespace so
    /// "António  O'Neill, Jr." becomes "antonio o neill jr".
    /// </summary>
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Strip accents (DelBono/Delbono, António/Antonio), keep letters/digits/spaces.
        var decomposed = name.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (char.IsWhiteSpace(ch))
                sb.Append(' ');
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool SameFirstAndLastName(string left, string right)
    {
        var leftTokens = left.Split(' ');
        var rightTokens = right.Split(' ');
        if (leftTokens.Length < 2 || rightTokens.Length < 2)
            return false;

        return leftTokens[0].Equals(rightTokens[0], StringComparison.Ordinal)
            && leftTokens[^1].Equals(rightTokens[^1], StringComparison.Ordinal);
    }

    private static string SwapSurnameOrder(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var parts = name.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? $"{parts[1]} {parts[0]}" : string.Empty;
    }
}
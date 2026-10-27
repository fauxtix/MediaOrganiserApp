using MediaOrganizerApp.Application.Utilities;

namespace MediaOrganizerApp.Tests;

public sealed class AuthorNameMatcherTests
{
    [Theory]
    [InlineData("James McCaffrey", new[] { "James McCaffrey" }, true)]
    [InlineData("James McCaffrey", new[] { "James M. Mccaffrey" }, true)]
    [InlineData("james mccaffrey", new[] { "James McCaffrey" }, true)]
    [InlineData("James McCaffrey", new[] { "Software Testing", "James D. McCaffrey" }, true)]
    [InlineData("James McCaffrey", new[] { "Christopher Fox" }, false)]
    [InlineData("Christopher Fox", new[] { "Kelvyn Laurence Smith", "Christopher Fox" }, true)]
    [InlineData("Christopher Fox", new[] { "Christopher F. Waythomas" }, false)]
    [InlineData("Eça de Queirós", new[] { "Eca de Queiros" }, true)]
    [InlineData("Emanuele DelBono", new[] { "Emanuele Del Bono" }, true)]
    public void Matches_ComparesNamesLeniently(string requested, string[] credited, bool expected)
    {
        Assert.Equal(expected, AuthorNameMatcher.Matches(requested, credited));
    }

    [Fact]
    public void Matches_AcceptsSurnameFirstOrdering()
    {
        Assert.True(AuthorNameMatcher.Matches("Christie, Agatha", new[] { "Agatha Christie" }));
    }

    [Fact]
    public void Matches_KeepsItemsWithoutAuthorData()
    {
        Assert.True(AuthorNameMatcher.Matches("Qualquer Autor", Array.Empty<string?>()));
        Assert.True(AuthorNameMatcher.Matches("Qualquer Autor", null));
        Assert.True(AuthorNameMatcher.Matches("Qualquer Autor", new string?[] { null, "  " }));
    }

    [Fact]
    public void Matches_KeepsItemsWhenRequestedNameIsUnusable()
    {
        Assert.True(AuthorNameMatcher.Matches(null, new[] { "Qualquer Um" }));
        Assert.True(AuthorNameMatcher.Matches("   ", new[] { "Qualquer Um" }));
    }

    [Theory]
    [InlineData("António O'Neill, Jr.", "ANTONIO ONEILL JR")]
    [InlineData("  Saramago,  José  ", "SARAMAGO JOSE")]
    [InlineData("ÁÉÍÓÚ", "AEIOU")]
    [InlineData("DelBono", "DELBONO")]
    [InlineData(null, "")]
    public void Normalize_StripsAccentsPunctuationAndSpacing(string? input, string expected)
    {
        Assert.Equal(expected, AuthorNameMatcher.Normalize(input));
    }
}
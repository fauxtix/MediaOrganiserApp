using MediaOrganizerApp.Application.Utilities;
using Xunit;

namespace MediaOrganizerApp.Tests
{
    public class ChatSafetyArtifactFilterTests
    {
        [Theory]
        [InlineData("User Safety: safe;")]
        [InlineData("Response Safety: safe")]
        [InlineData("Safety Categories: S1, S2")]
        [InlineData("  \nUser Safety: safe;\n  ")]
        [InlineData("**User Safety:** safe")]
        [InlineData("- Response Safety: safe")]
        [InlineData("Content Safety: safe;")]
        public void IsSafetyOnly_DetectsSafetyClassification(string text)
        {
            Assert.True(ChatSafetyArtifactFilter.IsSafetyOnly(text));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Eric Clapton was an English guitarist.")]
        [InlineData("User Safety: safe;\n\nEric Clapton was an English guitarist.")]
        [InlineData("Here is the answer.\nUser Safety: safe;")]
        public void IsSafetyOnly_ReturnsFalseForRealContent(string? text)
        {
            Assert.False(ChatSafetyArtifactFilter.IsSafetyOnly(text));
        }

        [Fact]
        public void IsSafetyOnly_ReturnsFalseForNull()
        {
            Assert.False(ChatSafetyArtifactFilter.IsSafetyOnly(null));
        }

        [Fact]
        public void StripSafetyLines_RemovesArtifactLinesKeepingContent()
        {
            var input = "User Safety: safe;\n\nEric Clapton was an English guitarist.";
            Assert.Equal("Eric Clapton was an English guitarist.", ChatSafetyArtifactFilter.StripSafetyLines(input));
        }

        [Fact]
        public void StripSafetyLines_RemovesMixedMarkdownArtifactLines()
        {
            var input = "**User Safety:** safe\n- Response Safety: safe\nReal text";
            Assert.Equal("Real text", ChatSafetyArtifactFilter.StripSafetyLines(input));
        }

        [Fact]
        public void StripSafetyLines_LeavesCleanTextUntouched()
        {
            var input = "Eric Clapton was an English guitarist.";
            Assert.Equal(input, ChatSafetyArtifactFilter.StripSafetyLines(input));
        }
    }
}

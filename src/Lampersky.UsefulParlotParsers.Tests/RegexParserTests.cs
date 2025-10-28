using Lampersky.Fluent;
using Xunit;

namespace Lampersky.UsefulParsers.Tests
{
    public class RegexParserTests
    {
        [Theory]
        [InlineData("ab*a", "abbbbba dummy", "abbbbba")]
        [InlineData("[a-z]+[0-9]*", "test12345 dummy", "test12345")]
        [InlineData("abc|def", "def ghi", "def")]
        [InlineData("[^d]*d", "a b c d e f g h", "a b c d")]
        public void ShouldParseRegularExpressionTerm(string pattern, string text, string expected)
        {
            var regex = Terms.Regex(pattern).Then(static x => x.ToString());

            Assert.True(regex.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("ab*a", "abbbbba dummy", "abbbbba")]
        [InlineData("[a-z]+[0-9]*", "test12345 dummy", "test12345")]
        [InlineData("abc|def", "def ghi", "def")]
        [InlineData("[^d]*d", "a b c d e f g h", "a b c d")]
        public void ShouldParseRegularExpressionTermCompiled(string pattern, string text, string expected)
        {
            var regex = Terms.Regex(pattern).Compile().Then(static x => x.ToString());

            Assert.True(regex.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("ab*a", "abbbbba12345", "abbbbba")]
        [InlineData("[a-z]+[0-9]*", "test12345xyz", "test12345")]
        public void ShouldParseRegularExpressionLiteral(string pattern, string text, string expected)
        {
            var regex = Literals.Regex(pattern).Then(static x => x.ToString());

            Assert.True(regex.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("ab*a", "abbbbba12345", "abbbbba")]
        [InlineData("[a-z]+[0-9]*", "test12345xyz", "test12345")]
        public void ShouldParseRegularExpressionLiteralCompiled(string pattern, string text, string expected)
        {
            var regex = Literals.Regex(pattern).Compile().Then(static x => x.ToString());

            Assert.True(regex.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }


    }
}

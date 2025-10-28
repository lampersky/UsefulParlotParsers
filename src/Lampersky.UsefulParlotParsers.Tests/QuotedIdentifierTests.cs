using Lampersky.Fluent;
using Parlot;
using Parlot.Fluent;
using Xunit;

namespace Lampersky.UsefulParsers.Tests
{
    public class QuotedIdentifierTests
    {
        private Parser<String> TermIdentifierParser { get; set; }
        private Parser<String> TermIdentifierCompiledParser { get; set; }

        private Parser<String> LiteralIdentifierParser { get; set; }
        private Parser<String> LiteralIdentifierCompiledParser { get; set; }

        public QuotedIdentifierTests()
        {
            TermIdentifierParser = Terms.QuotedIdentifier(Quotes.SingleQuotationMarks, Quotes.DoubleQuotationMarks, Quotes.BackTicks, Quotes.SquareBrackets)
                .Then(static x => x.ToString());
            TermIdentifierCompiledParser = TermIdentifierParser
                .Compile();
            LiteralIdentifierParser = Literals.QuotedIdentifier(Quotes.SingleQuotationMarks, Quotes.DoubleQuotationMarks, Quotes.BackTicks, Quotes.SquareBrackets)
                .Then(static x => x.ToString());
            LiteralIdentifierCompiledParser = LiteralIdentifierParser
                .Compile();
        }

        [Theory]
        [InlineData("[a]", "a")]
        [InlineData("[abbbb]", "abbbb")]
        [InlineData("[abc123]", "abc123")]
        [InlineData("[abc123!@#$]", "abc123!@#$")]
        [InlineData("[abc\ndef]", "abc\ndef")]
        [InlineData("`a`", "a")]
        [InlineData("`abbbb`", "abbbb")]
        [InlineData("`abc123`", "abc123")]
        [InlineData("`abc123!@#$`", "abc123!@#$")]
        [InlineData("`abc\ndef`", "abc\ndef")]
        public void ShouldParseQuotedIdentifier(string text, string expected)
        {
            Assert.True(TermIdentifierParser.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("[a]", "a")]
        [InlineData("[abbbb]", "abbbb")]
        [InlineData("[abc123]", "abc123")]
        [InlineData("[abc123!@#$]", "abc123!@#$")]
        [InlineData("[abc\ndef]", "abc\ndef")]
        [InlineData("`a`", "a")]
        [InlineData("`abbbb`", "abbbb")]
        [InlineData("`abc123`", "abc123")]
        [InlineData("`abc123!@#$`", "abc123!@#$")]
        [InlineData("`abc\ndef`", "abc\ndef")]
        public void ShouldParseQuotedIdentifierCompiled(string text, string expected)
        {
            Assert.True(TermIdentifierCompiledParser.TryParse(text, out var result));
            Assert.Equal(result, expected);
        }

        private void ErrorWhenMissingClosingSymbol(Parser<String> parser)
        {
            Assert.False(parser.TryParse("[a", out var _, out var error));
            Assert.Equal("Unclosed quotation mark ']'.", error.Message);
            Assert.Equal(new TextPosition(2, 1, 3), error.Position);

            Assert.False(parser.TryParse("[abbbb", out _, out error));
            Assert.Equal("Unclosed quotation mark ']'.", error.Message);
            Assert.Equal(new TextPosition(6, 1, 7), error.Position);

            Assert.False(parser.TryParse("`a", out _, out error));
            Assert.Equal("Unclosed quotation mark '`'.", error.Message);
            Assert.Equal(new TextPosition(2, 1, 3), error.Position);

            Assert.False(parser.TryParse("`abbbb", out _, out error));
            Assert.Equal("Unclosed quotation mark '`'.", error.Message);
            Assert.Equal(new TextPosition(6, 1, 7), error.Position);
        }

        [Fact]
        public void ErrorWhenMissingClosingSymbolNonCompiled()
        {
            ErrorWhenMissingClosingSymbol(TermIdentifierParser);
        }

        [Fact]
        public void ErrorWhenMissingClosingSymbolCompiled()
        {
            ErrorWhenMissingClosingSymbol(TermIdentifierCompiledParser);
        }

        private void ShouldNotParseWhenNotStartWithOpeningSymbol(Parser<String> parser)
        {
            Assert.False(TermIdentifierParser.TryParse("abc", out var result, out var error));
            Assert.Null(result);
            Assert.Null(error);
        }

        [Fact]
        public void ShouldNotParseWhenNotStartWithOpeningSymbolNonCompiled()
        {
            ShouldNotParseWhenNotStartWithOpeningSymbol(TermIdentifierParser);
        }

        [Fact]
        public void ShouldNotParseWhenNotStartWithOpeningSymbolCompiled()
        {
            ShouldNotParseWhenNotStartWithOpeningSymbol(TermIdentifierCompiledParser);
        }

        private void ShouldParseIdentifierWithEscapedClosingSymbol(Parser<String> parser)
        {
            Assert.True(TermIdentifierParser.TryParse("[abc]]def]", out var result, out var error));
            Assert.Equal("abc]def", result);
            Assert.Null(error);

            Assert.True(TermIdentifierParser.TryParse("[[abc]]]]def]", out result, out error));
            Assert.Equal("[abc]]def", result);
            Assert.Null(error);

            Assert.True(TermIdentifierParser.TryParse("`abc``def`", out result, out error));
            Assert.Equal("abc`def", result);
            Assert.Null(error);

            Assert.True(TermIdentifierParser.TryParse("```abc````def```", out result, out error));
            Assert.Equal("`abc``def`", result);
            Assert.Null(error);
        }

        [Fact]
        public void ShouldParseIdentifierWithEscapedClosingSymbolNonCompiled()
        {
            ShouldParseIdentifierWithEscapedClosingSymbol(TermIdentifierParser);
        }

        [Fact]
        public void ShouldParseIdentifierWithEscapedClosingSymbolCompiled()
        {
            ShouldParseIdentifierWithEscapedClosingSymbol(TermIdentifierCompiledParser);
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("``")]
        [InlineData("''")]
        [InlineData("\"\"")]
        public void ShouldFailOnInvalidIdentifiers(string text)
        {
            var expectedOffset = 2;
            Assert.False(TermIdentifierParser.TryParse(text, out var result, out var error));
            Assert.Null(result);
            Assert.Equal("Invalid zero length identifier.", error.Message);
            Assert.Equal(error.Position.Offset, expectedOffset);
        }

        [Theory]
        [InlineData("[]")]
        [InlineData("``")]
        [InlineData("''")]
        [InlineData("\"\"")]
        public void ShouldFailOnInvalidIdentifiersCompiled(string text)
        {
            var expectedOffset = 2;
            Assert.False(TermIdentifierCompiledParser.TryParse(text, out var result, out var error));
            Assert.Null(result);
            Assert.Equal("Invalid zero length identifier.", error.Message);
            Assert.Equal(error.Position.Offset, expectedOffset);
        }

        [Theory]
        [InlineData("[1][2][3][4]", new[] { "1", "2", "3", "4" })]
        [InlineData("'1'\"2\"`3`[4]", new[] { "1", "2", "3", "4" })]
        public void ShouldParseQuotedIdentifiersLiterals(string input, string[] expected)
        {
            OneOrMany(LiteralIdentifierCompiledParser).TryParse(input, out var identifiers);
            Assert.Equal(identifiers.ToArray(), expected);
        }
    }
}

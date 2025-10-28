using Lampersky.Fluent;
using Parlot;
using Parlot.Fluent;
using Xunit;

namespace Lampersky.UsefulParsers.Tests
{
    public class ExtendedWhiteSpaceParserTests
    {
        private Parser<String> WhiteSpaceWithCommentsParser;
        private Parser<String> WhiteSpaceWithCommentsParserNoNesting;
        public ExtendedWhiteSpaceParserTests()
        {
            WhiteSpaceWithCommentsParser = Literals.ExtendedWhiteSpace(
                s => s.SkipWhiteSpaceOrNewLine(),
                s => s.SkipSingleLineComment("--"),
                s => s.SkipMultiLineComment("/*", "*/", true)
            ).Then(static x => x.ToString());

            WhiteSpaceWithCommentsParserNoNesting = Literals.ExtendedWhiteSpace(
                s => s.SkipWhiteSpaceOrNewLine(),
                s => s.SkipSingleLineComment("--"),
                s => s.SkipMultiLineComment("/*", "*/", false)
            ).Then(static x => x.ToString());
        }

        [Theory]
        [InlineData("   ")]
        [InlineData("\n\n   ")]
        [InlineData("-- single line comment")]
        [InlineData("/* multi\nline\ncomment */")]
        [InlineData("  \n  \n  /* multi\nline\ncomment */ -- single line\n  \n \n /**/")]
        [InlineData("/* first \n comment /* nested\ncomment */ */")]
        public void ShouldReadWhiteSpacesOrComments(string text)
        {
            var ctx = new ParseContext(new Scanner(text));
            Assert.True(WhiteSpaceWithCommentsParser.TryParse(ctx, out var result, out var error));
            Assert.Null(error);
            Assert.Equal(ctx.Scanner.Cursor.Offset, text.Length);
            Assert.Equal(text, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123456")]
        [InlineData("asdfgh")]
        [InlineData("!@#$%")]
        public void ShouldNotReadWhenNoWhiteSpaceNorComment(string text)
        {
            var ctx = new ParseContext(new Scanner(text));
            Assert.False(WhiteSpaceWithCommentsParser.TryParse(ctx, out var _, out var error));
            Assert.Null(error);
            Assert.True(ctx.Scanner.Cursor.Offset == 0);
        }

        [Theory]
        [InlineData("/* abc")]
        [InlineData("/* first multi line comment\n /* nested */")]
        [InlineData("   -- single line comment \n /* abc \n\n\n")]
        [InlineData("/*/**/")]
        public void ErrorWhenMultiLineCommentNotProperlyClosed(string text)
        {
            var ctx = new ParseContext(new Scanner(text));
            Assert.False(WhiteSpaceWithCommentsParser.TryParse(ctx, out var _, out var error));
            Assert.NotNull(error);
            Assert.Equal("Missing end comment mark '*/'.", error.Message);
            Assert.Equal(ctx.Scanner.Cursor.Position, error.Position);
            Assert.Equal(ctx.Scanner.Cursor.Offset, text.Length);
        }

        [Theory]
        [InlineData("/* abc /* nested */*/", 7)]
        [InlineData("/*/**/*/", 2)]
        [InlineData("  /*", 4)]
        [InlineData("/*\n\n", 4)]
        public void ErrorWhenNestedMultiLineCommentIsNotAllowed(string text, int expectedClosingSymbolOffset)
        {
            var ctx = new ParseContext(new Scanner(text));
            Assert.False(WhiteSpaceWithCommentsParserNoNesting.TryParse(ctx, out var _, out var error));
            Assert.NotNull(error);
            Assert.Equal("Missing end comment mark '*/'.", error.Message);
            Assert.Equal(ctx.Scanner.Cursor.Position, error.Position);
            Assert.Equal(error.Position.Offset, expectedClosingSymbolOffset);
        }
    }
}

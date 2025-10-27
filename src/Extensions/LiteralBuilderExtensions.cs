using Parlot.FLuent;
using Parlot.UsefulParsers;
using System.Text.RegularExpressions;

namespace Parlot.Fluent
{
    public static class LiteralBuilderExtensions
    {
        public static Parser<TextSpan> CustomIdentifier(this LiteralBuilder literalBuilder, Func<char, bool> isStart, Func<char, bool> isPart) => new CustomIdentifierParser(isStart, isPart);
        public static Parser<TextSpan> SquareBracketsIdentifier(this LiteralBuilder literalBuilder) => new QuotedIdentifierParser(IdentifierQuotes.SquareBracket);
        public static Parser<TextSpan> SingleQuotedIdentifier(this LiteralBuilder literalBuilder) => new QuotedIdentifierParser(IdentifierQuotes.Single);
        public static Parser<TextSpan> DoubleQuotedIdentifier(this LiteralBuilder literalBuilder) => new QuotedIdentifierParser(IdentifierQuotes.Double);
        public static Parser<TextSpan> SingleOrDoubleQuotedIdentifier(this LiteralBuilder literalBuilder) => new QuotedIdentifierParser(Quotes.SingleQuotationMarks, Quotes.DoubleQuotationMarks);
        public static Parser<TextSpan> QuotedIdentifier(this LiteralBuilder literalBuilder, params (char, char)[] startEndSymbols) => new QuotedIdentifierParser(startEndSymbols);
        public static Parser<TextSpan> ExtendedWhiteSpace(this LiteralBuilder literalBuilder, params Func<Scanner, bool>[] predicates) => new ExtendedWhiteSpaceParser(predicates);
        public static Parser<TextSpan> Regex(this LiteralBuilder literalBuilder, string pattern) => new RegexParser(pattern);
        public static Parser<TextSpan> Regex(this LiteralBuilder literalBuilder, string pattern, RegexOptions options) => new RegexParser(pattern, options);
        public static Parser<T> Debug<T>(this LiteralBuilder literalBuilder, string name, Parser<T> parser) => new DebugParser<T>(name, parser);

        /// <summary>
        /// Builds a parser that matches non quoted valid SQL identifier. [a-zA-Z][a-zA-Z0-9_]*
        /// </summary>
        public static Parser<TextSpan> SimpleSqlIdentifier(this LiteralBuilder literalBuilder) =>
            literalBuilder.CustomIdentifier(SimpleSqlIdentifierCharacters.IsSimpleSqlIdentifierStart, SimpleSqlIdentifierCharacters.IsSimpleSqlIdentifierPart);

        /// <summary>
        /// Builds a parser that matches quoted valid SQL identifier. SingleQuote, DoubleQuote, BackTick or SquareBracketQuote.
        /// </summary>
        public static Parser<TextSpan> QuotedSqlIdentifier(this LiteralBuilder literalBuilder) =>
            literalBuilder.QuotedIdentifier(Quotes.SingleQuotationMarks, Quotes.DoubleQuotationMarks, Quotes.SquareBrackets, Quotes.BackTicks);

        /// <summary>
        /// Builds a parser that matches valid SQL identifier.
        /// </summary>
        public static Parser<TextSpan> SqlIdentifier(this LiteralBuilder literalBuilder) => literalBuilder.QuotedSqlIdentifier().Or(literalBuilder.SimpleSqlIdentifier());

        /// <summary>
        /// Builds a parser that skips WhiteSpace, NewLine, Single Line Comment and Multi Line Comment.
        /// </summary>
        public static Parser<TextSpan> SqlCommentsAndWhiteSpaces(this LiteralBuilder literalBuilder) => literalBuilder.ExtendedWhiteSpace(
            s => s.SkipWhiteSpaceOrNewLine(),
            s => s.SkipSingleLineComment("--"),
            s => s.SkipMultiLineComment("/*", "*/", true));
    }
}

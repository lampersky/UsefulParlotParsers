using Parlot.FLuent;
using Parlot.UsefulParsers;
using System.Text.RegularExpressions;

namespace Parlot.Fluent
{
    public static class TermBuilderExtensions
    {
        public static Parser<TextSpan> CustomIdentifier(this TermBuilder termBuilder, Func<char, bool> isStart, Func<char, bool> isPart) => Parsers.SkipWhiteSpace(new CustomIdentifierParser(isStart, isPart));
        public static Parser<TextSpan> SquareBracketsIdentifier(this TermBuilder termBuilder) => Parsers.SkipWhiteSpace(new QuotedIdentifierParser(IdentifierQuotes.SquareBracket));
        public static Parser<TextSpan> SingleQuotedIdentifier(this TermBuilder termBuilder) => Parsers.SkipWhiteSpace(new QuotedIdentifierParser(IdentifierQuotes.Single));
        public static Parser<TextSpan> DoubleQuotedIdentifier(this TermBuilder termBuilder) => Parsers.SkipWhiteSpace(new QuotedIdentifierParser(IdentifierQuotes.Double));
        public static Parser<TextSpan> SingleOrDoubleQuotedIdentifier(this TermBuilder termBuilder) => Parsers.SkipWhiteSpace(new QuotedIdentifierParser(Quotes.SingleQuotationMarks, Quotes.DoubleQuotationMarks));
        public static Parser<TextSpan> QuotedIdentifier(this TermBuilder termBuilder, params (char, char)[] startEndSymbols) => Parsers.SkipWhiteSpace(new QuotedIdentifierParser(startEndSymbols));
        public static Parser<TextSpan> Regex(this TermBuilder termBuilder, string pattern) => Parsers.SkipWhiteSpace(new RegexParser(pattern));
        public static Parser<TextSpan> Regex(this TermBuilder termBuilder, string pattern, RegexOptions options) => Parsers.SkipWhiteSpace(new RegexParser(pattern, options));
        public static Parser<T> Debug<T>(this TermBuilder termBuilder, string name, Parser<T> parser) => new DebugParser<T>(name, Parsers.SkipWhiteSpace(parser));

        /// <summary>
        /// Builds a parser that matches non quoted valid SQL identifier. [a-zA-Z][a-zA-Z0-9_]*
        /// </summary>
        public static Parser<TextSpan> SimpleSqlIdentifier(this TermBuilder termBuilder) => termBuilder.CustomIdentifier(SimpleSqlIdentifierCharacters.IsSimpleSqlIdentifierStart, SimpleSqlIdentifierCharacters.IsSimpleSqlIdentifierPart);

        /// <summary>
        /// Builds a parser that matches non quoted valid SQL identifier with non keyword check. [a-zA-Z][a-zA-Z0-9_]*
        /// </summary>
        public static Parser<TextSpan> SimpleSqlIdentifier(this TermBuilder termBuilder, HashSet<TextSpan> keywords) => termBuilder.SimpleSqlIdentifier().When(x => !keywords.Contains(x.ToString().ToLowerInvariant()));


        /// <summary>
        /// Builds a parser that matches quoted valid SQL identifier. SingleQuote, DoubleQuote or SquareBracketQuote.
        /// </summary>
        public static Parser<TextSpan> QuotedSqlIdentifier(this TermBuilder termBuilder) => termBuilder.QuotedIdentifier(Quotes.DoubleQuotationMarks, Quotes.SquareBrackets, Quotes.BackTicks);

        /// <summary>
        /// Builds a parser that matches valid SQL identifier. It can be quoted or simple.
        /// </summary>
        public static Parser<TextSpan> SqlIdentifier(this TermBuilder termBuilder) => termBuilder.QuotedSqlIdentifier().Or(termBuilder.SimpleSqlIdentifier());

        /// <summary>
        /// Builds a parser that matches valid SQL identifier. It can be quoted or simple. For non-quoted identifiers additional non-keyword check is performed.
        /// </summary>
        public static Parser<TextSpan> SqlIdentifier(this TermBuilder termBuilder, HashSet<TextSpan> keywords) =>
            termBuilder.QuotedSqlIdentifier().Or(termBuilder.SimpleSqlIdentifier(keywords));
    }
}

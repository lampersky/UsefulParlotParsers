using Parlot.Compilation;
using Parlot.Fluent;
using Parlot.Rewriting;
using Parlot.Sql.Extensions;
using System.Linq.Expressions;

namespace Parlot.UsefulParsers
{
    public enum IdentifierQuotes
    {
        Single,
        Double,
        SquareBracket,
        UsePredicate
    }

    public static class Quotes
    {
        public static (char, char) SingleQuotationMarks = ('\'', '\'');
        public static (char, char) DoubleQuotationMarks = ('\"', '\"');
        public static (char, char) SquareBrackets = ('[', ']');
        public static (char, char) CurlyBrackets = ('{', '}');
        public static (char, char) Parentheses = ('(', ')');
        public static (char, char) BackTicks = ('`', '`');
        public static (char, char) AngleBrackets = ('<', '>');
    }

    /// <summary>
    /// This parser matches quoted identifiers.
    /// Quoted identifier, must be enclosed within one of the available pairs of quoting characters.
    /// To include the closing quoting character within the identifier itself, 
    /// it must be escaped by doubling the closing quoting character.
    /// Opening quoting character do not need escaping as long as it is not the same as closing quoting character.
    /// </summary>
    public sealed class QuotedIdentifierParser : Parser<TextSpan>, ICompilable, ISeekable
    {
        public bool CanSeek => true;

        public char[] ExpectedChars => _quotes switch { 
            IdentifierQuotes.Single => new[] { '\'' },
            IdentifierQuotes.Double => new[] { '\"' },
            IdentifierQuotes.SquareBracket => new[] { '[' },
            IdentifierQuotes.UsePredicate => _expectedChars, 
            _ => Array.Empty<char>()
        };

        public bool SkipWhitespace => true;

        private readonly IdentifierQuotes _quotes;
        private char[] _expectedChars;
        private readonly Func<char, bool> _isStart;
        private readonly Func<char, char> _getEndChar;

        public QuotedIdentifierParser(IdentifierQuotes quotes) : base()
        {
            _quotes = quotes;
            if (quotes == IdentifierQuotes.UsePredicate)
            {
                throw new ArgumentException("UsePredicate can't be used directly!");
            }
            _isStart = (char c) => false;
            _getEndChar = (char c) => c;
            _expectedChars = [];
        }

        public QuotedIdentifierParser(params (char startSymbol, char endSymbol)[] startEndSymbols) : base()
        {
            if (startEndSymbols.Length == 0)
            {
                throw new ArgumentException("At least one start/end symbol pair must be provided!");
            }
            _quotes = IdentifierQuotes.UsePredicate;
            _isStart = ExpressionExtensions.CreateIsStartCharPredicate(startEndSymbols);
            _getEndChar = ExpressionExtensions.CreateGetEndCharPredicate(startEndSymbols);
            _expectedChars = startEndSymbols.Select(item => item.startSymbol).ToArray();
        }

        public override bool Parse(ParseContext context, ref ParseResult<TextSpan> result)
        {
            context.EnterParser(this);

            var start = context.Scanner.Cursor.Offset;

            var success = _quotes switch
            {
                IdentifierQuotes.Single => context.Scanner.ReadSingleQuotedIdentifier(),
                IdentifierQuotes.Double => context.Scanner.ReadDoubleQuotedIdentifier(),
                IdentifierQuotes.SquareBracket => context.Scanner.ReadSquareBracketsQuotedIdentifier(),
                IdentifierQuotes.UsePredicate => context.Scanner.ReadQuotedIdentifier(_isStart, _getEndChar),
                _ => false
            };

            var end = context.Scanner.Cursor.Offset;

            if (success)
            {
                var lastChar = context.Scanner.GetCharFromIndex(end - 1);
                var unescaped = QuotedIdentifierCharacters.Unescape(new TextSpan(context.Scanner.Buffer, start + 1, end - start - 2), lastChar);

                result.Set(start, end, unescaped);
                return true;
            }
            else
            {
                return false;
            }
        }

        public CompilationResult Compile(CompilationContext context)
        {
            var result = new CompilationResult();

            var success = context.DeclareSuccessVariable(result, false);
            var value = context.DeclareValueVariable(result, Expression.Default(typeof(TextSpan)));

            // var start = context.Scanner.Cursor.Offset;

            var start = Expression.Variable(typeof(int), $"start{context.NextNumber}");
            result.Variables.Add(start);

            result.Body.Add(Expression.Assign(start, context.Offset()));

            var parseStringExpression = _quotes switch
            {
                IdentifierQuotes.Single => context.ReadSingleQuotedIdentifier(),
                IdentifierQuotes.Double => context.ReadDoubleQuotedIdentifier(),
                IdentifierQuotes.SquareBracket => context.ReadSquareBracketsQuotedIdentifier(),
                IdentifierQuotes.UsePredicate => context.ReadQuotedIdentifier(_isStart, _getEndChar),
                _ => throw new InvalidOperationException()
            };

            // if (context.Scanner.ReadQuotedIdentifier())
            // {
            //     var end = context.Scanner.Cursor.Offset;
            //     success = true;
            //     value = QuotedIdentifierCharacters.Unescape(new TextSpan(context.Scanner.Buffer, start, end - start), char);
            // }

            var end = Expression.Variable(typeof(int), $"end{context.NextNumber}");

            var unescapeMethodInfo = typeof(QuotedIdentifierCharacters).GetMethod("Unescape", new[] { typeof(TextSpan), typeof(char) });

            //context.Unescape(
            //    context.NewTextSpan(
            //        context.Buffer(),
            //        Expression.Add(start, Expression.Constant(1)),
            //        Expression.Subtract(Expression.Subtract(end, start), Expression.Constant(2))
            //        ),
            //    context.GetCharFromIndex(Expression.Subtract(end, Expression.Constant(1))));

            result.Body.Add(
                Expression.IfThen(
                    parseStringExpression,
                    Expression.Block(
                        new[] { end },
                        Expression.Assign(end, context.Offset()),
                        Expression.Assign(success, Expression.Constant(true, typeof(bool))),
                        context.DiscardResult
                        ? Expression.Empty()
                        : Expression.Assign(value,
                            Expression.Call(unescapeMethodInfo,
                                context.NewTextSpan(
                                    context.Buffer(),
                                    Expression.Add(start, Expression.Constant(1)),
                                    Expression.Subtract(Expression.Subtract(end, start), Expression.Constant(2))
                                    ),
                                context.GetCharFromIndex(Expression.Subtract(end, Expression.Constant(1)))))
                    )
                ));

            return result;
        }
    }
}

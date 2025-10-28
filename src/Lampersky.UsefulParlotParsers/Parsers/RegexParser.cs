using Lampersky.Compilation;
using Parlot;
using Parlot.Compilation;
using Parlot.Fluent;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Lampersky.UsefulParsers
{
    public sealed class RegexParser : Parser<TextSpan>, ICompilable
    {
        private Regex Regex;

        public RegexParser(string pattern)
        {
            Regex = new Regex(pattern);
        }

        public RegexParser(string pattern, RegexOptions options)
        {
            Regex = new Regex(pattern, options);
        }

        public override bool Parse(ParseContext context, ref ParseResult<TextSpan> result)
        {
            context.EnterParser(this);

            var cursor = context.Scanner.Cursor;

            var match = Regex.Match(cursor.Buffer);

            if (match.Success)
            {
                var start = cursor.Offset + match.Index;

                cursor.Advance(match.Index + match.Length);

                var end = context.Scanner.Cursor.Offset;

                result.Set(start, end, new TextSpan(context.Scanner.Buffer, start, end - start));
                return true;
            }

            return false;
        }

        public CompilationResult Compile(CompilationContext context)
        {
            var result = context.CreateCompilationResult<TextSpan>();

            // var start = context.Scanner.Cursor.Offset;

            var start = Expression.Variable(typeof(int), $"start{context.NextNumber}");
            result.Variables.Add(start);

            // var match = context.RegexMatch(Regex);

            var match = Expression.Variable(typeof(Match), $"match{context.NextNumber}");
            result.Variables.Add(match);

            // if (context.IsRegexMatchSuccess(match))
            // {
            //     var start = cursor.Offset + match.Index;
            //     cursor.Advance(match.Index + match.Length)
            //     var end = context.Scanner.Cursor.Offset;
            //     success = true;
            //     value = new TextSpan(context.Scanner.Buffer, start, end - start), char);
            // }

            var end = Expression.Variable(typeof(int), $"end{context.NextNumber}");

            result.Body.Add(
                Expression.Block(
                    new[] { match },
                    Expression.Assign(match, context.RegexMatch(Regex)),
                    Expression.IfThen(
                        context.IsRegexMatchSuccess(match),
                        Expression.Block(
                            new[] { start, end },
                            Expression.Assign(start, Expression.Add(context.Offset(), context.GetRegexMatchIndex(match))),
                            context.Advance(Expression.Add(context.GetRegexMatchIndex(match), context.GetRegexMatchLength(match))),
                            Expression.Assign(end, context.Offset()),
                            Expression.Assign(result.Success, Expression.Constant(true, typeof(bool))),
                            context.DiscardResult
                            ? Expression.Empty()
                            : Expression.Assign(result.Value,
                                context.NewTextSpan(
                                    context.Buffer(),
                                    start,
                                    Expression.Subtract(end, start)
                                    )
                                )
                        )
                    )
                )
            );

            return result;
        }
    }
}

using Parlot.Compilation;
using Parlot.Fluent;

namespace Parlot.UsefulParsers
{
    public sealed class DebugParser<T> : Parser<T>/*, ICompilable*/
    {
        private readonly Parser<T> _parser;
        private readonly string _name;

        public DebugParser(string name, Parser<T> parser)
        {
            _name = name;
            _parser = parser;
        }

        public override bool Parse(ParseContext context, ref ParseResult<T> result)
        {
            context.EnterParser(this);

            System.Diagnostics.Debug.WriteLine($"Entered parser: {_name}");

            var start = context.Scanner.Cursor.Position;

            //ParseResult<T> _ = new();

            // Did parser succeed.
            try
            {
                if (_parser.Parse(context, ref result))
                {
                    var end = context.Scanner.Cursor.Offset;
                    var length = end - start.Offset;

                    //result.Set(start.Offset, end, new TextSpan(context.Scanner.Buffer, start.Offset, length));
                    var value = new TextSpan(context.Scanner.Buffer, start.Offset, length);

                    System.Diagnostics.Debug.WriteLine($"Parser succeed: {_name} with '{value}'");

                    return true;
                }
            }
            catch (ParseException pe)
            {
                System.Diagnostics.Debug.WriteLine($"Parser throws: {_name} with '{pe.Message}' at '{pe.Position}'");

                throw;
            }


            var rest = new TextSpan(context.Scanner.Buffer, start.Offset, context.Scanner.Buffer.Length - start.Offset);

            System.Diagnostics.Debug.WriteLine($"Parser failed: {_name} with '{rest}'");

            context.Scanner.Cursor.ResetPosition(start);

            return false;
        }

        //public CompilationResult Compile(CompilationContext context)
        //{
        //    var result = new CompilationResult();

        //    var success = context.DeclareSuccessVariable(result, false);
        //    var value = context.DeclareValueVariable(result, Expression.Default(typeof(TextSpan)));

        //    // var start = context.Scanner.Cursor.Offset;

        //    var start = Expression.Variable(typeof(int), $"start{context.NextNumber}");
        //    result.Variables.Add(start);

        //    // var match = context.RegexMatch(Regex);

        //    var match = Expression.Variable(typeof(Match), $"match{context.NextNumber}");
        //    result.Variables.Add(match);

        //    // if (context.IsRegexMatchSuccess(match))
        //    // {
        //    //     var start = cursor.Offset + match.Index;
        //    //     cursor.Advance(match.Index + match.Length)
        //    //     var end = context.Scanner.Cursor.Offset;
        //    //     success = true;
        //    //     value = new TextSpan(context.Scanner.Buffer, start, end - start), char);
        //    // }

        //    var end = Expression.Variable(typeof(int), $"end{context.NextNumber}");

        //    result.Body.Add(
        //        Expression.Block(
        //            new[] { match },
        //            Expression.Assign(match, context.RegexMatch(Regex)),
        //            Expression.IfThen(
        //                context.IsRegexMatchSuccess(match),
        //                Expression.Block(
        //                    new[] { start, end },
        //                    Expression.Assign(start, Expression.Add(context.Offset(), context.GetRegexMatchIndex(match))),
        //                    context.Advance(Expression.Add(context.GetRegexMatchIndex(match), context.GetRegexMatchLength(match))),
        //                    Expression.Assign(end, context.Offset()),
        //                    Expression.Assign(success, Expression.Constant(true, typeof(bool))),
        //                    context.DiscardResult
        //                    ? Expression.Empty()
        //                    : Expression.Assign(value,
        //                        context.NewTextSpan(
        //                            context.Buffer(),
        //                            start,
        //                            Expression.Subtract(end, start)
        //                            )
        //                        )
        //                )
        //            )
        //        )
        //    );

        //    return result;
        //}
    }
}

using Lampersky.Compilation;
using Parlot;
using Parlot.Compilation;
using Parlot.Fluent;
using System.Linq.Expressions;

namespace Lampersky.UsefulParsers
{
    /// <summary>
    /// Scans the input buffer until non of the provided predicate conditions is met.
    /// 
    /// This parser can by used to build more complicated White Space and Comment parsers.
    /// </summary>
    public sealed class ExtendedWhiteSpaceParser : Parser<TextSpan>, ICompilable
    {
        private readonly Func<Scanner, bool>[] _predicates;
        public ExtendedWhiteSpaceParser(params Func<Scanner, bool>[] predicates)
        {
            _predicates = predicates;
        }

        public override bool Parse(ParseContext context, ref ParseResult<TextSpan> result)
        {
            var start = context.Scanner.Cursor.Offset;

            var success = context.Scanner.Until(_predicates);

            var end = context.Scanner.Cursor.Offset;

            //TODO: maybe good to add "&& start != end"
            if (success)
            {
                result.Set(start, end, new TextSpan(context.Scanner.Buffer, start, end - start));

                return true;
            }
            else
            {
                return false;
            }
        }

        public CompilationResult Compile(CompilationContext context)
        {
            var result = context.CreateCompilationResult<TextSpan>();

            // var start = context.Scanner.Cursor.Offset;

            var start = Expression.Variable(typeof(int), $"start{context.NextNumber}");
            result.Variables.Add(start);

            result.Body.Add(Expression.Assign(start, context.Offset()));

            var until = context.Until(_predicates);

            // if (context.Scanner.Until())
            // {
            //     var end = context.Scanner.Cursor.Offset;
            //     success = true;
            //     value = new TextSpan(context.Scanner.Buffer, start, end - start - 1);
            // }

            var end = Expression.Variable(typeof(int), $"end{context.NextNumber}");

            result.Body.Add(
                Expression.IfThen(
                    until,
                    Expression.Block(
                        new[] { end },
                        Expression.Assign(end, context.Offset()),
                        Expression.Assign(result.Success, Expression.Constant(true, typeof(bool))),
                        context.DiscardResult
                        ? Expression.Empty()
                        : Expression.Assign(result.Value,
                            context.NewTextSpan(
                                context.Buffer(),
                                start,
                                Expression.Subtract(end, start)
                                ))
                    )
                ));

            return result;
        }
    }
}

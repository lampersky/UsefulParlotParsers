using Parlot.Compilation;
using Parlot.Fluent;
using Parlot.Rewriting;
using System.Linq.Expressions;

namespace Parlot.UsefulParsers
{
    public sealed class CustomIdentifierParser : Parser<TextSpan>, ICompilable, ISeekable
    {
        private readonly Func<char, bool> _extraStart;
        private readonly Func<char, bool> _extraPart;
        private readonly char[] _expectedChars;

        public bool CanSeek => true;

        public char[] ExpectedChars => _expectedChars;

        public bool SkipWhitespace => true;

        public CustomIdentifierParser(Func<char, bool> isStart, Func<char, bool> isPart)
        {
            _extraStart = isStart;
            _extraPart = isPart;
            _expectedChars = CustomIdentifierCharacters.GetCustomIdentifierExpectedCharacters(_extraStart);
        }

        public override bool Parse(ParseContext context, ref ParseResult<TextSpan> result)
        {
            context.EnterParser(this);

            var start = context.Scanner.Cursor.Offset;

            var success = context.Scanner.ReadFirstThenOthers(_extraStart, _extraPart);

            var end = context.Scanner.Cursor.Offset;

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
            var result = new CompilationResult();

            var success = context.DeclareSuccessVariable(result, false);
            var value = context.DeclareValueVariable(result, Expression.Default(typeof(TextSpan)));

            // var start = context.Scanner.Cursor.Offset;

            var start = Expression.Variable(typeof(int), $"start{context.NextNumber}");
            result.Variables.Add(start);

            result.Body.Add(Expression.Assign(start, context.Offset()));

            var readCustomIdentifier = context.ReadFirstThenOthers(_extraStart, _extraPart);

            // if (context.Scanner.ReadFirstThenOthers(_extraStart, _extraPart))
            // {
            //     var end = context.Scanner.Cursor.Offset;
            //     success = true;
            //     value = new TextSpan(context.Scanner.Buffer, start, end - start);
            // }

            var end = Expression.Variable(typeof(int), $"end{context.NextNumber}");

            result.Body.Add(
                Expression.IfThen(
                    readCustomIdentifier,
                    Expression.Block(
                        new[] { end },
                        Expression.Assign(end, context.Offset()),
                        Expression.Assign(success, Expression.Constant(true, typeof(bool))),
                        context.DiscardResult
                        ? Expression.Empty()
                        : Expression.Assign(value,
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

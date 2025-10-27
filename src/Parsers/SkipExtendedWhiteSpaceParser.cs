namespace Parlot.UsefulParsers.Parsers;

using Parlot.Compilation;
using Parlot.Fluent;
using Parlot.Rewriting;
using System;
using System.Linq.Expressions;

public sealed class SkipExtendedWhiteSpaceParser<T> : Parser<T>, ICompilable, ISeekable
{
    private readonly Parser<T> _parser;

    private readonly Func<Scanner, bool>[] _predicates;

    public SkipExtendedWhiteSpaceParser(Parser<T> parser, params Func<Scanner, bool>[] predicates)
    {
        _parser = parser;
        _predicates = predicates;
    }

    public bool CanSeek => _parser is ISeekable seekable && seekable.CanSeek;

    public char[] ExpectedChars => _parser is ISeekable seekable ? seekable.ExpectedChars : default;

    public bool SkipWhitespace => true;

    public override bool Parse(ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        var start = context.Scanner.Cursor.Position;

        // Use the scanner's logic to ignore whitespaces since it knows about multi-line grammars
        context.Scanner.Until(_predicates);

        if (_parser.Parse(context, ref result))
        {
            return true;
        }

        context.Scanner.Cursor.ResetPosition(start);

        return false;
    }

    public CompilationResult Compile(CompilationContext context)
    {
        var result = new CompilationResult();

        var success = context.DeclareSuccessVariable(result, false);
        var value = context.DeclareValueVariable(result, Expression.Default(typeof(T)));

        var start = context.DeclarePositionVariable(result);

        var parserCompileResult = _parser.Build(context);

        result.Body.Add(
            Expression.Block(
                parserCompileResult.Variables,
                context.Until(_predicates),
                Expression.Block(parserCompileResult.Body),
                Expression.IfThenElse(
                    parserCompileResult.Success,
                    Expression.Block(
                        context.DiscardResult ? Expression.Empty() : Expression.Assign(value, parserCompileResult.Value),
                        Expression.Assign(success, Expression.Constant(true, typeof(bool)))
                        ),
                    context.ResetPosition(start)
                    )
                )
            );

        return result;
    }
}

using Parlot.UsefulParsers;
using System.Linq.Expressions;
using System.Reflection;

namespace Parlot.Compilation
{
    public static class CommentsExpressionHelper
    {
        internal static MethodInfo Scanner_Until = typeof(CommentsScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.CommentsScannerExtensions.Until), new Type[2] { typeof(Scanner), typeof(Func<Scanner, bool>[]) });

        public static MethodCallExpression Until(this CompilationContext context, Func<Scanner, bool>[] predicates) => Expression.Call(null, Scanner_Until, context.Scanner(), Expression.Constant(predicates));
    }
}

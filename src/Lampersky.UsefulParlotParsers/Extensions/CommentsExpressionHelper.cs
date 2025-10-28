using Lampersky.UsefulParsers;
using Parlot;
using Parlot.Compilation;
using System.Linq.Expressions;
using System.Reflection;

namespace Lampersky.Compilation
{
    public static class CommentsExpressionHelper
    {
        internal static MethodInfo Scanner_Until = typeof(CommentsScannerExtensions).GetMethod(nameof(CommentsScannerExtensions.Until), new Type[2] { typeof(Scanner), typeof(Func<Scanner, bool>[]) });

        public static MethodCallExpression Until(this CompilationContext context, Func<Scanner, bool>[] predicates) => Expression.Call(null, Scanner_Until, context.Scanner(), Expression.Constant(predicates));
    }
}

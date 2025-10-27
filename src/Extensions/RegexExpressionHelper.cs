using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Parlot.Compilation
{
    public static class RegexExpressionHelper
    {
        internal static MethodInfo Cursor_Advance = typeof(Cursor).GetMethod(nameof(Parlot.Cursor.Advance), new Type[] { typeof(int) });
        internal static MethodInfo Regex_Match = typeof(Regex).GetMethod(nameof(Regex.Match), new Type[1] { typeof(string) });

        public static MethodCallExpression RegexMatch(this CompilationContext context, Regex regex) => Expression.Call(Expression.Constant(regex), Regex_Match, context.Buffer());
        public static MemberExpression IsRegexMatchSuccess(this CompilationContext _, Expression regexMatch) => Expression.Property(regexMatch, "Success");
        public static MemberExpression GetRegexMatchIndex(this CompilationContext _, Expression regexMatch) => Expression.Property(regexMatch, "Index");
        public static MemberExpression GetRegexMatchLength(this CompilationContext _, Expression regexMatch) => Expression.Property(regexMatch, "Length");
        public static MethodCallExpression Advance(this CompilationContext context, Expression count) => Expression.Call(context.Cursor(), Cursor_Advance, new[] { count });
    }
}

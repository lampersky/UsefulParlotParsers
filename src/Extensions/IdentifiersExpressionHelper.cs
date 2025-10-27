using Parlot.UsefulParsers;
using System.Linq.Expressions;
using System.Reflection;

namespace Parlot.Compilation
{
    public static class IdentifiersExpressionHelper
    {
        internal static MethodInfo Scanner_ReadSingleQuotedIdentifier = typeof(IdentifiersScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.IdentifiersScannerExtensions.ReadSingleQuotedIdentifier), new Type[1] { typeof(Scanner) });
        internal static MethodInfo Scanner_ReadDoubleQuotedIdentifier = typeof(IdentifiersScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.IdentifiersScannerExtensions.ReadDoubleQuotedIdentifier), new Type[1] { typeof(Scanner) });
        internal static MethodInfo Scanner_ReadSquareBracketsQuotedIdentifier = typeof(IdentifiersScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.IdentifiersScannerExtensions.ReadSquareBracketsQuotedIdentifier), new Type[1] { typeof(Scanner) });
        internal static MethodInfo Scanner_ReadQuotedIdentifierWithPredicate = typeof(IdentifiersScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.IdentifiersScannerExtensions.ReadQuotedIdentifier), new Type[3] { typeof(Scanner), typeof(Func<char, bool>), typeof(Func<char, char>) });
        internal static MethodInfo Scanner_ReadFirstThenOthers = typeof(Scanner).GetMethod(nameof(Parlot.Scanner.ReadFirstThenOthers), new Type[2] { typeof(Func<char, bool>), typeof(Func<char, bool>) });
        internal static MethodInfo Scanner_GetCharFromIndex = typeof(IdentifiersScannerExtensions).GetMethod(nameof(Parlot.UsefulParsers.IdentifiersScannerExtensions.GetCharFromIndex), new Type[2] { typeof(Scanner), typeof(int) });
        internal static MethodInfo Characters_Unescape = typeof(QuotedIdentifierCharacters).GetMethod("Unescape", new[] { typeof(TextSpan), typeof(char) });

        public static MethodCallExpression ReadSingleQuotedIdentifier(this CompilationContext context) => Expression.Call(null, Scanner_ReadSingleQuotedIdentifier, context.Scanner());
        public static MethodCallExpression ReadDoubleQuotedIdentifier(this CompilationContext context) => Expression.Call(null, Scanner_ReadDoubleQuotedIdentifier, context.Scanner());
        public static MethodCallExpression ReadSquareBracketsQuotedIdentifier(this CompilationContext context) => Expression.Call(null, Scanner_ReadSquareBracketsQuotedIdentifier, context.Scanner());
        public static MethodCallExpression ReadQuotedIdentifier(this CompilationContext context, Func<char, bool> isQuoteStartChar, Func<char, char> getQuoteEndChar) => Expression.Call(null, Scanner_ReadQuotedIdentifierWithPredicate, context.Scanner(), Expression.Constant(isQuoteStartChar), Expression.Constant(getQuoteEndChar));
        public static MethodCallExpression ReadFirstThenOthers(this CompilationContext context, Func<char, bool> isStart, Func<char, bool> isPart) => Expression.Call(context.Scanner(), Scanner_ReadFirstThenOthers, Expression.Constant(isStart), Expression.Constant(isPart));
        public static MethodCallExpression GetCharFromIndex(this CompilationContext context, Expression idx) => Expression.Call(null, Scanner_GetCharFromIndex, context.Scanner(), idx);
        public static MethodCallExpression Unescape(this CompilationContext context, Expression textSpan, Expression charToEscape) => Expression.Call(Characters_Unescape, textSpan, charToEscape);
    }
}

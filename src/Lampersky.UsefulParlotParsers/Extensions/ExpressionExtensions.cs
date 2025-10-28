using System.Linq.Expressions;

namespace Lampersky.Sql.Extensions
{
    public static class ExpressionExtensions
    {
        public static Func<T, bool> IsAtLeastOneSatisfied<T>(this Func<T, bool>[] predicates)
        {
            var logicalDisjunction = (T t) =>
            {
                var test = false;
                foreach (var predicate in predicates)
                {
                    if (predicate(t))
                        test = true;
                }
                return test;
            };

            return logicalDisjunction;
        }

        public static Func<char, bool> CreateIsStartCharPredicate(params (char startSymbol, char endSymbol)[] startEndSymbols)
        {
            if (startEndSymbols.Length == 0)
            {
                return (char c) => false;
            }

            ParameterExpression charParameter = Expression.Parameter(typeof(char), "char");
            Expression combined = Expression.Invoke((char c) => false, charParameter);
            foreach (var (startSymbol, endSymbol) in startEndSymbols)
            {
                Expression<Func<char, bool>> predicate = (char c) => c == startSymbol;
                var invocationExpression = Expression.Invoke(predicate, charParameter);
                combined = combined == null ? invocationExpression : Expression.OrElse(combined, invocationExpression);
            }

            return Expression.Lambda<Func<char, bool>>(combined, charParameter).Compile();
        }

        public static Func<char, char> CreateGetEndCharPredicate(params (char startSymbol, char endSymbol)[] startEndSymbols)
        {
            if (startEndSymbols.Length == 0)
            {
                return (char c) => c;
            }

            ParameterExpression charParameter = Expression.Parameter(typeof(char), "char");
            var cases = startEndSymbols.Select((item => Expression.SwitchCase(Expression.Constant(item.endSymbol), Expression.Constant(item.startSymbol)))).ToList();
            var switchExpr = Expression.Switch(charParameter, charParameter, null, cases);

            return Expression.Lambda<Func<char, char>>(switchExpr, charParameter).Compile();
        }
    }
}

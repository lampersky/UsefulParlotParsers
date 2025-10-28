using Parlot;

namespace Lampersky.Fluent
{
    public static class SimpleSqlIdentifierCharacters
    {
        public static bool IsSimpleSqlIdentifierPart(char ch) => IsSimpleSqlIdentifierStart(ch) || Character.IsDecimalDigit(ch);

        public static bool IsSimpleSqlIdentifierStart(char ch)
            => (ch == '_') ||
               (ch >= 'A' && ch <= 'Z') ||
               (ch >= 'a' && ch <= 'z');
    }
}

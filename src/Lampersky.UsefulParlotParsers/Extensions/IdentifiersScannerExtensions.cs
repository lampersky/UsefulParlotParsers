using Parlot;
using System.Runtime.CompilerServices;

namespace Lampersky.UsefulParsers
{
    public static class IdentifiersScannerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadSingleQuotedIdentifier(this Scanner scanner) => scanner.ReadSingleQuotedIdentifier(out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadSingleQuotedIdentifier(this Scanner scanner, out ReadOnlySpan<char> result)
        {
            return scanner.ReadQuotedIdentifier('\'', out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadDoubleQuotedIdentifier(this Scanner scanner) => scanner.ReadDoubleQuotedIdentifier(out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadDoubleQuotedIdentifier(this Scanner scanner, out ReadOnlySpan<char> result)
        {
            return scanner.ReadQuotedIdentifier('\"', out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadSquareBracketsQuotedIdentifier(this Scanner scanner) => scanner.ReadSquareBracketsQuotedIdentifier(out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadSquareBracketsQuotedIdentifier(this Scanner scanner, out ReadOnlySpan<char> result)
        {
            return scanner.ReadQuotedIdentifier('[', ']', out result);
        }

        private static bool ReadQuotedIdentifier(this Scanner scanner, char quoteChar, out ReadOnlySpan<char> result)
        {
            return scanner.ReadQuotedIdentifier(quoteChar, quoteChar, out result);
        }

        private static bool ReadQuotedIdentifier(this Scanner scanner, char quoteStartChar, char quoteEndChar, out ReadOnlySpan<char> result)
        {
            var first = scanner.Cursor.Current;

            if (first == quoteStartChar)
            {
                var start = scanner.Cursor.Offset;

                scanner.Cursor.AdvanceNoNewLines(1);

                var isClosed = false;

                while (!scanner.Cursor.Eof)
                {
                    if (quoteEndChar == scanner.Cursor.Current)
                    {
                        if (quoteEndChar == scanner.Cursor.PeekNext(1))
                        {
                            scanner.Cursor.AdvanceNoNewLines(2);
                            continue;
                        }
                        isClosed = true;
                        scanner.Cursor.AdvanceNoNewLines(1);
                        break;
                    }
                    scanner.Cursor.Advance(1);
                }

                if (!isClosed)
                {
                    throw new ParseException($"Unclosed quotation mark '{quoteEndChar}'.", scanner.Cursor.Position);
                }

                if (scanner.Cursor.Offset - start == 2)
                {
                    throw new ParseException("Invalid zero length identifier.", scanner.Cursor.Position);
                }

                result = scanner.Buffer.AsSpan(start, scanner.Cursor.Offset - start);
                return true;
            }

            result = [];

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadQuotedIdentifier(this Scanner scanner, Func<char, bool> isQuoteStartChar, Func<char, char> getQuoteEndChar) => scanner.ReadQuotedIdentifier(isQuoteStartChar, getQuoteEndChar, out _);

        private static bool ReadQuotedIdentifier(this Scanner scanner, Func<char, bool> isQuoteStartChar, Func<char, char> getQuoteEndChar, out ReadOnlySpan<char> result)
        {
            var first = scanner.Cursor.Current;
            if (isQuoteStartChar(first))
            {
                var quoteEndChar = getQuoteEndChar(first);

                return scanner.ReadQuotedIdentifier(first, quoteEndChar, out result);
            }

            result = [];

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetCharFromIndex(this Scanner scanner, int position)
        {
            return scanner.Buffer[position];
        }
    }
}

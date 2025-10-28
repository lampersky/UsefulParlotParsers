using Parlot.Sql.Extensions;
using System.Runtime.CompilerServices;

namespace Parlot.UsefulParsers
{
    public static class CommentsScannerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Until(this Scanner scanner, params Func<Scanner, bool>[] predicates)
        {
            if (predicates == null || predicates.Length == 0)
            {
                return false;
            }

            var testPredicates = predicates.IsAtLeastOneSatisfied();

            var found = false;

            while (testPredicates(scanner))
            {
                found = true;
            }

            return found;
        }

        public static bool SkipSingleLineComment(this Scanner scanner, Func<Cursor, bool> isCommentStartSymbol)
        {
            if (scanner.Cursor.Eof || !isCommentStartSymbol(scanner.Cursor))
            {
                return false;
            }

            while (!scanner.Cursor.Eof && !Character.IsNewLine(scanner.Cursor.Current))
            {
                scanner.Cursor.Advance();
            }

            scanner.Cursor.Advance(1);

            return true;
        }

        public static bool SkipSingleLineComment(this Scanner scanner, char commentStartSymbol)
        {
            return scanner.SkipSingleLineComment(c => c.PeekNext(c.Offset) == commentStartSymbol);
        }

        public static bool SkipSingleLineComment(this Scanner scanner, string commentStartSymbol)
        {
            return scanner.SkipSingleLineComment(c => c.Buffer.AsSpan(c.Offset).StartsWith(commentStartSymbol.AsSpan()));
        }

        public static bool SkipMultiLineComment(this Scanner scanner, string commentStartSymbol, string commentEndSymbol, bool isNestingAllowed)
        {
            if (scanner.Cursor.Eof || string.IsNullOrEmpty(commentStartSymbol))
            {
                return false;
            }

            var startSymbol = commentStartSymbol.AsSpan();

            var span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset);

            if (!span.StartsWith(startSymbol))
            {
                return false;
            }

            scanner.Cursor.Advance(startSymbol.Length);

            if (scanner.Cursor.Eof)
            {
                throw new ParseException($"Missing end comment mark '{commentEndSymbol}'.", scanner.Cursor.Position);
            }

            var endSymbol = commentEndSymbol.AsSpan();

            if (isNestingAllowed)
            {
                var startCount = 1; /* we are already in the comment */
                var endCount = 0;

                while (!scanner.Cursor.Eof)
                {
                    span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset);
                    if (span.StartsWith(startSymbol))
                    {
                        startCount++;
                        scanner.Cursor.Advance(startSymbol.Length);
                    }
                    else if (span.StartsWith(endSymbol))
                    {
                        endCount++;
                        scanner.Cursor.Advance(endSymbol.Length);
                        if (endCount >= startCount)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        scanner.Cursor.Advance(1);
                    }
                }
            }
            else
            {
                var resetPosition = scanner.Cursor.Position;

                while (!scanner.Cursor.Eof)
                {
                    span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset);
                    if (span.StartsWith(startSymbol))
                    {
                        // if new start symbol is spotted, it means that missing closing symbol is before
                        // reseting to last known position
                        scanner.Cursor.ResetPosition(resetPosition);
                        break;
                    }
                    else
                    {
                        if (!span.StartsWith(endSymbol))
                        {
                            scanner.Cursor.Advance();
                            resetPosition = scanner.Cursor.Position;
                        }
                        else
                        {
                            scanner.Cursor.Advance(endSymbol.Length);
                            return true;
                        }
                    }
                }
            }

            throw new ParseException($"Missing end comment mark '{commentEndSymbol}'.", scanner.Cursor.Position);
        }

        public static bool ReadSingleLineComment(this Scanner scanner, Func<Cursor, bool> isCommentStartSymbol, out ReadOnlySpan<char> result)
        {
            if (scanner.Cursor.Eof || !isCommentStartSymbol(scanner.Cursor))
            {
                result = [];
                return false;
            }

            var start = scanner.Cursor.Offset;

            while (!scanner.Cursor.Eof && !Character.IsNewLine(scanner.Cursor.Current))
            {
                scanner.Cursor.Advance();
            }

            scanner.Cursor.Advance(1);

            result = scanner.Buffer.AsSpan(start, scanner.Cursor.Offset - start);
            return true;
        }

        public static bool ReadSingleLineComment(this Scanner scanner, char commentStartSymbol, out ReadOnlySpan<char> result)
        {
            return scanner.ReadSingleLineComment(c => c.PeekNext(c.Offset) == commentStartSymbol, out result);
        }

        public static bool ReadSingleLineComment(this Scanner scanner, string commentStartSymbol, out ReadOnlySpan<char> result)
        {
            return scanner.ReadSingleLineComment(c => c.Buffer.AsSpan(c.Offset, commentStartSymbol.Length).StartsWith(commentStartSymbol.AsSpan()), out result);
        }

        public static bool ReadMultiLineComment(this Scanner scanner, string commentStartSymbol, string commentEndSymbol, bool isNestingAllowed, out ReadOnlySpan<char> result)
        {
            if (scanner.Cursor.Eof || string.IsNullOrEmpty(commentStartSymbol))
            {
                result = [];
                return false;
            }

            var startSymbol = commentStartSymbol.AsSpan();
            var span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset, startSymbol.Length);

            if (!span.StartsWith(startSymbol))
            {
                result = [];
                return false;
            }

            var start = scanner.Cursor.Offset;

            scanner.Cursor.Advance(startSymbol.Length);

            if (scanner.Cursor.Eof)
            {
                throw new ParseException($"Missing end comment mark '{commentEndSymbol}'.", scanner.Cursor.Position);
            }

            var endSymbol = commentEndSymbol.AsSpan();

            if (isNestingAllowed)
            {
                var startCount = 1; /* we are already in the comment */
                var endCount = 0;

                while (!scanner.Cursor.Eof)
                {
                    span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset);
                    if (span.StartsWith(startSymbol))
                    {
                        startCount++;
                        scanner.Cursor.Advance(startSymbol.Length);
                    }
                    else if (span.StartsWith(endSymbol))
                    {
                        endCount++;
                        scanner.Cursor.Advance(endSymbol.Length);
                        if (endCount >= startCount)
                        {
                            result = scanner.Buffer.AsSpan(start, scanner.Cursor.Offset - start);
                            return true;
                        }
                    }
                    else
                    {
                        scanner.Cursor.Advance(1);
                    }
                }
            }
            else
            {
                var resetPosition = scanner.Cursor.Position;

                while (!scanner.Cursor.Eof)
                {
                    span = scanner.Cursor.Buffer.AsSpan(scanner.Cursor.Offset);
                    if (span.StartsWith(startSymbol))
                    {
                        // if new start symbol is spotted, it means that missing closing symbol is before
                        // reseting to last known position
                        scanner.Cursor.ResetPosition(resetPosition);
                        break;
                    }
                    else
                    {
                        if (!span.StartsWith(endSymbol))
                        {
                            scanner.Cursor.Advance();
                            resetPosition = scanner.Cursor.Position;
                        }
                        else
                        {
                            result = scanner.Buffer.AsSpan(start, scanner.Cursor.Offset - start);
                            return true;
                        }
                    }
                }
            }

            throw new ParseException($"Missing end comment mark '{commentEndSymbol}'.", scanner.Cursor.Position);
        }
    }
}

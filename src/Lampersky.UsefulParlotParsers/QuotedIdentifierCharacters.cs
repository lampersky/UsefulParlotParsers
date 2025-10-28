namespace Parlot.UsefulParsers
{
    public static class QuotedIdentifierCharacters
    {
        public static TextSpan Escape(TextSpan span, char charToEscape)
        {
            var occurrences = span.Buffer.Count(c => c == charToEscape);
            if (occurrences == 0)
            {
                return span;
            }

#if NETSTANDARD2_0
            var result = QuotedIdentifierCharacters.CreateString(span.Length + occurrences, (span, charToEscape), static (output, src) =>
#else
            var result = String.Create(span.Length + occurrences, (span, charToEscape), static (output, src) =>
#endif
            {
                var idx = 0;
                var buffer = src.span.Buffer;
                var end = src.span.Offset + src.span.Length;

                for (var i = src.span.Offset; i < end; i++)
                {
                    var c = buffer[i];

                    if (c == src.charToEscape)
                    {
                        output[idx++] = c;
                    }

                    output[idx++] = c;
                }
            });

            for (var i = result.Length - 1; i >= 0; i--)
            {
                if (result[i] != '\0')
                {
                    return new TextSpan(result, 0, i + 1);
                }
            }

            return new TextSpan(result);
        }

        public static TextSpan Unescape(TextSpan span, char escapeChar)
        {
            var subSpan = new char[] { escapeChar, escapeChar }.AsSpan();
            if (span.Buffer.AsSpan(span.Offset, span.Length).IndexOf(subSpan) == -1)
            {
                return span;
            }

#if NETSTANDARD2_0
            var result = QuotedIdentifierCharacters.CreateString(span.Length, (span, escapeChar), static (output, src) =>
#else
            var result = String.Create(span.Length, (span, escapeChar), static (output, src) =>
#endif
            {
                var idx = 0;
                var buffer = src.span.Buffer;
                var end = src.span.Offset + src.span.Length;

                //src.span.Buffer.AsSpan().IndexOf(subSpan);

                for (var i = src.span.Offset; i < end; i++)
                {
                    var c = buffer[i];

                    if (i + 1 < end && c == src.escapeChar && buffer[i + 1] == src.escapeChar)
                    {
                        i++;
                    }

                    output[idx++] = c;
                }

                output[idx++] = '\0';
            });

            for (var i = result.Length - 1; i >= 0; i--)
            {
                if (result[i] != '\0')
                {
                    return new TextSpan(result, 0, i + 1);
                }
            }

            return new TextSpan(result);
        }

#if NETSTANDARD2_0
        private delegate void SpanAction<T, in TArg>(T[] span, TArg arg);
        private static string CreateString<TState>(int length, TState state, SpanAction<char, TState> action)
        {
            var array = new char[length];

            action(array, state);

            return new string(array);
        }
#endif
    }
}

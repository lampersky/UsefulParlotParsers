using Lampersky.Fluent;
using Newtonsoft.Json.Linq;
using Parlot;
using Parlot.Fluent;
using System.Text.Json.Nodes;
using Xunit;

namespace Lampersky.UsefulParsers.Tests
{
    public class PlaygroundTests
    {
        public static bool IsKeyValuePart(char ch) => IsKeyValueStart(ch) || Character.IsDecimalDigit(ch);

        public static bool IsKeyValueStart(char ch) => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');

        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("key1:value1", new [] { "0@0:11" })]
        [InlineData("key1 : value1\r\n", new[] { "0@0:13" })]
        [InlineData("key1 : value1\r\nkey2 : value22\r\nkey3 : value333\r\n", new [] { "0@0:13", "1@15:29", "2@31:46" })]
        public void ShouldParseKeyValuePairs(string text, string[] expected)
        {
            var COLON = Terms.Char(':');
            var EOL = Literals.Text("\r\n");

            var keyValueParser = Terms.CustomIdentifier(IsKeyValueStart, IsKeyValuePart);

            //var pair = Separated(COLON, keyValueParser)
            //    .Then(result => (StartIndex: result[0].Offset, EndIndex: result[^1].Offset + result[^1].Length));

            var pair = keyValueParser.AndSkip(COLON).And(keyValueParser)
                .Then(result => (StartIndex: result.Item1.Offset, EndIndex: result.Item2.Offset + result.Item2.Length));

            var pairs = Separated(EOL, pair)
                .Then(result => result.Select((p, idx) => $"{idx}@{p.StartIndex}:{p.EndIndex}").ToList())
                .Else([]);

            var parsed = pairs.TryParse(text, out var keyValuePairs);

            Assert.True(parsed);
            Assert.Equal(keyValuePairs, expected);
        }
    }
}

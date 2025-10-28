using Lampersky.Fluent;
using Xunit;

namespace Lampersky.UsefulParsers.Tests
{
    public class CustomIdentifierTests
    {
        [Fact]
        public void ShouldParseCustomIdentifier()
        {
            var identifier = Terms.CustomIdentifier(s => s == 'a', p => p == 'b').Then(static x => x.ToString());

            Assert.True(identifier.TryParse("ab", out var result));
            Assert.Equal("ab", result);

            Assert.True(identifier.TryParse("abbbb", out result));
            Assert.Equal("abbbb", result);

            Assert.True(identifier.TryParse("a", out result));
            Assert.Equal("a", result);

            Assert.False(identifier.TryParse("ba", out result));
            Assert.Equal(default, result);
        }

        [Fact]
        public void ShouldParseCustomIdentifierCompiled()
        {
            var identifier = Terms.CustomIdentifier(s => s == 'a', p => p == 'b').Compile().Then(static x => x.ToString());

            Assert.True(identifier.TryParse("ab", out var result));
            Assert.Equal("ab", result);

            Assert.True(identifier.TryParse("abbbb", out result));
            Assert.Equal("abbbb", result);

            Assert.True(identifier.TryParse("a", out result));
            Assert.Equal("a", result);

            Assert.False(identifier.TryParse("ba", out result));
            Assert.Equal(default, result);
        }
    }
}

namespace Parlot.UsefulParsers
{
    public static class CustomIdentifierCharacters
    {
        public static char[] GetCustomIdentifierExpectedCharacters(Func<char, bool> isStart)
        {
            var expectedChars = new List<char>();
            for (int i = 0; i < 256; i++)
            {
                if (isStart((char)i))
                {
                    expectedChars.Add((char)i);
                }
            }

            return expectedChars.ToArray();
        }
    }
}

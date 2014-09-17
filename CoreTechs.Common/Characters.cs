using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public static class Characters
    {
        public static IEnumerable<char> All
        {
            get { return Enumerable.Range(char.MinValue, char.MaxValue + 1).Select(i => (char)i); }
        }

        public static IEnumerable<char> Letters
        {
            get { return All.Where(char.IsLetter); }
        }

        public static IEnumerable<char> ControlCharacters
        {
            get { return All.Where(char.IsControl); }
        }

        public static IEnumerable<char> Digits
        {
            get { return All.Where(char.IsDigit); }
        }

        public static IEnumerable<char> HighSurrogates
        {
            get { return All.Where(char.IsHighSurrogate); }
        }

        public static IEnumerable<char> LettersAndDigits
        {
            get { return All.Where(char.IsLetterOrDigit); }
        }

        public static IEnumerable<char> LowerCaseCharacters
        {
            get { return All.Where(char.IsLower); }
        }

        public static IEnumerable<char> LowSurrogates
        {
            get { return All.Where(char.IsLowSurrogate); }
        }

        public static IEnumerable<char> Numbers
        {
            get { return All.Where(char.IsNumber); }
        }

        public static IEnumerable<char> PunctuationCharacters
        {
            get { return All.Where(char.IsPunctuation); }
        }

        public static IEnumerable<char> Separators
        {
            get { return All.Where(char.IsSeparator); }
        }

        public static IEnumerable<char> Symbols
        {
            get { return All.Where(char.IsSymbol); }
        }

        public static IEnumerable<char> UpperCaseCharacters
        {
            get { return All.Where(char.IsUpper); }
        }

        public static IEnumerable<char> WhiteSpaceCharacters
        {
            get { return All.Where(char.IsWhiteSpace); }
        }

        public static IEnumerable<char> WhereAny(CharTypes types)
        {
            Func<char, bool> predicate = c => false;

            if (types.HasFlag(CharTypes.Control))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsControl(c);
            }

            if (types.HasFlag(CharTypes.Digit))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsDigit(c);
            }

            if (types.HasFlag(CharTypes.HighSurrogate))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsHighSurrogate(c);
            }

            if (types.HasFlag(CharTypes.Letter))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsLetter(c);
            }

            if (types.HasFlag(CharTypes.LowerCase))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsLower(c);
            }

            if (types.HasFlag(CharTypes.LowSurrogate))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsLowSurrogate(c);
            }

            if (types.HasFlag(CharTypes.Number))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsNumber(c);
            }

            if (types.HasFlag(CharTypes.Punctuation))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsPunctuation(c);
            }

            if (types.HasFlag(CharTypes.Symbol))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsSymbol(c);
            }

            if (types.HasFlag(CharTypes.UpperCase))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsUpper(c);
            }

            if (types.HasFlag(CharTypes.WhiteSpace))
            {
                var f = predicate;
                predicate = c => f(c) || char.IsWhiteSpace(c);
            }

            return All.Where(predicate);
        }
    }

    [Flags]
    public enum CharTypes
    {
        Letter = 1,
        Digit = 2,
        Number = 4,
        Symbol = 8,
        Punctuation = 16,
        WhiteSpace = 32,
        UpperCase = 64,
        LowerCase = 128,
        Control = 256,
        HighSurrogate = 512,
        LowSurrogate = 1024
    }
}

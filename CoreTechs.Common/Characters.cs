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

        private static Func<char, bool> MapPredicate(CharTypes type)
        {
            switch (type)
            {
                case CharTypes.Letter: return char.IsLetter;
                case CharTypes.Digit: return char.IsDigit;
                case CharTypes.Number: return char.IsNumber;
                case CharTypes.Symbol: return char.IsSymbol;
                case CharTypes.Punctuation: return char.IsPunctuation;
                case CharTypes.WhiteSpace: return char.IsWhiteSpace;
                case CharTypes.UpperCase: return char.IsUpper;
                case CharTypes.LowerCase: return char.IsLower;
                case CharTypes.Control: return char.IsControl;
                case CharTypes.HighSurrogate: return char.IsHighSurrogate;
                case CharTypes.LowSurrogate: return char.IsLowSurrogate;
                case CharTypes.Separator: return char.IsSeparator;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public static IEnumerable<char> WhereAny(CharTypes types)
        {
            return All.WhereAny(types);
        }

        public static IEnumerable<char> WhereAny(this IEnumerable<char> chars, CharTypes types)
        {
            var predicate = Enum.GetValues(typeof(CharTypes))
                .Cast<CharTypes>()
                .ToDictionary(x => x, MapPredicate)
                .Where(x => types.HasFlag(x.Key))
                .Select(x => x.Value)
                .AnyTrue();

            return chars.Where(predicate);
        }

        public static class Keyboard
        {
            public const string Digits = "0123456789";
            public const string LowerLetters = "abcdefghijklmnopqrstuvwxyz";
            public const string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            public const string AllLetters = LowerLetters + UpperLetters;
            public const string AllLettersAndDigits = AllLetters + Digits;
            public const string Symbols = "~`$^+=<>|";
            public const string Punctuation = @"!@#%&*()_-?,./:"";'{}[]\'";
            public const string All = AllLettersAndDigits + Symbols + Punctuation;
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
        LowSurrogate = 1024,
        Separator = 2048,
    }
}

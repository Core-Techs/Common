using System;

namespace CoreTechs.Common.Measures
{
    public class Length : IComparable<Length>
    {
        public static class ConversionFactors
        {
            public const decimal Meter = 1;
            public const decimal Kilometer = Meter * 1000;
            public const decimal Centimeter = Meter / 100;
            public const decimal Millimeter = Meter / 1000;
            public const decimal Inch = 0.0254m;
            public const decimal Foot = Inch * 12;
            public const decimal Yard = Foot * 3;
            public const decimal Mile = Foot * 5280;
            public const decimal AstronomicalUnit = 149597870700;
            public const decimal LightYear = 9.4605284e15m;
            public const decimal Parsec = 3.08567758e16m;
            public const decimal HubbleLength = 9.460499999987408e24m;
            public const decimal Cubit = 2.1872265966754156m;
        }

        public decimal Meters { get; private set; }
        public decimal Kilometers { get { return Meters / ConversionFactors.Kilometer; } }
        public decimal Centimeters { get { return Meters / ConversionFactors.Centimeter; } }
        public decimal Millimeters { get { return Meters / ConversionFactors.Millimeter; } }
        public decimal Inches { get { return Meters / ConversionFactors.Inch; } }
        public decimal Feet { get { return Meters / ConversionFactors.Foot; } }
        public decimal Yards { get { return Meters / ConversionFactors.Yard; } }
        public decimal Miles { get { return Meters / ConversionFactors.Mile; } }
        public decimal AstronomicalUnits { get { return Meters / ConversionFactors.AstronomicalUnit; } }
        public decimal LightYears { get { return Meters / ConversionFactors.LightYear; } }
        public decimal Parsecs { get { return Meters / ConversionFactors.Parsec; } }
        public decimal HubbleLengths { get { return Meters / ConversionFactors.HubbleLength; } }
        public decimal Cubits { get { return Meters / ConversionFactors.Cubit; } }

        private Length(decimal meters)
        {
            Meters = meters;
        }

        public static Length FromMeters(decimal meters)
        {
            return new Length(meters);
        }

        public static Length FromKilometers(decimal kilometers)
        {
            var meters = kilometers * ConversionFactors.Kilometer;
            return new Length(meters);
        }

        public static Length FromCentimeters(decimal centimeters)
        {
            var meters = centimeters * ConversionFactors.Centimeter;
            return new Length(meters);
        }

        public static Length FromMillimeters(decimal millimeters)
        {
            var meters = millimeters * ConversionFactors.Millimeter;
            return new Length(meters);
        }

        public static Length FromInches(decimal inches)
        {
            var meters = inches * ConversionFactors.Inch;
            return new Length(meters);
        }

        public static Length FromFeet(decimal feet)
        {
            var meters = feet * ConversionFactors.Foot;
            return new Length(meters);
        }

        public static Length FromYards(decimal yards)
        {
            var meters = yards * ConversionFactors.Yard;
            return new Length(meters);
        }

        public static Length FromMiles(decimal miles)
        {
            var meters = miles * ConversionFactors.Mile;
            return new Length(meters);
        }

        public static Length FromAstronomicalUnits(decimal astronomicalUnits)
        {
            var meters = astronomicalUnits * ConversionFactors.AstronomicalUnit;
            return new Length(meters);
        }

        public static Length FromLightYears(decimal lightYears)
        {
            var meters = lightYears * ConversionFactors.LightYear;
            return new Length(meters);
        }

        public static Length FromParsecs(decimal parsecs)
        {
            var meters = parsecs * ConversionFactors.Parsec;
            return new Length(meters);
        }

        public static Length FromCubits(decimal cubits)
        {
            var meters = cubits * ConversionFactors.Cubit;
            return new Length(meters);
        }

        public int CompareTo(Length other)
        {
            if (Equals(other))
                return 0;

            return Meters < other.Meters ? -1 : 1;
        }

        protected bool Equals(Length other)
        {
            return Meters == other.Meters;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Length) obj);
        }

        public override int GetHashCode()
        {
            return Meters.GetHashCode();
        }

        public static bool operator ==(Length left, Length right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Length left, Length right)
        {
            return !Equals(left, right);
        }
    }
}

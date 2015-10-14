using System;

namespace CoreTechs.Common
{
    public class ByteSize : IEquatable<ByteSize>, IComparable<ByteSize>
    {
        public static class ConversionFactors
        {
            public const double Byte = 1;
            public const double Kilobyte = Byte * 1024;
            public const double Megabyte = Kilobyte * 1024;
            public const double Gigabyte = Megabyte * 1024;
            public const double Terabyte = Gigabyte * 1024;
            public const double Petabyte = Terabyte * 1024;
        }

        public long Bytes { get; }

        private double? _kb;
        public double Kilobytes
        {
            get
            {
                if (!_kb.HasValue)
                    _kb = Bytes / ConversionFactors.Kilobyte;

                return _kb.Value;
            }
        }

        private double? _mb;
        public double Megabytes
        {
            get
            {
                if (!_mb.HasValue)
                    _mb = Bytes / ConversionFactors.Megabyte;

                return _mb.Value;
            }
        }

        private double? _gb;
        public double Gigabytes
        {
            get
            {
                if (!_gb.HasValue)
                    _gb = Bytes / ConversionFactors.Gigabyte;

                return _gb.Value;
            }
        }

        private double? _tb;
        public double Terabytes
        {
            get
            {
                if (!_tb.HasValue)
                    _tb = Bytes / ConversionFactors.Terabyte;

                return _tb.Value;
            }
        }

        private double? _pb;
        public double Petabytes
        {
            get
            {
                if (!_pb.HasValue)
                    _pb = Bytes / ConversionFactors.Petabyte;

                return _pb.Value;
            }
        }

        public ByteSize(long bytes)
        {
            Bytes = bytes;
        }

        public static ByteSize FromBytes(long bytes)
        {
            return new ByteSize(bytes);
        }

        public static ByteSize FromKilobytes(double kilobytes)
        {
            var bytes = kilobytes * ConversionFactors.Kilobyte;
            return new ByteSize((long)bytes);
        }

        public static ByteSize FromMegabytes(double megabytes)
        {
            var bytes = megabytes * ConversionFactors.Megabyte;
            return new ByteSize((long)bytes);
        }

        public static ByteSize FromGigabytes(double gigabytes)
        {
            var bytes = gigabytes * ConversionFactors.Gigabyte;
            return new ByteSize((long)bytes);
        }

        public static ByteSize FromTerabytes(double terabytes)
        {
            var bytes = terabytes * ConversionFactors.Terabyte;
            return new ByteSize((long)bytes);
        }

        public static ByteSize FromPetabytes(double petabytes)
        {
            var bytes = petabytes * ConversionFactors.Petabyte;
            return new ByteSize((long)bytes);
        }

        public bool Equals(ByteSize other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Bytes == other.Bytes;
        }

        public int CompareTo(ByteSize other)
        {
            return Bytes.CompareTo(other.Bytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ByteSize)obj);
        }

        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        public static bool operator ==(ByteSize left, ByteSize right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ByteSize left, ByteSize right)
        {
            return !Equals(left, right);
        }

        public static ByteSize operator +(ByteSize left, ByteSize right)
        {
            return new ByteSize(left.Bytes + right.Bytes);
        }

        public static ByteSize operator -(ByteSize left, ByteSize right)
        {
            return new ByteSize(left.Bytes - right.Bytes);
        }

        public static bool operator <(ByteSize left, ByteSize right)
        {
            return left.Bytes < right.Bytes;
        }

        public static bool operator >(ByteSize left, ByteSize right)
        {
            return left.Bytes > right.Bytes;
        }

        public static bool operator <=(ByteSize left, ByteSize right)
        {
            return left.Bytes <= right.Bytes;
        }

        public static bool operator >=(ByteSize left, ByteSize right)
        {
            return left.Bytes >= right.Bytes;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    class TypeConversionTests
    {
        [Test, TestCaseSource("GetConversionCases")]
        public void CanConvert(Type sourceType, object value, Type targetType, object expectedValue, bool recip)
        {
            Assert.AreEqual(expectedValue, value.ConvertTo(targetType),
                string.Format("couldn't convert {0} to {1}", sourceType.Name, targetType.Name));

            // do reciprocal test

            if(recip)
            Assert.AreEqual(value, expectedValue.ConvertTo(sourceType),
                string.Format("couldn't do reciprocal conversion: {0} to {1}", expectedValue.GetType().Name,
                    value.GetType().Name));
        }

        public IEnumerable<object[]> GetConversionCases()
        {
            yield return ConversionCase("01:00:00", TimeSpan.FromHours(1));

            var mybday = new DateTime(1984, 5, 10);
            yield return ConversionCase("may 10 1984", mybday, false);
            yield return ConversionCase(mybday.ToString(CultureInfo.CurrentCulture), mybday);
            yield return ConversionCase(mybday, new DateTimeOffset(mybday));



            foreach (var c in GetNumericConversionCases())
                yield return c;

        }

        private IEnumerable<object[]> GetNumericConversionCases()
        {
            var values = new dynamic[]
            {
                (byte)1,
                (short)1,
                1,
                1L,
                1F,
                1D,
                1M,
            };

            // return test case(s) for all permutations

            foreach (var v1 in values)
            foreach (var v2 in values)
            {
                yield return ConversionCase(v1, v2);

                var n1 = Activator.CreateInstance(typeof (Nullable<>).MakeGenericType(v1.GetType()), new object[] {v1});
                var n2 = Activator.CreateInstance(typeof (Nullable<>).MakeGenericType(v2.GetType()), new object[] {v2});

                yield return ConversionCase(n1, n2);
                yield return ConversionCase(v1, n1);
                yield return ConversionCase(v1, n2);
                yield return ConversionCase(v2, n1);
                yield return ConversionCase(v2, n2);
                
            }
        }

        private static object[] ConversionCase<TIn, TOut>(TIn inValue = default(TIn), TOut outValue = default(TOut),bool recip = true)
        {
            return new object[] {inValue.GetType(), inValue, outValue.GetType(), outValue, recip};
        }

       
    }
}
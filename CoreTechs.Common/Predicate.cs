using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTechs.Common
{
    public class Pred<T>
    {
        public Pred(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            Func = predicate;
        }

        public static Pred<T> False
        {
            get { return new Pred<T>(_ => false); }
        }

        public static Pred<T> True
        {
            get { return new Pred<T>(_ => true); }
        }

        public Func<T, bool> Func { get; }

        public Pred<T> And(Func<T, bool> predicate, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return shortCircuit
                ? new Pred<T>(x => Func(x) && predicate(x))
                : new Pred<T>(x => Func(x) & predicate(x));
        }

        public Pred<T> Or(Func<T, bool> predicate, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return shortCircuit
                ? new Pred<T>(x => Func(x) || predicate(x))
                : new Pred<T>(x => Func(x) | predicate(x));
        }

        public static implicit operator Pred<T>(Predicate<T> predicate)
        {
            return new Pred<T>(x => predicate(x));
        }

        public static implicit operator Predicate<T>(Pred<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return predicate;
        }

        public static implicit operator Func<T, bool>(Pred<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return predicate.Func;
        }

        public static implicit operator Pred<T>(Func<T, bool> func)
        {
            return new Pred<T>(func);
        }

        public static Pred<T> All(params Func<T, bool>[] predicates)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return predicates.Aggregate((a, b) => a.And(b));
        }

        public static Pred<T> Any(params Func<T, bool>[] predicates)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return predicates.Aggregate((a, b) => a.Or(b));
        }

        public static Pred<T> All(IEnumerable<Func<T, bool>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return predicates.Aggregate((a, b) => new Pred<T>(a).And(b, shortCircuit));
        }

        public static Pred<T> Any(IEnumerable<Func<T, bool>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return predicates.Aggregate((a, b) => new Pred<T>(a).Or(b, shortCircuit));
        }

        public bool Invoke(T x)
        {
            return Func(x);
        }

        public Pred<T> Invert()
        {
            return new Pred<T>(x => !Func(x));
        }
    }

    public static class PredicateExtensions
    {
        public static Func<T, bool> Invert<T>(this Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new Pred<T>(predicate).Invert();
        }

        public static Func<T, bool> Invert<T>(this Predicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return predicate.AsFunc().Invert();
        }

        public static Func<T, bool> And<T>(this Func<T, bool> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));

            return new Pred<T>(predicate).And(other, shortCircuit);
        }

        public static Predicate<T> And<T>(this Predicate<T> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));

            return new Pred<T>(x => predicate(x)).And(other, shortCircuit);
        }

        public static Func<T, bool> Or<T>(this Func<T, bool> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Pred<T>(predicate).Or(other, shortCircuit);
        }

        public static Predicate<T> Or<T>(this Predicate<T> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Pred<T>(x => predicate(x)).Or(other, shortCircuit);
        }

        public static Func<T, bool> AllTrue<T>(this IEnumerable<Func<T, bool>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.All(predicates, shortCircuit);
        }

        public static Func<T, bool> AnyTrue<T>(this IEnumerable<Func<T, bool>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.Any(predicates, shortCircuit);
        }

        public static Func<T, bool> AsFunc<T>(this Predicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return x => predicate(x);
        }

        public static Predicate<T> AllTrue<T>(this IEnumerable<Predicate<T>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.All(predicates.Select(AsFunc), shortCircuit);
        }

        public static Predicate<T> AnyTrue<T>(this IEnumerable<Predicate<T>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.Any(predicates.Select(AsFunc), shortCircuit);
        }

        public static Pred<T> AllTrue<T>(this IEnumerable<Pred<T>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.All(predicates.Select(p => p.Func), shortCircuit);
        }

        public static Pred<T> AnyTrue<T>(this IEnumerable<Pred<T>> predicates, bool shortCircuit = true)
        {
            if (predicates == null) throw new ArgumentNullException(nameof(predicates));
            return Pred<T>.Any(predicates.Select(p => p.Func), shortCircuit);
        }
    }
}
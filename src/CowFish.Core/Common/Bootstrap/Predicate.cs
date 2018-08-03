using System;

namespace CowFish.Core.Common.Bootstrap
{
    public class Pred<T>
    {
        public Pred(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            Func = predicate;
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


        public static Func<T, bool> And<T>(this Func<T, bool> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));

            return new Pred<T>(predicate).And(other, shortCircuit);
        }


        public static Func<T, bool> Or<T>(this Func<T, bool> predicate, Pred<T> other, bool shortCircuit = true)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new Pred<T>(predicate).Or(other, shortCircuit);
        }
    }
}
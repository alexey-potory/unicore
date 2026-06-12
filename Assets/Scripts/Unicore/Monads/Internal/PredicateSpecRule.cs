using System;

namespace Unicore.Monads.Internal
{
    internal readonly struct PredicateSpecRule<T> : Spec<T>.ISpecRule
    {
        private readonly Func<T, bool> _predicate;

        public PredicateSpecRule(Func<T, bool> predicate) => _predicate = predicate;

        public bool IsSatisfiedBy(T value) => _predicate(value);
    }
}
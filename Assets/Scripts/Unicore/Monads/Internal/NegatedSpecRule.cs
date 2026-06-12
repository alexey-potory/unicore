namespace Unicore.Monads.Internal
{
    internal readonly struct NegatedSpecRule<T> : Spec<T>.ISpecRule
    {
        private readonly Spec<T>.ISpecRule _rule;

        public NegatedSpecRule(Spec<T>.ISpecRule rule) => _rule = rule;

        public bool IsSatisfiedBy(T value) => !_rule.IsSatisfiedBy(value);
    }
}
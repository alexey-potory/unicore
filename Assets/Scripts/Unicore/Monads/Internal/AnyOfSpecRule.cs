namespace Unicore.Monads.Internal
{
    internal readonly struct AnyOfSpecRule<T> : Spec<T>.ISpecRule
    {
        private readonly Spec<T>.ISpecRule[] _rules;

        public AnyOfSpecRule(params Spec<T>.ISpecRule[] rules) => _rules = rules;

        public bool IsSatisfiedBy(T value)
        {
            foreach (var rule in _rules)
            {
                if (rule.IsSatisfiedBy(value))
                    return true;
            }

            return false;
        }
    }
}
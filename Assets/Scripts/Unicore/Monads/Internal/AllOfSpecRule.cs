namespace Unicore.Monads.Internal
{
    internal readonly struct AllOfSpecRule<T> : Spec<T>.ISpecRule
    {
        private readonly Spec<T>.ISpecRule[] _rules;

        public AllOfSpecRule(params Spec<T>.ISpecRule[] rules) => _rules = rules;

        public bool IsSatisfiedBy(T value)
        {
            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfiedBy(value))
                    return false;
            }

            return true;
        }
    }
}
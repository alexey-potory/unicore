using System;

namespace Unicore.Monads
{
    public interface ISpecRule<in T>
    {
        bool IsSatisfiedBy(T value);
    }

    internal readonly struct PredicateSpecRule<T> : ISpecRule<T>
    {
        private readonly Func<T, bool> _predicate;

        public PredicateSpecRule(Func<T, bool> predicate) => _predicate = predicate;

        public bool IsSatisfiedBy(T value) => _predicate(value);
    }

    internal readonly struct AllOfSpecRule<T> : ISpecRule<T>
    {
        private readonly ISpecRule<T>[] _rules;

        public AllOfSpecRule(params ISpecRule<T>[] rules) => _rules = rules;

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

    internal readonly struct AnyOfSpecRule<T> : ISpecRule<T>
    {
        private readonly ISpecRule<T>[] _rules;

        public AnyOfSpecRule(params ISpecRule<T>[] rules) => _rules = rules;

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

    internal readonly struct NegatedSpecRule<T> : ISpecRule<T>
    {
        private readonly ISpecRule<T> _rule;

        public NegatedSpecRule(ISpecRule<T> rule) => _rule = rule;

        public bool IsSatisfiedBy(T value) => !_rule.IsSatisfiedBy(value);
    }

    public readonly struct Spec<T>
    {
        private readonly ISpecRule<T> _rule;

        public static Spec<T> Create(Func<T, bool> predicate) => new(predicate);
        public static Spec<T> All(params Spec<T>[] specs) => specs.Length == 0
            ? AlwaysTrue()
            : new(new AllOfSpecRule<T>(ToRules(specs)));

        public static Spec<T> Any(params Spec<T>[] specs) => specs.Length == 0
            ? AlwaysFalse()
            : new(new AnyOfSpecRule<T>(ToRules(specs)));

        public static Spec<T> Not(Spec<T> spec) => spec.Not();
        public static Spec<T> AlwaysTrue() => new(_ => true);
        public static Spec<T> AlwaysFalse() => new(_ => false);

        public Spec(Func<T, bool> predicate) => _rule = new PredicateSpecRule<T>(predicate);
        public Spec(ISpecRule<T> rule) => _rule = rule;

        public bool IsSatisfiedBy(T value) => _rule.IsSatisfiedBy(value);
        public bool IsNotSatisfiedBy(T value) => !IsSatisfiedBy(value);

        public Result<T> ToResult(T value) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure();

        public Result<T> ToResult(T value, Exception error) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(error);

        public Result<T> ToResult(T value, Func<Exception> errorFactory) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(errorFactory.Invoke());

        public Result<T> Ensure(T value, Exception error) =>
            ToResult(value, error);

        public Result<T> Ensure(T value, Func<Exception> errorFactory) =>
            ToResult(value, errorFactory);

        public T GetValueOrDefault(T value, T defaultValue = default) =>
            IsSatisfiedBy(value) ? value : defaultValue;

        public Func<T, bool> AsPredicate() => IsSatisfiedBy;

        public Spec<T> And(Spec<T> other) =>
            new(new AllOfSpecRule<T>(_rule, other._rule));

        public Spec<T> Or(Spec<T> other) =>
            new(new AnyOfSpecRule<T>(_rule, other._rule));

        public Spec<T> Not() =>
            new(new NegatedSpecRule<T>(_rule));

        private static ISpecRule<T>[] ToRules(Spec<T>[] specs)
        {
            var rules = new ISpecRule<T>[specs.Length];

            for (var i = 0; i < specs.Length; i++)
                rules[i] = specs[i]._rule;

            return rules;
        }
    }
}

using System;
using Unicore.Monads.Internal;

namespace Unicore.Monads
{
    /// <summary>
    /// Represents specification that evaluates values of type <typeparamref name="T" /> against a rule.
    /// </summary>
    /// <typeparam name="T">Type of value evaluated by the specification.</typeparam>
    public readonly struct Spec<T>
    {
        /// <summary>
        /// Defines contract for rule that determines whether value satisfies a specification.
        /// </summary>
        public interface ISpecRule
        {
            /// <summary>
            /// Determines whether specified value satisfies current rule.
            /// </summary>
            /// <param name="value">A value to evaluate.</param>
            /// <returns><see langword="true" /> if <paramref name="value" /> satisfies current rule; otherwise, <see langword="false" />.</returns>
            bool IsSatisfiedBy(T value);
        }

        private readonly ISpecRule _rule;

        /// <summary>
        /// Creates specification from specified predicate.
        /// </summary>
        /// <param name="predicate">A predicate that evaluates a value.</param>
        /// <returns>A specification that delegates evaluation to <paramref name="predicate" />.</returns>
        public static Spec<T> Create(Func<T, bool> predicate) => new(predicate);

        /// <summary>
        /// Creates specification that is satisfied only when all provided specifications are satisfied.
        /// </summary>
        /// <param name="specs">A set of specifications to combine.</param>
        /// <returns>A specification that requires every specification in <paramref name="specs" /> to be satisfied, or <see cref="AlwaysTrue" /> when no specifications are provided.</returns>
        public static Spec<T> All(params Spec<T>[] specs) => specs.Length == 0
            ? AlwaysTrue()
            : new Spec<T>(new AllOfSpecRule<T>(ToRules(specs)));

        /// <summary>
        /// Creates specification that is satisfied when any provided specification is satisfied.
        /// </summary>
        /// <param name="specs">A set of specifications to combine.</param>
        /// <returns>A specification that requires at least one specification in <paramref name="specs" /> to be satisfied, or <see cref="AlwaysFalse" /> when no specifications are provided.</returns>
        public static Spec<T> Any(params Spec<T>[] specs) => specs.Length == 0
            ? AlwaysFalse()
            : new Spec<T>(new AnyOfSpecRule<T>(ToRules(specs)));

        /// <summary>
        /// Creates specification that negates specified specification.
        /// </summary>
        /// <param name="spec">A specification to negate.</param>
        /// <returns>A specification that is satisfied when <paramref name="spec" /> is not satisfied.</returns>
        public static Spec<T> Not(Spec<T> spec) => spec.Not();

        /// <summary>
        /// Creates specification that is always satisfied.
        /// </summary>
        /// <returns>A specification that accepts every value.</returns>
        public static Spec<T> AlwaysTrue() => new(_ => true);

        /// <summary>
        /// Creates specification that is never satisfied.
        /// </summary>
        /// <returns>A specification that rejects every value.</returns>
        public static Spec<T> AlwaysFalse() => new(_ => false);

        /// <summary>
        /// Initializes a new instance of the <see cref="Spec{T}" /> struct.
        /// </summary>
        /// <param name="predicate">A predicate that evaluates a value.</param>
        public Spec(Func<T, bool> predicate) => _rule = new PredicateSpecRule<T>(predicate);

        /// <summary>
        /// Initializes a new instance of the <see cref="Spec{T}" /> struct.
        /// </summary>
        /// <param name="rule">A rule that evaluates a value.</param>
        public Spec(ISpecRule rule) => _rule = rule;

        /// <summary>
        /// Determines whether specified value satisfies current specification.
        /// </summary>
        /// <param name="value">A value to evaluate.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> satisfies current specification; otherwise, <see langword="false" />.</returns>
        public bool IsSatisfiedBy(T value) => _rule.IsSatisfiedBy(value);

        /// <summary>
        /// Determines whether specified value does not satisfy current specification.
        /// </summary>
        /// <param name="value">A value to evaluate.</param>
        /// <returns><see langword="true" /> if <paramref name="value" /> does not satisfy current specification; otherwise, <see langword="false" />.</returns>
        public bool IsNotSatisfiedBy(T value) => !IsSatisfiedBy(value);

        /// <summary>
        /// Converts specified value into result by validating it against current specification.
        /// </summary>
        /// <param name="value">A value to validate.</param>
        /// <returns>A successful result that contains <paramref name="value" /> when specification is satisfied; otherwise, a failed result with unknown error.</returns>
        public Result<T> ToResult(T value) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure();

        /// <summary>
        /// Converts specified value into result by validating it against current specification and using provided error on failure.
        /// </summary>
        /// <param name="value">A value to validate.</param>
        /// <param name="error">An error that describes validation failure.</param>
        /// <returns>A successful result that contains <paramref name="value" /> when specification is satisfied; otherwise, a failed result that contains <paramref name="error" />.</returns>
        public Result<T> ToResult(T value, Exception error) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(error);

        /// <summary>
        /// Converts specified value into result by validating it against current specification and creating error on failure.
        /// </summary>
        /// <param name="value">A value to validate.</param>
        /// <param name="errorFactory">A factory that produces validation error.</param>
        /// <returns>A successful result that contains <paramref name="value" /> when specification is satisfied; otherwise, a failed result that contains produced error.</returns>
        public Result<T> ToResult(T value, Func<Exception> errorFactory) => _rule.IsSatisfiedBy(value)
            ? Result<T>.Success(value)
            : Result<T>.Failure(errorFactory.Invoke());

        /// <summary>
        /// Ensures that specified value satisfies current specification and returns failed result with provided error otherwise.
        /// </summary>
        /// <param name="value">A value to validate.</param>
        /// <param name="error">An error that describes validation failure.</param>
        /// <returns>A successful result that contains <paramref name="value" /> when specification is satisfied; otherwise, a failed result that contains <paramref name="error" />.</returns>
        public Result<T> Ensure(T value, Exception error) =>
            ToResult(value, error);

        /// <summary>
        /// Ensures that specified value satisfies current specification and returns failed result with error produced on failure.
        /// </summary>
        /// <param name="value">A value to validate.</param>
        /// <param name="errorFactory">A factory that produces validation error.</param>
        /// <returns>A successful result that contains <paramref name="value" /> when specification is satisfied; otherwise, a failed result that contains produced error.</returns>
        public Result<T> Ensure(T value, Func<Exception> errorFactory) =>
            ToResult(value, errorFactory);

        /// <summary>
        /// Gets specified value or fallback value when current specification is not satisfied.
        /// </summary>
        /// <param name="value">A value to evaluate.</param>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns><paramref name="value" /> when current specification is satisfied; otherwise, <paramref name="defaultValue" />.</returns>
        public T GetValueOrDefault(T value, T defaultValue = default) =>
            IsSatisfiedBy(value) ? value : defaultValue;

        /// <summary>
        /// Creates predicate delegate that uses current specification.
        /// </summary>
        /// <returns>A predicate that delegates to <see cref="IsSatisfiedBy" />.</returns>
        public Func<T, bool> AsPredicate() => IsSatisfiedBy;

        /// <summary>
        /// Combines current specification with another specification using logical conjunction.
        /// </summary>
        /// <param name="other">Another specification.</param>
        /// <returns>A specification that is satisfied only when both specifications are satisfied.</returns>
        public Spec<T> And(Spec<T> other) =>
            new(new AllOfSpecRule<T>(_rule, other._rule));

        /// <summary>
        /// Combines current specification with another specification using logical disjunction.
        /// </summary>
        /// <param name="other">Another specification.</param>
        /// <returns>A specification that is satisfied when either specification is satisfied.</returns>
        public Spec<T> Or(Spec<T> other) =>
            new(new AnyOfSpecRule<T>(_rule, other._rule));

        /// <summary>
        /// Creates specification that negates current specification.
        /// </summary>
        /// <returns>A specification that is satisfied when current specification is not satisfied.</returns>
        public Spec<T> Not() =>
            new(new NegatedSpecRule<T>(_rule));

        private static ISpecRule[] ToRules(Spec<T>[] specs)
        {
            ISpecRule[] rules = new ISpecRule[specs.Length];

            for (int i = 0; i < specs.Length; i++)
                rules[i] = specs[i]._rule;

            return rules;
        }
    }
}

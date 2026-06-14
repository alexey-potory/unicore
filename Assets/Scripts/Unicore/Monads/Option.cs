using System;
using Cysharp.Threading.Tasks;
using Unicore.Monads.Internal;

namespace Unicore.Monads
{
    /// <summary>
    /// Represents optional value that is either present as <typeparamref name="T" /> or absent.
    /// </summary>
    /// <typeparam name="T">Type of optional value.</typeparam>
    public abstract class Option<T>
    {
        /// <summary>
        /// Creates option that contains specified value.
        /// </summary>
        /// <param name="value">A value to wrap.</param>
        /// <returns>An option that contains <paramref name="value" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public static Option<T> Some(T value) => new Some<T>(value);

        /// <summary>
        /// Creates empty option.
        /// </summary>
        /// <returns>An option with no value.</returns>
        public static Option<T> None() => new None<T>();

        /// <summary>
        /// Gets a value that indicates whether option contains a value.
        /// </summary>
        /// <value><see langword="true" /> when option contains a value; otherwise, <see langword="false" />.</value>
        public abstract bool IsSome { get; }

        /// <summary>
        /// Gets a value that indicates whether option does not contain a value.
        /// </summary>
        /// <value><see langword="true" /> when option contains no value; otherwise, <see langword="false" />.</value>
        public bool IsNone => !IsSome;

        /// <summary>
        /// Gets contained value.
        /// </summary>
        /// <value>A contained value.</value>
        /// <exception cref="InvalidOperationException">Option has no value.</exception>
        public T Value => IsSome ? GetOrThrow() : throw new InvalidOperationException("Option has no value.");

        /// <summary>
        /// Projects option into another value by selecting one mapping function based on current state.
        /// </summary>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="mapSome">A mapping function for contained value.</param>
        /// <param name="mapNone">A mapping function for empty option.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public abstract TReturn Map<TReturn>(Func<T, TReturn> mapSome, Func<TReturn> mapNone);

        /// <summary>
        /// Projects option into another value by selecting one mapping function based on current state and passing shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="context">A context value passed to selected mapper.</param>
        /// <param name="mapSome">A mapping function for contained value.</param>
        /// <param name="mapNone">A mapping function for empty option.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public abstract TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSome,
            Func<TContext, TReturn> mapNone);

        /// <summary>
        /// Projects option into another value asynchronously by selecting one mapping function based on current state.
        /// </summary>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="mapSome">A mapping function for contained value.</param>
        /// <param name="mapNone">A mapping function for empty option.</param>
        /// <returns>A task that produces mapped value from selected mapper.</returns>
        public abstract UniTask<TReturn> MapAsync<TReturn>(
            Func<T, UniTask<TReturn>> mapSome,
            Func<UniTask<TReturn>> mapNone);

        /// <summary>
        /// Projects option into another value asynchronously by selecting one mapping function based on current state and passing shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="context">A context value passed to selected mapper.</param>
        /// <param name="mapSome">A mapping function for contained value.</param>
        /// <param name="mapNone">A mapping function for empty option.</param>
        /// <returns>A task that produces mapped value from selected mapper.</returns>
        public abstract UniTask<TReturn> MapAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<TReturn>> mapSome,
            Func<TContext, UniTask<TReturn>> mapNone);

        /// <summary>
        /// Executes one action based on current state.
        /// </summary>
        /// <param name="matchSome">An action for contained value.</param>
        /// <param name="matchNone">An action for empty option.</param>
        public abstract void Match(Action<T> matchSome, Action matchNone);

        /// <summary>
        /// Executes one action based on current state and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to selected action.</param>
        /// <param name="matchSome">An action for contained value.</param>
        /// <param name="matchNone">An action for empty option.</param>
        public abstract void Match<TContext>(TContext context, Action<TContext, T> matchSome,
            Action<TContext> matchNone);

        /// <summary>
        /// Executes one asynchronous action based on current state.
        /// </summary>
        /// <param name="matchSome">An action for contained value.</param>
        /// <param name="matchNone">An action for empty option.</param>
        /// <returns>A task that completes when selected action finishes.</returns>
        public abstract UniTask MatchAsync(Func<T, UniTask> matchSome,
            Func<UniTask> matchNone);

        /// <summary>
        /// Executes one asynchronous action based on current state and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to selected action.</param>
        /// <param name="matchSome">An action for contained value.</param>
        /// <param name="matchNone">An action for empty option.</param>
        /// <returns>A task that completes when selected action finishes.</returns>
        public abstract UniTask MatchAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> matchSome,
            Func<TContext, UniTask> matchNone);

        /// <summary>
        /// Chains another option-producing operation that runs only for present value.
        /// </summary>
        /// <typeparam name="TReturn">Type of chained value.</typeparam>
        /// <param name="binder">A function that produces next option from contained value.</param>
        /// <returns>A chained option.</returns>
        public abstract Option<TReturn> Bind<TReturn>(Func<T, Option<TReturn>> binder);

        /// <summary>
        /// Chains another option-producing operation that runs only for present value and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of chained value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="binder" />.</param>
        /// <param name="binder">A function that produces next option from contained value.</param>
        /// <returns>A chained option.</returns>
        public abstract Option<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Option<TReturn>> binder);

        /// <summary>
        /// Chains another asynchronously produced option that runs only for present value.
        /// </summary>
        /// <typeparam name="TReturn">Type of chained value.</typeparam>
        /// <param name="binder">A function that produces next option from contained value.</param>
        /// <returns>A task that produces chained option.</returns>
        public abstract UniTask<Option<TReturn>> BindAsync<TReturn>(Func<T, UniTask<Option<TReturn>>> binder);

        /// <summary>
        /// Chains another asynchronously produced option that runs only for present value and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of chained value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="binder" />.</param>
        /// <param name="binder">A function that produces next option from contained value.</param>
        /// <returns>A task that produces chained option.</returns>
        public abstract UniTask<Option<TReturn>> BindAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<Option<TReturn>>> binder);

        /// <summary>
        /// Executes side effect for present value and returns current option.
        /// </summary>
        /// <param name="tapSome">An action for contained value.</param>
        /// <returns>Current option instance.</returns>
        public abstract Option<T> Tap(Action<T> tapSome);

        /// <summary>
        /// Executes side effect for present value with shared context and returns current option.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tapSome" />.</param>
        /// <param name="tapSome">An action for contained value.</param>
        /// <returns>Current option instance.</returns>
        public abstract Option<T> Tap<TContext>(TContext context, Action<TContext, T> tapSome);

        /// <summary>
        /// Executes asynchronous side effect for present value and returns current option.
        /// </summary>
        /// <param name="tapSome">An action for contained value.</param>
        /// <returns>A task that produces current option instance.</returns>
        public abstract UniTask<Option<T>> TapAsync(Func<T, UniTask> tapSome);

        /// <summary>
        /// Executes asynchronous side effect for present value with shared context and returns current option.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tapSome" />.</param>
        /// <param name="tapSome">An action for contained value.</param>
        /// <returns>A task that produces current option instance.</returns>
        public abstract UniTask<Option<T>> TapAsync<TContext>(TContext context, Func<TContext, T, UniTask> tapSome);

        /// <summary>
        /// Gets contained value or throws when option is empty.
        /// </summary>
        /// <returns>A contained value.</returns>
        /// <exception cref="InvalidOperationException">Option has no value.</exception>
        public abstract T GetOrThrow();

        /// <summary>
        /// Gets contained value or provided default value when option is empty.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>A contained value or <paramref name="defaultValue" />.</returns>
        public abstract T GetOrDefault(T defaultValue);

        /// <summary>
        /// Gets contained value or value produced by fallback factory when option is empty.
        /// </summary>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A contained value or fallback value.</returns>
        public abstract T GetOrDefault(Func<T> defaultValueFactory);

        /// <summary>
        /// Gets contained value or value produced by fallback factory with shared context when option is empty.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A contained value or fallback value.</returns>
        public abstract T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory);

        /// <summary>
        /// Gets contained value or value produced asynchronously by fallback factory when option is empty.
        /// </summary>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A task that produces contained value or fallback value.</returns>
        public abstract UniTask<T> GetOrDefaultAsync(Func<UniTask<T>> defaultValueFactory);

        /// <summary>
        /// Gets contained value or value produced asynchronously by fallback factory with shared context when option is empty.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A task that produces contained value or fallback value.</returns>
        public abstract UniTask<T> GetOrDefaultAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory);

        /// <summary>
        /// Converts empty option into present option with provided fallback value.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>Current present option or a new option that contains <paramref name="defaultValue" />.</returns>
        public abstract Option<T> Ensure(T defaultValue);

        /// <summary>
        /// Converts empty option into present option with value produced by fallback factory.
        /// </summary>
        /// <typeparam name="TContext">Type parameter reserved by current API shape.</typeparam>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>Current present option or a new option that contains produced value.</returns>
        public abstract Option<T> Ensure<TContext>(Func<T> defaultValueFactory);

        /// <summary>
        /// Converts empty option into present option with value produced by fallback factory and shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>Current present option or a new option that contains produced value.</returns>
        public abstract Option<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory);

        /// <summary>
        /// Converts empty option into present option with value produced asynchronously by fallback factory.
        /// </summary>
        /// <typeparam name="TContext">Type parameter reserved by current API shape.</typeparam>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A task that produces current present option or a new option that contains produced value.</returns>
        public abstract UniTask<Option<T>> EnsureAsync<TContext>(Func<UniTask<T>> defaultValueFactory);

        /// <summary>
        /// Converts empty option into present option with value produced asynchronously by fallback factory and shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A task that produces current present option or a new option that contains produced value.</returns>
        public abstract UniTask<Option<T>> EnsureAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory);
    }

    /// <summary>
    /// Provides factory methods for creating <see cref="Option{T}" /> values.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Creates option that contains specified value.
        /// </summary>
        /// <typeparam name="T">Type of wrapped value.</typeparam>
        /// <param name="value">A value to wrap.</param>
        /// <returns>An option that contains <paramref name="value" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public static Option<T> Some<T>(T value) => new Some<T>(value);

        /// <summary>
        /// Creates empty option.
        /// </summary>
        /// <typeparam name="T">Type of value the option may contain.</typeparam>
        /// <returns>An option with no value.</returns>
        public static Option<T> None<T>() => new None<T>();

        /// <summary>
        /// Creates an option from a reference value.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="value">A value to wrap.</param>
        /// <returns>An option that contains <paramref name="value" /> when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> From<T>(T value) where T : class =>
            value == null
                ? None<T>()
                : Some(value);

        /// <summary>
        /// Creates an option from a reference value produced by a factory.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>An option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> From<T>(Func<T> func) where T : class =>
            From(func());

        /// <summary>
        /// Creates an option from a reference value produced by a context-aware factory.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>An option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> From<TContext, T>(TContext context, Func<TContext, T> func) where T : class
        {
            var result = func(context);

            return result == null
                ? None<T>()
                : Some(result);
        }

        /// <summary>
        /// Creates an option from a reference value produced asynchronously by a factory.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces an option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static async UniTask<Option<T>> FromAsync<T>(Func<UniTask<T>> func) where T : class =>
            From(await func());

        /// <summary>
        /// Creates an option from a reference value produced asynchronously by a context-aware factory.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces an option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static async UniTask<Option<T>> FromAsync<TContext, T>(TContext context, Func<TContext, UniTask<T>> func)
            where T : class
        {
            var result = await func(context);

            return result == null
                ? None<T>()
                : Some(result);
        }
    }
}

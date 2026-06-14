using System;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads
{
    /// <summary>
    /// Provides extension methods for converting reference values and factories into <see cref="Option{T}" /> values.
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Converts a reference value into an option.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="value">A value to wrap.</param>
        /// <returns>An option that contains <paramref name="value" /> when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> ToOption<T>(this T value) where T : class =>
            Option.From(value);

        /// <summary>
        /// Converts a reference value produced by a factory into an option.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>An option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> ToOption<T>(this Func<T> func) where T : class =>
            Option.From(func);

        /// <summary>
        /// Converts a reference value produced by a context-aware factory into an option.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>An option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static Option<T> ToOption<TContext, T>(this TContext context, Func<TContext, T> func) where T : class =>
            Option.From(context, func);

        /// <summary>
        /// Converts a reference value produced asynchronously by a factory into an option.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces an option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static UniTask<Option<T>> ToOptionAsync<T>(this Func<UniTask<T>> func) where T : class =>
            Option.FromAsync(func);

        /// <summary>
        /// Converts a reference value produced asynchronously by a context-aware factory into an option.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces an option that contains produced value when it is not <see langword="null" />; otherwise, an empty option.</returns>
        public static UniTask<Option<T>> ToOptionAsync<TContext, T>(this TContext context, Func<TContext, UniTask<T>> func) where T : class =>
            Option.FromAsync(context, func);
    }
}

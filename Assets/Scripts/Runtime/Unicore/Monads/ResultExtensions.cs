using System;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads
{
    /// <summary>
    /// Provides extension methods for converting reference values and factories into <see cref="Result{T}" /> values.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a reference value into a successful result.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="value">A value to wrap.</param>
        /// <returns>A successful result that contains <paramref name="value" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public static Result<T> ToResult<T>(this T value) where T : class =>
            Result.Success(value);

        /// <summary>
        /// Converts a reference value produced by a factory into a result.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A successful result when factory returns a non-<see langword="null" /> value; otherwise, a failed result that contains thrown exception.</returns>
        public static Result<T> ToResult<T>(this Func<T> func) where T : class =>
            Result.Try(func);

        /// <summary>
        /// Converts a reference value produced by a context-aware factory into a result.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A successful result when factory returns a non-<see langword="null" /> value; otherwise, a failed result that contains thrown exception.</returns>
        public static Result<T> ToResult<TContext, T>(this TContext context, Func<TContext, T> func) where T : class =>
            Result.Try<T, TContext>(context, func);

        /// <summary>
        /// Converts a reference value produced asynchronously by a factory into a result.
        /// </summary>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces a successful result when factory returns a non-<see langword="null" /> value; otherwise, a failed result that contains thrown exception.</returns>
        public static UniTask<Result<T>> ToResultAsync<T>(this Func<UniTask<T>> func) where T : class =>
            Result.TryAsync(func);

        /// <summary>
        /// Converts a reference value produced asynchronously by a context-aware factory into a result.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="T">Type of wrapped reference value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A factory that produces a value to wrap.</param>
        /// <returns>A task that produces a successful result when factory returns a non-<see langword="null" /> value; otherwise, a failed result that contains thrown exception.</returns>
        public static UniTask<Result<T>> ToResultAsync<TContext, T>(this TContext context, Func<TContext, UniTask<T>> func) where T : class =>
            Result.TryAsync<T, TContext>(context, func);
    }
}

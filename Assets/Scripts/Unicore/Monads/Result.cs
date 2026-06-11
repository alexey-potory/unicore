using System;
using Unicore.Exceptions;
using Unicore.Monads.Internal;

namespace Unicore.Monads
{
    /// <summary>
    /// Represents result of operation that either succeeds with value of type <typeparamref name="T" /> or fails with an exception.
    /// </summary>
    /// <typeparam name="T">Type of successful value.</typeparam>
    public abstract class Result<T>
    {
        /// <summary>
        /// Creates successful result that carries <see cref="Unit.Value" />.
        /// </summary>
        /// <returns>A successful result with no payload beyond <see cref="Unit" />.</returns>
        public static Result<Unit> Success() => new Success<Unit>(Unit.Value);

        /// <summary>
        /// Creates successful result that carries specified value.
        /// </summary>
        /// <param name="value">A successful value.</param>
        /// <returns>A successful result that contains <paramref name="value" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public static Result<T> Success(T value) => new Success<T>(value);

        /// <summary>
        /// Creates failed result with unknown error.
        /// </summary>
        /// <returns>A failed result that contains an internally generated exception.</returns>
        public static Result<T> Failure() => new Failure<T>(new UnknownException());

        /// <summary>
        /// Creates failed result that carries specified exception.
        /// </summary>
        /// <param name="error">An error that describes failure.</param>
        /// <returns>A failed result that contains <paramref name="error" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="error" /> is <see langword="null" />.</exception>
        public static Result<T> Failure(Exception error) => new Failure<T>(error);

        /// <summary>
        /// Gets a value that indicates whether result represents success.
        /// </summary>
        /// <value><see langword="true" /> if result is successful; otherwise, <see langword="false" />.</value>
        public abstract bool IsSuccess { get; }

        /// <summary>
        /// Gets a value that indicates whether result represents failure.
        /// </summary>
        /// <value><see langword="true" /> if result is failed; otherwise, <see langword="false" />.</value>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets successful value.
        /// </summary>
        /// <value>A successful value.</value>
        /// <exception cref="InvalidOperationException">Result is not successful.</exception>
        public T Value => IsSuccess ? GetOrThrow() : throw new InvalidOperationException("Result is not successful.");

        /// <summary>
        /// Projects result into another value by selecting one mapping function based on current state.
        /// </summary>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="mapSuccess">A mapping function for successful value.</param>
        /// <param name="mapFailure">A mapping function for failure exception.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public abstract TReturn Map<TReturn>(Func<T, TReturn> mapSuccess, Func<Exception, TReturn> mapFailure);

        /// <summary>
        /// Projects result into another value by selecting one mapping function based on current state and passing shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="context">A context value passed to selected mapper.</param>
        /// <param name="mapSuccess">A mapping function for successful value.</param>
        /// <param name="mapFailure">A mapping function for failure exception.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public abstract TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSuccess,
            Func<TContext, Exception, TReturn> mapFailure);

        /// <summary>
        /// Executes one action based on current state.
        /// </summary>
        /// <param name="matchSuccess">An action for successful value.</param>
        /// <param name="matchFailure">An action for failure exception.</param>
        public abstract void Match(Action<T> matchSuccess, Action<Exception> matchFailure);

        /// <summary>
        /// Executes one action based on current state and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to selected action.</param>
        /// <param name="matchSuccess">An action for successful value.</param>
        /// <param name="matchFailure">An action for failure exception.</param>
        public abstract void Match<TContext>(TContext context, Action<TContext, T> matchSuccess,
            Action<TContext, Exception> matchFailure);

        /// <summary>
        /// Chains another result-producing operation that runs only for successful result.
        /// </summary>
        /// <typeparam name="TReturn">Type of chained successful value.</typeparam>
        /// <param name="binder">A function that produces next result from successful value.</param>
        /// <returns>A chained result.</returns>
        public abstract Result<TReturn> Bind<TReturn>(Func<T, Result<TReturn>> binder);

        /// <summary>
        /// Chains another result-producing operation that runs only for successful result and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of chained successful value.</typeparam>
        /// <param name="context">A context value passed to <paramref name="binder" />.</param>
        /// <param name="binder">A function that produces next result from successful value.</param>
        /// <returns>A chained result.</returns>
        public abstract Result<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Result<TReturn>> binder);

        /// <summary>
        /// Executes side effect for successful result and returns current result.
        /// </summary>
        /// <param name="tapSuccess">An action for successful value.</param>
        /// <returns>Current result instance.</returns>
        public abstract Result<T> Tap(Action<T> tapSuccess);

        /// <summary>
        /// Executes side effect for successful result with shared context and returns current result.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tapSuccess" />.</param>
        /// <param name="tapSuccess">An action for successful value.</param>
        /// <returns>Current result instance.</returns>
        public abstract Result<T> Tap<TContext>(TContext context, Action<TContext, T> tapSuccess);

        /// <summary>
        /// Executes side effect for failed result and returns current result.
        /// </summary>
        /// <param name="tapError">An action for failure exception.</param>
        /// <returns>Current result instance.</returns>
        public abstract Result<T> TapError(Action<Exception> tapError);

        /// <summary>
        /// Executes side effect for failed result with shared context and returns current result.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tapError" />.</param>
        /// <param name="tapError">An action for failure exception.</param>
        /// <returns>Current result instance.</returns>
        public abstract Result<T> TapError<TContext>(TContext context, Action<TContext, Exception> tapError);

        /// <summary>
        /// Logs failure exception when result is failed.
        /// </summary>
        /// <returns>Current result instance.</returns>
        public abstract Result<T> LogIfError();

        /// <summary>
        /// Throws failure exception when result is failed.
        /// </summary>
        /// <returns>Current result instance when result is successful.</returns>
        /// <exception cref="Exception">Result contains failure exception.</exception>
        public abstract Result<T> ThrowIfError();

        /// <summary>
        /// Gets successful value or throws stored exception.
        /// </summary>
        /// <returns>A successful value.</returns>
        /// <exception cref="Exception">Result contains failure exception.</exception>
        public abstract T GetOrThrow();

        /// <summary>
        /// Gets successful value or provided default value when result is failed.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>A successful value or <paramref name="defaultValue" />.</returns>
        public abstract T GetOrDefault(T defaultValue);

        /// <summary>
        /// Gets successful value or value produced by fallback factory when result is failed.
        /// </summary>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A successful value or fallback value.</returns>
        public abstract T GetOrDefault(Func<T> defaultValueFactory);

        /// <summary>
        /// Gets successful value or value produced by fallback factory with shared context when result is failed.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>A successful value or fallback value.</returns>
        public abstract T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory);

        /// <summary>
        /// Converts failed result into successful result with provided fallback value.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>Current successful result or a new successful fallback result.</returns>
        public abstract Result<T> Ensure(T defaultValue);

        /// <summary>
        /// Converts failed result into successful result with value produced by fallback factory.
        /// </summary>
        /// <typeparam name="TContext">Type parameter reserved by current API shape.</typeparam>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>Current successful result or a new successful fallback result.</returns>
        public abstract Result<T> Ensure<TContext>(Func<T> defaultValueFactory);

        /// <summary>
        /// Converts failed result into successful result with value produced by fallback factory and shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="defaultValueFactory" />.</param>
        /// <param name="defaultValueFactory">A factory that produces fallback value.</param>
        /// <returns>Current successful result or a new successful fallback result.</returns>
        public abstract Result<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory);
    }

    /// <summary>
    /// Provides factory and helper methods for creating and executing <see cref="Result{T}" /> values.
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Creates successful result that carries <see cref="Unit.Value" />.
        /// </summary>
        /// <returns>A successful result with no payload beyond <see cref="Unit" />.</returns>
        public static Result<Unit> Success() => new Success<Unit>(Unit.Value);

        /// <summary>
        /// Creates successful result that carries specified value.
        /// </summary>
        /// <typeparam name="T">Type of successful value.</typeparam>
        /// <param name="value">A successful value.</param>
        /// <returns>A successful result that contains <paramref name="value" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
        public static Result<T> Success<T>(T value) => new Success<T>(value);

        /// <summary>
        /// Creates failed result with unknown error.
        /// </summary>
        /// <typeparam name="T">Type of successful value the result would otherwise contain.</typeparam>
        /// <returns>A failed result that contains an internally generated exception.</returns>
        public static Result<T> Failure<T>() => new Failure<T>(new UnknownException());

        /// <summary>
        /// Creates failed result that carries specified exception.
        /// </summary>
        /// <typeparam name="T">Type of successful value the result would otherwise contain.</typeparam>
        /// <param name="error">An error that describes failure.</param>
        /// <returns>A failed result that contains <paramref name="error" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="error" /> is <see langword="null" />.</exception>
        public static Result<T> Failure<T>(Exception error) => new Failure<T>(error);

        /// <summary>
        /// Executes action and captures its outcome as a result.
        /// </summary>
        /// <param name="action">An action to execute.</param>
        /// <returns>A successful unit result when <paramref name="action" /> completes; otherwise, a failed result with thrown exception.</returns>
        public static Result<Unit> Try(Action action)
        {
            try
            {
                action();
                return Result<Unit>.Success();
            }
            catch (Exception e)
            {
                return Result<Unit>.Failure(e);
            }
        }

        /// <summary>
        /// Executes action with shared context and captures its outcome as a result.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="action" />.</param>
        /// <param name="action">An action to execute.</param>
        /// <returns>A successful unit result when <paramref name="action" /> completes; otherwise, a failed result with thrown exception.</returns>
        public static Result<Unit> Try<TContext>(TContext context, Action<TContext> action)
        {
            try
            {
                action(context);
                return Result<Unit>.Success();
            }
            catch (Exception e)
            {
                return Result<Unit>.Failure(e);
            }
        }

        /// <summary>
        /// Executes function and captures its outcome as a result.
        /// </summary>
        /// <typeparam name="T">Type of successful value.</typeparam>
        /// <param name="func">A function to execute.</param>
        /// <returns>A successful result with returned value when <paramref name="func" /> completes; otherwise, a failed result with thrown exception.</returns>
        public static Result<T> Try<T>(Func<T> func)
        {
            try
            {
                return Result<T>.Success(func.Invoke());
            }
            catch (Exception e)
            {
                return Result<T>.Failure(e);
            }
        }

        /// <summary>
        /// Executes function with shared context and captures its outcome as a result.
        /// </summary>
        /// <typeparam name="T">Type of successful value.</typeparam>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="func" />.</param>
        /// <param name="func">A function to execute.</param>
        /// <returns>A successful result with returned value when <paramref name="func" /> completes; otherwise, a failed result with thrown exception.</returns>
        public static Result<T> Try<T, TContext>(TContext context, Func<TContext, T> func)
        {
            try
            {
                return Result<T>.Success(func.Invoke(context));
            }
            catch (Exception e)
            {
                return Result<T>.Failure(e);
            }
        }
    }
}

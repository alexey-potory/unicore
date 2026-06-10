using System;
using Unicore.Exceptions;
using Unicore.Monads.Internal;

namespace Unicore.Monads
{
    public abstract class Result<T>
    {
        public static Result<Unit> Success() => new Success<Unit>(Unit.Value);
        public static Result<T> Success(T value) => new Success<T>(value);

        public static Result<T> Failure() => new Failure<T>(new UnknownException());
        public static Result<T> Failure(Exception error) => new Failure<T>(error);

        public abstract bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public T Value => IsSuccess ? GetOrThrow() : throw new InvalidOperationException("Result is not successful.");

        public abstract TReturn Map<TReturn>(Func<T, TReturn> mapSuccess, Func<Exception, TReturn> mapFailure);

        public abstract TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSuccess,
            Func<TContext, Exception, TReturn> mapFailure);

        public abstract void Match(Action<T> matchSuccess, Action<Exception> matchFailure);

        public abstract void Match<TContext>(TContext context, Action<TContext, T> matchSuccess,
            Action<TContext, Exception> matchFailure);

        public abstract Result<TReturn> Bind<TReturn>(Func<T, Result<TReturn>> binder);

        public abstract Result<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Result<TReturn>> binder);

        public abstract Result<T> Tap(Action<T> tapSuccess);
        public abstract Result<T> Tap<TContext>(TContext context, Action<TContext, T> tapSuccess);

        public abstract Result<T> TapError(Action<Exception> tapError);
        public abstract Result<T> TapError<TContext>(TContext context, Action<TContext, Exception> tapError);

        public abstract Result<T> LogIfError();
        public abstract Result<T> ThrowIfError();

        public abstract T GetOrThrow();

        public abstract T GetOrDefault(T defaultValue);
        public abstract T GetOrDefault(Func<T> defaultValueFactory);
        public abstract T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory);

        public abstract Result<T> Ensure(T defaultValue);
        public abstract Result<T> Ensure<TContext>(Func<T> defaultValueFactory);
        public abstract Result<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory);
    }

    public static class Result
    {
        public static Result<Unit> Success() => new Success<Unit>(Unit.Value);
        public static Result<T> Success<T>(T value) => new Success<T>(value);

        public static Result<T> Failure<T>() => new Failure<T>(new UnknownException());
        public static Result<T> Failure<T>(Exception error) => new Failure<T>(error);

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
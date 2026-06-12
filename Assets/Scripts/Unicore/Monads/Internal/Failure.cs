using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Unicore.Monads.Internal
{
    internal sealed class Failure<T> : Result<T>
    {
        private readonly Exception _error;
        public override bool IsSuccess => false;

        public Failure(Exception error) => _error = error ?? throw new ArgumentNullException(nameof(error));

        public override TReturn Map<TReturn>(Func<T, TReturn> mapSuccess, Func<Exception, TReturn> mapFailure) =>
            mapFailure(_error);

        public override TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSuccess,
            Func<TContext, Exception, TReturn> mapFailure) =>
            mapFailure(context, _error);

        public override UniTask<TReturn> MapAsync<TReturn>(
            Func<T, UniTask<TReturn>> mapSuccess,
            Func<Exception, UniTask<TReturn>> mapFailure) => mapFailure(_error);

        public override UniTask<TReturn> MapAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<TReturn>> mapSuccess,
            Func<TContext, Exception, UniTask<TReturn>> mapFailure) =>
            mapFailure(context, _error);

        public override void Match(Action<T> matchSuccess, Action<Exception> matchFailure) => matchFailure(_error);

        public override void Match<TContext>(TContext context, Action<TContext, T> matchSuccess,
            Action<TContext, Exception> matchFailure) => matchFailure(context, _error);

        public override UniTask MatchAsync(
            Func<T, UniTask> matchSuccess,
            Func<Exception, UniTask> matchFailure) => matchFailure(_error);

        public override UniTask MatchAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> matchSuccess,
            Func<TContext, Exception, UniTask> matchFailure) => matchFailure(context, _error);

        public override Result<TReturn> Bind<TReturn>(Func<T, Result<TReturn>> binder) =>
            Result<TReturn>.Failure(_error);

        public override Result<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Result<TReturn>> binder) =>
            Result<TReturn>.Failure(_error);

        public override UniTask<Result<TReturn>> BindAsync<TReturn>(Func<T,
            UniTask<Result<TReturn>>> binder) => UniTask.FromResult(Result<TReturn>.Failure(_error));

        public override UniTask<Result<TReturn>> BindAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<Result<TReturn>>> binder) => UniTask.FromResult(Result<TReturn>.Failure(_error));

        public override Result<T> Tap(Action<T> tapSuccess) => this;
        public override Result<T> Tap<TContext>(TContext context, Action<TContext, T> tapSuccess) => this;

        public override UniTask<Result<T>> TapAsync(Func<T,
            UniTask> tapSuccess) => UniTask.FromResult<Result<T>>(this);

        public override UniTask<Result<T>> TapAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> tapSuccess) => UniTask.FromResult<Result<T>>(this);

        public override Result<T> TapError(Action<Exception> tapError)
        {
            tapError(_error);
            return this;
        }

        public override Result<T> TapError<TContext>(TContext context, Action<TContext, Exception> tapError)
        {
            tapError(context, _error);
            return this;
        }

        public override async UniTask<Result<T>> TapErrorAsync(Func<Exception, UniTask> tapError)
        {
            await tapError(_error);
            return this;
        }

        public override async UniTask<Result<T>> TapErrorAsync<TContext>(TContext context,
            Func<TContext, Exception, UniTask> tapError)
        {
            await tapError(context, _error);
            return this;
        }

        public override Result<T> LogIfError()
        {
#if UNITY_EDITOR
            Debug.LogException(_error);
#endif
            return this;
        }

        public override Result<T> ThrowIfError() => throw _error;
        public override T GetOrThrow() => throw _error;

        public override T GetOrDefault(T defaultValue) => defaultValue;
        public override T GetOrDefault(Func<T> defaultValueFactory) => defaultValueFactory();

        public override T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory) =>
            defaultValueFactory(context);

        public override UniTask<T> GetOrDefaultAsync(
            Func<UniTask<T>> defaultValueFactory) => defaultValueFactory();

        public override UniTask<T> GetOrDefaultAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => defaultValueFactory(context);

        public override Result<T> Ensure(T defaultValue) => Success(defaultValue);
        public override Result<T> Ensure<TContext>(Func<T> defaultValueFactory) => Success(defaultValueFactory());

        public override Result<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory) =>
            Success(defaultValueFactory(context));

        public override async UniTask<Result<T>> EnsureAsync<TContext>(
            Func<UniTask<T>> defaultValueFactory) => Success(await defaultValueFactory());

        public override async UniTask<Result<T>> EnsureAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => Success(await defaultValueFactory(context));
    }
}
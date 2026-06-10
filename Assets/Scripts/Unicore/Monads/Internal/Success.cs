using System;

namespace Unicore.Monads.Internal
{
    internal sealed class Success<T> : Result<T>
    {
        private readonly T _value;
        public override bool IsSuccess => true;

        public Success(T value) => _value = value ?? throw new ArgumentNullException(nameof(value));

        public override TReturn Map<TReturn>(Func<T, TReturn> mapSuccess, Func<Exception, TReturn> mapFailure) =>
            mapSuccess(_value);

        public override TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSuccess,
            Func<TContext, Exception, TReturn> mapFailure) =>
            mapSuccess(context, _value);

        public override void Match(Action<T> matchSuccess, Action<Exception> matchFailure) => matchSuccess(_value);

        public override void Match<TContext>(TContext context, Action<TContext, T> matchSuccess,
            Action<TContext, Exception> matchFailure) => matchSuccess(context, _value);

        public override Result<TReturn> Bind<TReturn>(Func<T, Result<TReturn>> binder) =>
            binder(_value);

        public override Result<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Result<TReturn>> binder) =>
            binder(context, _value);

        public override Result<T> Tap(Action<T> tapSuccess)
        {
            tapSuccess(_value);
            return this;
        }

        public override Result<T> Tap<TContext>(TContext context, Action<TContext, T> tapSuccess)
        {
            tapSuccess(context, _value);
            return this;
        }

        public override Result<T> TapError(Action<Exception> tapError) => this;
        public override Result<T> TapError<TContext>(TContext context, Action<TContext, Exception> tapError) => this;

        public override Result<T> LogIfError() => this;
        public override Result<T> ThrowIfError() => this;

        public override T GetOrThrow() => _value;

        public override T GetOrDefault(T defaultValue) => _value;
        public override T GetOrDefault(Func<T> defaultValueFactory) => _value;
        public override T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory) => _value;

        public override Result<T> Ensure(T defaultValue) => this;
        public override Result<T> Ensure<TContext>(Func<T> defaultValueFactory) => this;
        public override Result<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory) => this;
    }
}
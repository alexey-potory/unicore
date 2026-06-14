using System;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads.Internal
{
    internal sealed class Some<T> : Option<T>
    {
        private readonly T _value;
        public override bool IsSome => true;

        public Some(T value) => _value = value ?? throw new ArgumentNullException(nameof(value));

        public override TReturn Map<TReturn>(Func<T, TReturn> mapSome, Func<TReturn> mapNone) =>
            mapSome(_value);

        public override TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSome,
            Func<TContext, TReturn> mapNone) =>
            mapSome(context, _value);

        public override UniTask<TReturn> MapAsync<TReturn>(
            Func<T, UniTask<TReturn>> mapSome,
            Func<UniTask<TReturn>> mapNone) => mapSome(_value);

        public override UniTask<TReturn> MapAsync<TContext, TReturn>(
            TContext context, Func<TContext, T, UniTask<TReturn>> mapSome,
            Func<TContext, UniTask<TReturn>> mapNone) => mapSome(context, _value);

        public override void Match(Action<T> matchSome, Action matchNone) => matchSome(_value);

        public override void Match<TContext>(TContext context, Action<TContext, T> matchSome,
            Action<TContext> matchNone) => matchSome(context, _value);

        public override UniTask MatchAsync(Func<T, UniTask> matchSome,
            Func<UniTask> matchNone) => matchSome(_value);

        public override UniTask MatchAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> matchSome,
            Func<TContext, UniTask> matchNone) => matchSome(context, _value);

        public override Option<TReturn> Bind<TReturn>(Func<T, Option<TReturn>> binder) =>
            binder(_value);

        public override Option<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Option<TReturn>> binder) =>
            binder(context, _value);

        public override UniTask<Option<TReturn>> BindAsync<TReturn>(
            Func<T, UniTask<Option<TReturn>>> binder) => binder(_value);

        public override UniTask<Option<TReturn>> BindAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<Option<TReturn>>> binder) => binder(context, _value);

        public override Option<T> Tap(Action<T> tapSome)
        {
            tapSome(_value);
            return this;
        }

        public override Option<T> Tap<TContext>(TContext context, Action<TContext, T> tapSome)
        {
            tapSome(context, _value);
            return this;
        }

        public override async UniTask<Option<T>> TapAsync(Func<T, UniTask> tapSome)
        {
            await tapSome(_value);
            return this;
        }

        public override async UniTask<Option<T>> TapAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> tapSome)
        {
            await tapSome(context, _value);
            return this;
        }

        public override T GetOrThrow() => _value;
        public override T GetOrDefault(T defaultValue) => _value;
        public override T GetOrDefault(Func<T> defaultValueFactory) => _value;
        public override T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory) => _value;

        public override UniTask<T> GetOrDefaultAsync(Func<UniTask<T>> defaultValueFactory) =>
            UniTask.FromResult(_value);

        public override UniTask<T> GetOrDefaultAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => UniTask.FromResult(_value);

        public override Option<T> Ensure(T defaultValue) => this;
        public override Option<T> Ensure<TContext>(Func<T> defaultValueFactory) => this;
        public override Option<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory) => this;

        public override UniTask<Option<T>> EnsureAsync<TContext>(Func<UniTask<T>> defaultValueFactory) =>
            UniTask.FromResult<Option<T>>(this);

        public override UniTask<Option<T>> EnsureAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => UniTask.FromResult<Option<T>>(this);
    }
}

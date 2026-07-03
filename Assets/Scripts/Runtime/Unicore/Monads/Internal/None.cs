using System;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads.Internal
{
    internal sealed class None<T> : Option<T>
    {
        public override bool IsSome => false;

        public override TReturn Map<TReturn>(Func<T, TReturn> mapSome, Func<TReturn> mapNone) =>
            mapNone();

        public override TReturn Map<TContext, TReturn>(TContext context, Func<TContext, T, TReturn> mapSome,
            Func<TContext, TReturn> mapNone) =>
            mapNone(context);

        public override UniTask<TReturn> MapAsync<TReturn>(
            Func<T, UniTask<TReturn>> mapSome,
            Func<UniTask<TReturn>> mapNone) => mapNone();

        public override UniTask<TReturn> MapAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<TReturn>> mapSome,
            Func<TContext, UniTask<TReturn>> mapNone) =>
            mapNone(context);

        public override void Match(Action<T> matchSome, Action matchNone) => matchNone();

        public override void Match<TContext>(TContext context, Action<TContext, T> matchSome,
            Action<TContext> matchNone) => matchNone(context);

        public override UniTask MatchAsync(
            Func<T, UniTask> matchSome,
            Func<UniTask> matchNone) => matchNone();

        public override UniTask MatchAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> matchSome,
            Func<TContext, UniTask> matchNone) => matchNone(context);

        public override Option<TReturn> Bind<TReturn>(Func<T, Option<TReturn>> binder) =>
            Option<TReturn>.None();

        public override Option<TReturn> Bind<TContext, TReturn>(TContext context,
            Func<TContext, T, Option<TReturn>> binder) =>
            Option<TReturn>.None();

        public override UniTask<Option<TReturn>> BindAsync<TReturn>(Func<T,
            UniTask<Option<TReturn>>> binder) => UniTask.FromResult(Option<TReturn>.None());

        public override UniTask<Option<TReturn>> BindAsync<TContext, TReturn>(TContext context,
            Func<TContext, T, UniTask<Option<TReturn>>> binder) => UniTask.FromResult(Option<TReturn>.None());

        public override Option<T> Tap(Action<T> tapSome) => this;
        public override Option<T> Tap<TContext>(TContext context, Action<TContext, T> tapSome) => this;

        public override UniTask<Option<T>> TapAsync(Func<T,
            UniTask> tapSome) => UniTask.FromResult<Option<T>>(this);

        public override UniTask<Option<T>> TapAsync<TContext>(TContext context,
            Func<TContext, T, UniTask> tapSome) => UniTask.FromResult<Option<T>>(this);

        public override T GetOrThrow() => throw new InvalidOperationException("Option has no value.");
        public override T GetOrDefault(T defaultValue) => defaultValue;
        public override T GetOrDefault(Func<T> defaultValueFactory) => defaultValueFactory();

        public override T GetOrDefault<TContext>(TContext context, Func<TContext, T> defaultValueFactory) =>
            defaultValueFactory(context);

        public override UniTask<T> GetOrDefaultAsync(
            Func<UniTask<T>> defaultValueFactory) => defaultValueFactory();

        public override UniTask<T> GetOrDefaultAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => defaultValueFactory(context);

        public override Option<T> Ensure(T defaultValue) => Some(defaultValue);
        public override Option<T> Ensure<TContext>(Func<T> defaultValueFactory) => Some(defaultValueFactory());

        public override Option<T> Ensure<TContext>(TContext context, Func<TContext, T> defaultValueFactory) =>
            Some(defaultValueFactory(context));

        public override async UniTask<Option<T>> EnsureAsync<TContext>(
            Func<UniTask<T>> defaultValueFactory) => Some(await defaultValueFactory());

        public override async UniTask<Option<T>> EnsureAsync<TContext>(TContext context,
            Func<TContext, UniTask<T>> defaultValueFactory) => Some(await defaultValueFactory(context));
    }
}

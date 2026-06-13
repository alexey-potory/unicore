using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads
{
    public readonly struct OneOf<T0, T1> : IEquatable<OneOf<T0, T1>>
    {
        private readonly object _value;
        private readonly byte _index;

        public T0 AsT0 => _index == 0 ? (T0)_value : throw new InvalidOperationException();
        public T1 AsT1 => _index == 1 ? (T1)_value : throw new InvalidOperationException();

        public bool IsT0 => _index == 0;
        public bool IsT1 => _index == 1;

        public OneOf(T0 value)
        {
            _value = value;
            _index = 0;
        }

        public OneOf(T1 value)
        {
            _value = value;
            _index = 1;
        }

        public TReturn Map<TReturn>(
            Func<T0, TReturn> map0,
            Func<T1, TReturn> map1)
        {
            return _index switch
            {
                0 => map0(AsT0),
                1 => map1(AsT1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public TReturn Map<TContext, TReturn>(TContext context,
            Func<TContext, T0, TReturn> map0,
            Func<TContext, T1, TReturn> map1)
        {
            return _index switch
            {
                0 => map0(context, AsT0),
                1 => map1(context, AsT1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public UniTask<TReturn> MapAsync<TReturn>(
            Func<T0, UniTask<TReturn>> map0,
            Func<T1, UniTask<TReturn>> map1)
        {
            return _index switch
            {
                0 => map0(AsT0),
                1 => map1(AsT1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public UniTask<TReturn> MapAsync<TContext, TReturn>(TContext context,
            Func<TContext, T0, UniTask<TReturn>> map0,
            Func<TContext, T1, UniTask<TReturn>> map1)
        {
            return _index switch
            {
                0 => map0(context, AsT0),
                1 => map1(context, AsT1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool TryGetT0(out T0 value)
        {
            if (IsT0)
            {
                value = AsT0;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetT1(out T1 value)
        {
            if (IsT1)
            {
                value = AsT1;
                return true;
            }

            value = default;
            return false;
        }

        public T0 GetT0OrDefault(T0 defaultValue = default) => IsT0 ? AsT0 : defaultValue;

        public T1 GetT1OrDefault(T1 defaultValue = default) => IsT1 ? AsT1 : defaultValue;

        public OneOf<TOut0, T1> MapT0<TOut0>(Func<T0, TOut0> map)
        {
            return IsT0 ? new OneOf<TOut0, T1>(map(AsT0)) : new OneOf<TOut0, T1>(AsT1);
        }

        public OneOf<T0, TOut1> MapT1<TOut1>(Func<T1, TOut1> map)
        {
            return IsT1 ? new OneOf<T0, TOut1>(map(AsT1)) : new OneOf<T0, TOut1>(AsT0);
        }

        public async UniTask<OneOf<TOut0, T1>> MapT0Async<TOut0>(Func<T0, UniTask<TOut0>> map)
        {
            return IsT0 ? new OneOf<TOut0, T1>(await map(AsT0)) : new OneOf<TOut0, T1>(AsT1);
        }

        public async UniTask<OneOf<T0, TOut1>> MapT1Async<TOut1>(Func<T1, UniTask<TOut1>> map)
        {
            return IsT1 ? new OneOf<T0, TOut1>(await map(AsT1)) : new OneOf<T0, TOut1>(AsT0);
        }

        public void Switch(Action<T0> map0, Action<T1> map1)
        {
            if (IsT0)
            {
                map0(AsT0);
                return;
            }

            if (IsT1)
            {
                map1(AsT1);
                return;
            }

            throw new ArgumentOutOfRangeException();
        }

        public void Switch<TContext>(TContext context,
            Action<TContext, T0> map0, Action<TContext, T1> map1)
        {
            if (IsT0)
            {
                map0(context, AsT0);
                return;
            }

            if (IsT1)
            {
                map1(context, AsT1);
                return;
            }

            throw new ArgumentOutOfRangeException();
        }

        public UniTask SwitchAsync(Func<T0, UniTask> map0, Func<T1, UniTask> map1)
        {
            if (IsT0)
                return map0(AsT0);

            if (IsT1)
                return map1(AsT1);

            throw new ArgumentOutOfRangeException();
        }

        public UniTask SwitchAsync<TContext>(TContext context,
            Func<TContext, T0, UniTask> map0,
            Func<TContext, T1, UniTask> map1)
        {
            if (IsT0)
                return map0(context, AsT0);

            if (IsT1)
                return map1(context, AsT1);

            throw new ArgumentOutOfRangeException();
        }

        public OneOf<T0, T1> TapT0(Action<T0> tap)
        {
            if (IsT0)
                tap(AsT0);

            return this;
        }

        public OneOf<T0, T1> TapT0<TContext>(TContext context, Action<TContext, T0> tap)
        {
            if (IsT0)
                tap(context, AsT0);

            return this;
        }

        public OneOf<T0, T1> TapT1(Action<T1> tap)
        {
            if (IsT1)
                tap(AsT1);

            return this;
        }

        public OneOf<T0, T1> TapT1<TContext>(TContext context, Action<TContext, T1> tap)
        {
            if (IsT1)
                tap(context, AsT1);

            return this;
        }

        public async UniTask<OneOf<T0, T1>> TapT0Async(Func<T0, UniTask> tap)
        {
            if (IsT0)
                await tap(AsT0);

            return this;
        }

        public async UniTask<OneOf<T0, T1>> TapT0Async<TContext>(TContext context, Func<TContext, T0, UniTask> tap)
        {
            if (IsT0)
                await tap(context, AsT0);

            return this;
        }

        public async UniTask<OneOf<T0, T1>> TapT1Async(Func<T1, UniTask> tap)
        {
            if (IsT1)
                await tap(AsT1);

            return this;
        }

        public async UniTask<OneOf<T0, T1>> TapT1Async<TContext>(TContext context, Func<TContext, T1, UniTask> tap)
        {
            if (IsT1)
                await tap(context, AsT1);

            return this;
        }

        public OneOf<TOut0, T1> BindT0<TOut0>(Func<T0, OneOf<TOut0, T1>> binder)
        {
            return IsT0 ? binder(AsT0) : new OneOf<TOut0, T1>(AsT1);
        }

        public OneOf<TOut0, T1> BindT0<TContext, TOut0>(TContext context, Func<TContext, T0, OneOf<TOut0, T1>> binder)
        {
            return IsT0 ? binder(context, AsT0) : new OneOf<TOut0, T1>(AsT1);
        }

        public OneOf<T0, TOut1> BindT1<TOut1>(Func<T1, OneOf<T0, TOut1>> binder)
        {
            return IsT1 ? binder(AsT1) : new OneOf<T0, TOut1>(AsT0);
        }

        public OneOf<T0, TOut1> BindT1<TContext, TOut1>(TContext context, Func<TContext, T1, OneOf<T0, TOut1>> binder)
        {
            return IsT1 ? binder(context, AsT1) : new OneOf<T0, TOut1>(AsT0);
        }

        public bool Equals(OneOf<T0, T1> other)
        {
            if (_index != other._index)
                return false;

            return _index switch
            {
                0 => EqualityComparer<T0>.Default.Equals(AsT0, other.AsT0),
                1 => EqualityComparer<T1>.Default.Equals(AsT1, other.AsT1),
                _ => false
            };
        }

        public override bool Equals(object obj) => obj is OneOf<T0, T1> other && Equals(other);

        public override int GetHashCode()
        {
            return _index switch
            {
                0 => HashCode.Combine(_index, EqualityComparer<T0>.Default.GetHashCode(AsT0)),
                1 => HashCode.Combine(_index, EqualityComparer<T1>.Default.GetHashCode(AsT1)),
                _ => _index.GetHashCode()
            };
        }

        public override string ToString()
        {
            return _index switch
            {
                0 => $"T0({AsT0})",
                1 => $"T1({AsT1})",
                _ => $"{nameof(OneOf<T0, T1>)}(Invalid)"
            };
        }

        public static bool operator ==(OneOf<T0, T1> left, OneOf<T0, T1> right) => left.Equals(right);

        public static bool operator !=(OneOf<T0, T1> left, OneOf<T0, T1> right) => !left.Equals(right);
    }
}

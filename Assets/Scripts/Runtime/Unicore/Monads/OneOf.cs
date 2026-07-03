using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Unicore.Monads
{
    /// <summary>
    /// Represents discriminated union that contains either value of type <typeparamref name="T0" /> or value of type <typeparamref name="T1" />.
    /// </summary>
    /// <typeparam name="T0">Type of first union branch.</typeparam>
    /// <typeparam name="T1">Type of second union branch.</typeparam>
    public readonly struct OneOf<T0, T1> : IEquatable<OneOf<T0, T1>>
    {
        private readonly object _value;
        private readonly byte _index;
        private readonly bool _hasValue;

        /// <summary>
        /// Gets first branch value.
        /// </summary>
        /// <value>A value stored in first branch.</value>
        /// <exception cref="InvalidOperationException">Current value is not stored in first branch.</exception>
        public T0 AsT0 => _hasValue
            ? _index == 0
                ? (T0)_value
                : throw CreateWrongBranchException("T0")
            : throw CreateInvalidStateException();

        /// <summary>
        /// Gets second branch value.
        /// </summary>
        /// <value>A value stored in second branch.</value>
        /// <exception cref="InvalidOperationException">Current value is not stored in second branch.</exception>
        public T1 AsT1 => _hasValue
            ? _index == 1
                ? (T1)_value
                : throw CreateWrongBranchException("T1")
            : throw CreateInvalidStateException();

        /// <summary>
        /// Gets a value that indicates whether current value is stored in first branch.
        /// </summary>
        /// <value><see langword="true" /> if current value is stored in first branch; otherwise, <see langword="false" />.</value>
        public bool IsT0 => _index == 0;

        /// <summary>
        /// Gets a value that indicates whether current value is stored in second branch.
        /// </summary>
        /// <value><see langword="true" /> if current value is stored in second branch; otherwise, <see langword="false" />.</value>
        public bool IsT1 => _index == 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOf{T0, T1}" /> struct with first branch value.
        /// </summary>
        /// <param name="value">A value to store in first branch.</param>
        public OneOf(T0 value)
        {
            _value = value;
            _index = 0;
            _hasValue = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOf{T0, T1}" /> struct with second branch value.
        /// </summary>
        /// <param name="value">A value to store in second branch.</param>
        public OneOf(T1 value)
        {
            _value = value;
            _index = 1;
            _hasValue = true;
        }

        /// <summary>
        /// Projects current union into another value by selecting mapper for active branch.
        /// </summary>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="map0">A mapping function for first branch value.</param>
        /// <param name="map1">A mapping function for second branch value.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public TReturn Map<TReturn>(
            Func<T0, TReturn> map0,
            Func<T1, TReturn> map1)
        {
            return _index switch
            {
                0 => map0(AsT0),
                1 => map1(AsT1),
                _ => throw CreateInvalidStateException()
            };
        }

        /// <summary>
        /// Projects current union into another value by selecting mapper for active branch and passing shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="context">A context value passed to selected mapper.</param>
        /// <param name="map0">A mapping function for first branch value.</param>
        /// <param name="map1">A mapping function for second branch value.</param>
        /// <returns>A mapped value produced by selected mapper.</returns>
        public TReturn Map<TContext, TReturn>(TContext context,
            Func<TContext, T0, TReturn> map0,
            Func<TContext, T1, TReturn> map1)
        {
            return _index switch
            {
                0 => map0(context, AsT0),
                1 => map1(context, AsT1),
                _ => throw CreateInvalidStateException()
            };
        }

        /// <summary>
        /// Projects current union into another value asynchronously by selecting mapper for active branch.
        /// </summary>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="map0">A mapping function for first branch value.</param>
        /// <param name="map1">A mapping function for second branch value.</param>
        /// <returns>A task that produces mapped value from selected mapper.</returns>
        public UniTask<TReturn> MapAsync<TReturn>(
            Func<T0, UniTask<TReturn>> map0,
            Func<T1, UniTask<TReturn>> map1)
        {
            return _index switch
            {
                0 => map0(AsT0),
                1 => map1(AsT1),
                _ => throw CreateInvalidStateException()
            };
        }

        /// <summary>
        /// Projects current union into another value asynchronously by selecting mapper for active branch and passing shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TReturn">Type of mapped value.</typeparam>
        /// <param name="context">A context value passed to selected mapper.</param>
        /// <param name="map0">A mapping function for first branch value.</param>
        /// <param name="map1">A mapping function for second branch value.</param>
        /// <returns>A task that produces mapped value from selected mapper.</returns>
        public UniTask<TReturn> MapAsync<TContext, TReturn>(TContext context,
            Func<TContext, T0, UniTask<TReturn>> map0,
            Func<TContext, T1, UniTask<TReturn>> map1)
        {
            return _index switch
            {
                0 => map0(context, AsT0),
                1 => map1(context, AsT1),
                _ => throw CreateInvalidStateException()
            };
        }

        /// <summary>
        /// Gets first branch value when current union stores it.
        /// </summary>
        /// <param name="value">When this method returns, contains first branch value if it exists; otherwise, the default value of <typeparamref name="T0" />. This parameter is treated as uninitialized.</param>
        /// <returns><see langword="true" /> if current value is stored in first branch; otherwise, <see langword="false" />.</returns>
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

        /// <summary>
        /// Gets second branch value when current union stores it.
        /// </summary>
        /// <param name="value">When this method returns, contains second branch value if it exists; otherwise, the default value of <typeparamref name="T1" />. This parameter is treated as uninitialized.</param>
        /// <returns><see langword="true" /> if current value is stored in second branch; otherwise, <see langword="false" />.</returns>
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

        /// <summary>
        /// Gets first branch value or provided fallback when current union stores second branch.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>A first branch value or <paramref name="defaultValue" />.</returns>
        public T0 GetT0OrDefault(T0 defaultValue = default) => IsT0 ? AsT0 : defaultValue;

        /// <summary>
        /// Gets second branch value or provided fallback when current union stores first branch.
        /// </summary>
        /// <param name="defaultValue">A fallback value.</param>
        /// <returns>A second branch value or <paramref name="defaultValue" />.</returns>
        public T1 GetT1OrDefault(T1 defaultValue = default) => IsT1 ? AsT1 : defaultValue;

        /// <summary>
        /// Projects first branch into another first-branch type and preserves second branch unchanged.
        /// </summary>
        /// <typeparam name="TOut0">Type of mapped first branch.</typeparam>
        /// <param name="map">A mapping function for first branch value.</param>
        /// <returns>A union with mapped first branch or original second branch.</returns>
        public OneOf<TOut0, T1> MapT0<TOut0>(Func<T0, TOut0> map)
        {
            return IsT0 ? new OneOf<TOut0, T1>(map(AsT0)) : new OneOf<TOut0, T1>(AsT1);
        }

        /// <summary>
        /// Projects second branch into another second-branch type and preserves first branch unchanged.
        /// </summary>
        /// <typeparam name="TOut1">Type of mapped second branch.</typeparam>
        /// <param name="map">A mapping function for second branch value.</param>
        /// <returns>A union with original first branch or mapped second branch.</returns>
        public OneOf<T0, TOut1> MapT1<TOut1>(Func<T1, TOut1> map)
        {
            return IsT1 ? new OneOf<T0, TOut1>(map(AsT1)) : new OneOf<T0, TOut1>(AsT0);
        }

        /// <summary>
        /// Projects first branch into another first-branch type asynchronously and preserves second branch unchanged.
        /// </summary>
        /// <typeparam name="TOut0">Type of mapped first branch.</typeparam>
        /// <param name="map">A mapping function for first branch value.</param>
        /// <returns>A task that produces union with mapped first branch or original second branch.</returns>
        public async UniTask<OneOf<TOut0, T1>> MapT0Async<TOut0>(Func<T0, UniTask<TOut0>> map)
        {
            return IsT0 ? new OneOf<TOut0, T1>(await map(AsT0)) : new OneOf<TOut0, T1>(AsT1);
        }

        /// <summary>
        /// Projects second branch into another second-branch type asynchronously and preserves first branch unchanged.
        /// </summary>
        /// <typeparam name="TOut1">Type of mapped second branch.</typeparam>
        /// <param name="map">A mapping function for second branch value.</param>
        /// <returns>A task that produces union with original first branch or mapped second branch.</returns>
        public async UniTask<OneOf<T0, TOut1>> MapT1Async<TOut1>(Func<T1, UniTask<TOut1>> map)
        {
            return IsT1 ? new OneOf<T0, TOut1>(await map(AsT1)) : new OneOf<T0, TOut1>(AsT0);
        }

        /// <summary>
        /// Executes action for active branch.
        /// </summary>
        /// <param name="map0">An action for first branch value.</param>
        /// <param name="map1">An action for second branch value.</param>
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

            throw CreateInvalidStateException();
        }

        /// <summary>
        /// Executes action for active branch and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to selected action.</param>
        /// <param name="map0">An action for first branch value.</param>
        /// <param name="map1">An action for second branch value.</param>
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

            throw CreateInvalidStateException();
        }

        /// <summary>
        /// Executes asynchronous action for active branch.
        /// </summary>
        /// <param name="map0">An action for first branch value.</param>
        /// <param name="map1">An action for second branch value.</param>
        /// <returns>A task that completes when selected action finishes.</returns>
        public UniTask SwitchAsync(Func<T0, UniTask> map0, Func<T1, UniTask> map1)
        {
            if (IsT0)
                return map0(AsT0);

            if (IsT1)
                return map1(AsT1);

            throw CreateInvalidStateException();
        }

        /// <summary>
        /// Executes asynchronous action for active branch and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to selected action.</param>
        /// <param name="map0">An action for first branch value.</param>
        /// <param name="map1">An action for second branch value.</param>
        /// <returns>A task that completes when selected action finishes.</returns>
        public UniTask SwitchAsync<TContext>(TContext context,
            Func<TContext, T0, UniTask> map0,
            Func<TContext, T1, UniTask> map1)
        {
            if (IsT0)
                return map0(context, AsT0);

            if (IsT1)
                return map1(context, AsT1);

            throw CreateInvalidStateException();
        }

        /// <summary>
        /// Executes side effect for first branch and returns current union.
        /// </summary>
        /// <param name="tap">An action for first branch value.</param>
        /// <returns>Current union value.</returns>
        public OneOf<T0, T1> TapT0(Action<T0> tap)
        {
            if (IsT0)
                tap(AsT0);

            return this;
        }

        /// <summary>
        /// Executes side effect for first branch with shared context and returns current union.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tap" />.</param>
        /// <param name="tap">An action for first branch value.</param>
        /// <returns>Current union value.</returns>
        public OneOf<T0, T1> TapT0<TContext>(TContext context, Action<TContext, T0> tap)
        {
            if (IsT0)
                tap(context, AsT0);

            return this;
        }

        /// <summary>
        /// Executes side effect for second branch and returns current union.
        /// </summary>
        /// <param name="tap">An action for second branch value.</param>
        /// <returns>Current union value.</returns>
        public OneOf<T0, T1> TapT1(Action<T1> tap)
        {
            if (IsT1)
                tap(AsT1);

            return this;
        }

        /// <summary>
        /// Executes side effect for second branch with shared context and returns current union.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tap" />.</param>
        /// <param name="tap">An action for second branch value.</param>
        /// <returns>Current union value.</returns>
        public OneOf<T0, T1> TapT1<TContext>(TContext context, Action<TContext, T1> tap)
        {
            if (IsT1)
                tap(context, AsT1);

            return this;
        }

        /// <summary>
        /// Executes asynchronous side effect for first branch and returns current union.
        /// </summary>
        /// <param name="tap">An action for first branch value.</param>
        /// <returns>A task that produces current union value.</returns>
        public async UniTask<OneOf<T0, T1>> TapT0Async(Func<T0, UniTask> tap)
        {
            if (IsT0)
                await tap(AsT0);

            return this;
        }

        /// <summary>
        /// Executes asynchronous side effect for first branch with shared context and returns current union.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tap" />.</param>
        /// <param name="tap">An action for first branch value.</param>
        /// <returns>A task that produces current union value.</returns>
        public async UniTask<OneOf<T0, T1>> TapT0Async<TContext>(TContext context, Func<TContext, T0, UniTask> tap)
        {
            if (IsT0)
                await tap(context, AsT0);

            return this;
        }

        /// <summary>
        /// Executes asynchronous side effect for second branch and returns current union.
        /// </summary>
        /// <param name="tap">An action for second branch value.</param>
        /// <returns>A task that produces current union value.</returns>
        public async UniTask<OneOf<T0, T1>> TapT1Async(Func<T1, UniTask> tap)
        {
            if (IsT1)
                await tap(AsT1);

            return this;
        }

        /// <summary>
        /// Executes asynchronous side effect for second branch with shared context and returns current union.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <param name="context">A context value passed to <paramref name="tap" />.</param>
        /// <param name="tap">An action for second branch value.</param>
        /// <returns>A task that produces current union value.</returns>
        public async UniTask<OneOf<T0, T1>> TapT1Async<TContext>(TContext context, Func<TContext, T1, UniTask> tap)
        {
            if (IsT1)
                await tap(context, AsT1);

            return this;
        }

        /// <summary>
        /// Chains another union-producing operation that runs only for first branch.
        /// </summary>
        /// <typeparam name="TOut0">Type of chained first branch.</typeparam>
        /// <param name="binder">A function that produces next union from first branch value.</param>
        /// <returns>A chained union.</returns>
        public OneOf<TOut0, T1> BindT0<TOut0>(Func<T0, OneOf<TOut0, T1>> binder)
        {
            return IsT0 ? binder(AsT0) : new OneOf<TOut0, T1>(AsT1);
        }

        /// <summary>
        /// Chains another union-producing operation that runs only for first branch and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TOut0">Type of chained first branch.</typeparam>
        /// <param name="context">A context value passed to <paramref name="binder" />.</param>
        /// <param name="binder">A function that produces next union from first branch value.</param>
        /// <returns>A chained union.</returns>
        public OneOf<TOut0, T1> BindT0<TContext, TOut0>(TContext context, Func<TContext, T0, OneOf<TOut0, T1>> binder)
        {
            return IsT0 ? binder(context, AsT0) : new OneOf<TOut0, T1>(AsT1);
        }

        /// <summary>
        /// Chains another union-producing operation that runs only for second branch.
        /// </summary>
        /// <typeparam name="TOut1">Type of chained second branch.</typeparam>
        /// <param name="binder">A function that produces next union from second branch value.</param>
        /// <returns>A chained union.</returns>
        public OneOf<T0, TOut1> BindT1<TOut1>(Func<T1, OneOf<T0, TOut1>> binder)
        {
            return IsT1 ? binder(AsT1) : new OneOf<T0, TOut1>(AsT0);
        }

        /// <summary>
        /// Chains another union-producing operation that runs only for second branch and passes shared context.
        /// </summary>
        /// <typeparam name="TContext">Type of shared context.</typeparam>
        /// <typeparam name="TOut1">Type of chained second branch.</typeparam>
        /// <param name="context">A context value passed to <paramref name="binder" />.</param>
        /// <param name="binder">A function that produces next union from second branch value.</param>
        /// <returns>A chained union.</returns>
        public OneOf<T0, TOut1> BindT1<TContext, TOut1>(TContext context, Func<TContext, T1, OneOf<T0, TOut1>> binder)
        {
            return IsT1 ? binder(context, AsT1) : new OneOf<T0, TOut1>(AsT0);
        }

        /// <summary>
        /// Determines whether current union equals specified union.
        /// </summary>
        /// <param name="other">A union to compare with current union.</param>
        /// <returns><see langword="true" /> if current union equals <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool Equals(OneOf<T0, T1> other)
        {
            if (_hasValue != other._hasValue || _index != other._index)
                return false;

            return _index switch
            {
                0 => EqualityComparer<T0>.Default.Equals(AsT0, other.AsT0),
                1 => EqualityComparer<T1>.Default.Equals(AsT1, other.AsT1),
                _ => false
            };
        }

        /// <summary>
        /// Determines whether current union equals specified object.
        /// </summary>
        /// <param name="obj">An object to compare with current union.</param>
        /// <returns><see langword="true" /> if <paramref name="obj" /> is equal to current union; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => obj is OneOf<T0, T1> other && Equals(other);

        /// <summary>
        /// Serves as hash function for current union.
        /// </summary>
        /// <returns>A hash code for current union.</returns>
        public override int GetHashCode()
        {
            return _index switch
            {
                0 when _hasValue => HashCode.Combine(_hasValue, _index, EqualityComparer<T0>.Default.GetHashCode(AsT0)),
                1 when _hasValue => HashCode.Combine(_hasValue, _index, EqualityComparer<T1>.Default.GetHashCode(AsT1)),
                _ => HashCode.Combine(_hasValue, _index)
            };
        }

        /// <summary>
        /// Returns string representation of current union.
        /// </summary>
        /// <returns>A string that identifies active branch and stored value.</returns>
        public override string ToString()
        {
            return _index switch
            {
                0 => $"T0({AsT0})",
                1 => $"T1({AsT1})",
                _ => $"{nameof(OneOf<T0, T1>)}(Invalid)"
            };
        }

        private static InvalidOperationException CreateWrongBranchException(string branchName)
            => new($"OneOf does not contain a value of type {branchName}.");

        private static ArgumentOutOfRangeException CreateInvalidStateException()
            => new(nameof(OneOf<T0, T1>), "OneOf is in an invalid state.");

        /// <summary>
        /// Determines whether two unions are equal.
        /// </summary>
        /// <param name="left">A union to compare.</param>
        /// <param name="right">A union to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> equals <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(OneOf<T0, T1> left, OneOf<T0, T1> right) => left.Equals(right);

        /// <summary>
        /// Determines whether two unions are not equal.
        /// </summary>
        /// <param name="left">A union to compare.</param>
        /// <param name="right">A union to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left" /> does not equal <paramref name="right" />; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(OneOf<T0, T1> left, OneOf<T0, T1> right) => !left.Equals(right);
    }
}

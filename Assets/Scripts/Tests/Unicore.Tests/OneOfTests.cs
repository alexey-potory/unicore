using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Unicore.Monads;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class OneOfTests
    {
        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.NoUnexpectedReceived();
            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void Switch_OnT0_InvokesSelectedActionWithoutThrowing()
        {
            var oneOf = new OneOf<int, string>(42);
            var captured = 0;
            var otherCalled = false;

            Assert.DoesNotThrow(() => oneOf.Switch(
                value => captured = value,
                _ => otherCalled = true));

            Assert.That(captured, Is.EqualTo(42));
            Assert.That(otherCalled, Is.False);
        }

        [Test]
        public void Switch_OnDefaultValue_ThrowsArgumentOutOfRangeExceptionWithMessage()
        {
            var oneOf = default(OneOf<int, string>);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => oneOf.Switch(_ => { }, _ => { }));

            Assert.That(exception!.Message, Does.StartWith("OneOf is in an invalid state."));
        }

        [Test]
        public void Switch_WithContext_OnT1_InvokesSelectedActionWithoutThrowing()
        {
            var oneOf = new OneOf<int, string>("boom");
            var captured = string.Empty;
            var otherCalled = false;

            Assert.DoesNotThrow(() => oneOf.Switch("ctx",
                (_, _) => otherCalled = true,
                (context, value) => captured = $"{context}:{value}"));

            Assert.That(captured, Is.EqualTo("ctx:boom"));
            Assert.That(otherCalled, Is.False);
        }

        [UnityTest]
        public IEnumerator SwitchAsync_OnT0_InvokesSelectedActionWithoutThrowing()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var captured = 0;
                var otherCalled = false;

                await oneOf.SwitchAsync(
                    value =>
                    {
                        captured = value;
                        return UniTask.CompletedTask;
                    },
                    _ =>
                    {
                        otherCalled = true;
                        return UniTask.CompletedTask;
                    });

                Assert.That(captured, Is.EqualTo(42));
                Assert.That(otherCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator SwitchAsync_WithContext_OnT1_InvokesSelectedActionWithoutThrowing()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var captured = string.Empty;
                var otherCalled = false;

                await oneOf.SwitchAsync("ctx",
                    (_, _) =>
                    {
                        otherCalled = true;
                        return UniTask.CompletedTask;
                    },
                    (context, value) =>
                    {
                        captured = $"{context}:{value}";
                        return UniTask.CompletedTask;
                    });

                Assert.That(captured, Is.EqualTo("ctx:boom"));
                Assert.That(otherCalled, Is.False);
            });
        }

        [Test]
        public void TryGetT0_OnT0_ReturnsTrueAndValue()
        {
            var oneOf = new OneOf<int, string>(42);

            var result = oneOf.TryGetT0(out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void AsT0_OnT1_ThrowsInvalidOperationExceptionWithMessage()
        {
            var oneOf = new OneOf<int, string>("boom");

            var exception = Assert.Throws<InvalidOperationException>(() => _ = oneOf.AsT0);

            Assert.That(exception!.Message, Is.EqualTo("OneOf does not contain a value of type T0."));
        }

        [Test]
        public void TryGetT0_OnT1_ReturnsFalseAndDefault()
        {
            var oneOf = new OneOf<int, string>("boom");

            var result = oneOf.TryGetT0(out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryGetT1_OnT1_ReturnsTrueAndValue()
        {
            var oneOf = new OneOf<int, string>("boom");

            var result = oneOf.TryGetT1(out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("boom"));
        }

        [Test]
        public void AsT1_OnT0_ThrowsInvalidOperationExceptionWithMessage()
        {
            var oneOf = new OneOf<int, string>(42);

            var exception = Assert.Throws<InvalidOperationException>(() => _ = oneOf.AsT1);

            Assert.That(exception!.Message, Is.EqualTo("OneOf does not contain a value of type T1."));
        }

        [Test]
        public void TryGetT1_OnT0_ReturnsFalseAndDefault()
        {
            var oneOf = new OneOf<int, string>(42);

            var result = oneOf.TryGetT1(out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void GetT0OrDefault_OnT0_ReturnsValue()
        {
            var oneOf = new OneOf<int, string>(42);

            var value = oneOf.GetT0OrDefault(-1);

            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void GetT0OrDefault_OnT1_ReturnsFallback()
        {
            var oneOf = new OneOf<int, string>("boom");

            var value = oneOf.GetT0OrDefault(-1);

            Assert.That(value, Is.EqualTo(-1));
        }

        [Test]
        public void GetT1OrDefault_OnT1_ReturnsValue()
        {
            var oneOf = new OneOf<int, string>("boom");

            var value = oneOf.GetT1OrDefault("fallback");

            Assert.That(value, Is.EqualTo("boom"));
        }

        [Test]
        public void GetT1OrDefault_OnT0_ReturnsFallback()
        {
            var oneOf = new OneOf<int, string>(42);

            var value = oneOf.GetT1OrDefault("fallback");

            Assert.That(value, Is.EqualTo("fallback"));
        }

        [Test]
        public void MapT0_OnT0_MapsSelectedBranch()
        {
            var oneOf = new OneOf<int, string>(21);

            var mapped = oneOf.MapT0(value => value * 2);

            Assert.That(mapped.IsT0, Is.True);
            Assert.That(mapped.AsT0, Is.EqualTo(42));
        }

        [Test]
        public void Map_OnDefaultValue_ThrowsArgumentOutOfRangeExceptionWithMessage()
        {
            var oneOf = default(OneOf<int, string>);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => oneOf.Map(value => value, value => value.Length));

            Assert.That(exception!.Message, Does.StartWith("OneOf is in an invalid state."));
        }

        [Test]
        public void MapT0_OnT1_PreservesOtherBranch()
        {
            var oneOf = new OneOf<int, string>("boom");
            var called = false;

            var mapped = oneOf.MapT0(value =>
            {
                called = true;
                return value * 2;
            });

            Assert.That(called, Is.False);
            Assert.That(mapped.IsT1, Is.True);
            Assert.That(mapped.AsT1, Is.EqualTo("boom"));
        }

        [Test]
        public void MapT1_OnT1_MapsSelectedBranch()
        {
            var oneOf = new OneOf<int, string>("boom");

            var mapped = oneOf.MapT1(value => value.Length);

            Assert.That(mapped.IsT1, Is.True);
            Assert.That(mapped.AsT1, Is.EqualTo(4));
        }

        [Test]
        public void MapT1_OnT0_PreservesOtherBranch()
        {
            var oneOf = new OneOf<int, string>(42);
            var called = false;

            var mapped = oneOf.MapT1(value =>
            {
                called = true;
                return value.Length;
            });

            Assert.That(called, Is.False);
            Assert.That(mapped.IsT0, Is.True);
            Assert.That(mapped.AsT0, Is.EqualTo(42));
        }

        [UnityTest]
        public IEnumerator MapT0Async_OnT0_MapsSelectedBranch()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(21);

                var mapped = await oneOf.MapT0Async(value => UniTask.FromResult(value * 2));

                Assert.That(mapped.IsT0, Is.True);
                Assert.That(mapped.AsT0, Is.EqualTo(42));
            });
        }

        [UnityTest]
        public IEnumerator MapT0Async_OnT1_PreservesOtherBranch()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var called = false;

                var mapped = await oneOf.MapT0Async(value =>
                {
                    called = true;
                    return UniTask.FromResult(value * 2);
                });

                Assert.That(called, Is.False);
                Assert.That(mapped.IsT1, Is.True);
                Assert.That(mapped.AsT1, Is.EqualTo("boom"));
            });
        }

        [UnityTest]
        public IEnumerator MapT1Async_OnT1_MapsSelectedBranch()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");

                var mapped = await oneOf.MapT1Async(value => UniTask.FromResult(value.Length));

                Assert.That(mapped.IsT1, Is.True);
                Assert.That(mapped.AsT1, Is.EqualTo(4));
            });
        }

        [UnityTest]
        public IEnumerator MapT1Async_OnT0_PreservesOtherBranch()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var called = false;

                var mapped = await oneOf.MapT1Async(value =>
                {
                    called = true;
                    return UniTask.FromResult(value.Length);
                });

                Assert.That(called, Is.False);
                Assert.That(mapped.IsT0, Is.True);
                Assert.That(mapped.AsT0, Is.EqualTo(42));
            });
        }

        [Test]
        public void TapT0_OnT0_InvokesActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>(42);
            var captured = 0;

            var tapped = oneOf.TapT0(value => captured = value);

            Assert.That(captured, Is.EqualTo(42));
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [Test]
        public void TapT0_OnT1_DoesNotInvokeActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>("boom");
            var called = false;

            var tapped = oneOf.TapT0(_ => called = true);

            Assert.That(called, Is.False);
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [Test]
        public void TapT0_WithContext_OnT0_PassesContextAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>(42);
            var captured = string.Empty;

            var tapped = oneOf.TapT0("ctx", (context, value) => captured = $"{context}:{value}");

            Assert.That(captured, Is.EqualTo("ctx:42"));
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [Test]
        public void TapT0_WithContext_OnT1_DoesNotInvokeActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>("boom");
            var called = false;

            var tapped = oneOf.TapT0("ctx", (_, _) => called = true);

            Assert.That(called, Is.False);
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [Test]
        public void TapT1_OnT1_InvokesActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>("boom");
            var captured = string.Empty;

            var tapped = oneOf.TapT1(value => captured = value);

            Assert.That(captured, Is.EqualTo("boom"));
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [Test]
        public void TapT1_OnT0_DoesNotInvokeActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>(42);
            var called = false;

            var tapped = oneOf.TapT1(_ => called = true);

            Assert.That(called, Is.False);
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [UnityTest]
        public IEnumerator TapT1_WithContext_OnT1_PassesContextAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(() =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var captured = string.Empty;

                var tapped = oneOf.TapT1("ctx", (context, value) => captured = $"{context}:{value}");

                Assert.That(captured, Is.EqualTo("ctx:boom"));
                Assert.That(tapped, Is.EqualTo(oneOf));
                return UniTask.CompletedTask;
            });
        }

        [Test]
        public void TapT1_WithContext_OnT0_DoesNotInvokeActionAndReturnsSameValue()
        {
            var oneOf = new OneOf<int, string>(42);
            var called = false;

            var tapped = oneOf.TapT1("ctx", (_, _) => called = true);

            Assert.That(called, Is.False);
            Assert.That(tapped, Is.EqualTo(oneOf));
        }

        [UnityTest]
        public IEnumerator TapT0Async_OnT0_InvokesActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var captured = 0;

                var tapped = await oneOf.TapT0Async(value =>
                {
                    captured = value;
                    return UniTask.CompletedTask;
                });

                Assert.That(captured, Is.EqualTo(42));
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT0Async_OnT1_DoesNotInvokeActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var called = false;

                var tapped = await oneOf.TapT0Async(_ =>
                {
                    called = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(called, Is.False);
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT0Async_WithContext_OnT0_PassesContextAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var captured = string.Empty;

                var tapped = await oneOf.TapT0Async("ctx", (context, value) =>
                {
                    captured = $"{context}:{value}";
                    return UniTask.CompletedTask;
                });

                Assert.That(captured, Is.EqualTo("ctx:42"));
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT0Async_WithContext_OnT1_DoesNotInvokeActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var called = false;

                var tapped = await oneOf.TapT0Async("ctx", (_, _) =>
                {
                    called = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(called, Is.False);
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT1Async_OnT1_InvokesActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var captured = string.Empty;

                var tapped = await oneOf.TapT1Async(value =>
                {
                    captured = value;
                    return UniTask.CompletedTask;
                });

                Assert.That(captured, Is.EqualTo("boom"));
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT1Async_WithContext_OnT1_PassesContextAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>("boom");
                var captured = string.Empty;

                var tapped = await oneOf.TapT1Async("ctx", (context, value) =>
                {
                    captured = $"{context}:{value}";
                    return UniTask.CompletedTask;
                });

                Assert.That(captured, Is.EqualTo("ctx:boom"));
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT1Async_WithContext_OnT0_DoesNotInvokeActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var called = false;

                var tapped = await oneOf.TapT1Async("ctx", (_, _) =>
                {
                    called = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(called, Is.False);
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [UnityTest]
        public IEnumerator TapT1Async_OnT0_DoesNotInvokeActionAndReturnsSameValue()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var oneOf = new OneOf<int, string>(42);
                var called = false;

                var tapped = await oneOf.TapT1Async(_ =>
                {
                    called = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(called, Is.False);
                Assert.That(tapped, Is.EqualTo(oneOf));
            });
        }

        [Test]
        public void BindT0_OnT0_InvokesBinder()
        {
            var oneOf = new OneOf<int, string>(21);

            var bound = oneOf.BindT0(value => new OneOf<int, string>(value * 2));

            Assert.That(bound.IsT0, Is.True);
            Assert.That(bound.AsT0, Is.EqualTo(42));
        }

        [Test]
        public void BindT0_OnT1_PreservesOtherBranch()
        {
            var oneOf = new OneOf<int, string>("boom");
            var called = false;

            var bound = oneOf.BindT0(value =>
            {
                called = true;
                return new OneOf<int, string>(value * 2);
            });

            Assert.That(called, Is.False);
            Assert.That(bound.IsT1, Is.True);
            Assert.That(bound.AsT1, Is.EqualTo("boom"));
        }

        [Test]
        public void BindT0_WithContext_OnT0_PassesContextIntoBinder()
        {
            var oneOf = new OneOf<int, string>(21);

            var bound = oneOf.BindT0("ctx", (context, value) => new OneOf<object, string>((object)$"{context}:{value}"));

            Assert.That(bound.IsT0, Is.True);
            Assert.That(bound.AsT0, Is.EqualTo((object)"ctx:21"));
        }

        [Test]
        public void BindT0_WithContext_OnT1_DoesNotInvokeBinder()
        {
            var oneOf = new OneOf<int, string>("boom");
            var called = false;

            var bound = oneOf.BindT0("ctx", (context, value) =>
            {
                called = true;
                return new OneOf<object, string>((object)$"{context}:{value}");
            });

            Assert.That(called, Is.False);
            Assert.That(bound.IsT1, Is.True);
            Assert.That(bound.AsT1, Is.EqualTo("boom"));
        }

        [Test]
        public void BindT1_OnT1_InvokesBinder()
        {
            var oneOf = new OneOf<int, string>("boom");

            var bound = oneOf.BindT1(value => new OneOf<int, long>((long)value.Length));

            Assert.That(bound.IsT1, Is.True);
            Assert.That(bound.AsT1, Is.EqualTo(4L));
        }

        [Test]
        public void BindT1_OnT0_PreservesOtherBranch()
        {
            var oneOf = new OneOf<int, string>(42);
            var called = false;

            var bound = oneOf.BindT1(value =>
            {
                called = true;
                return new OneOf<int, long>((long)value.Length);
            });

            Assert.That(called, Is.False);
            Assert.That(bound.IsT0, Is.True);
            Assert.That(bound.AsT0, Is.EqualTo(42));
        }

        [Test]
        public void BindT1_WithContext_OnT1_PassesContextIntoBinder()
        {
            var oneOf = new OneOf<int, string>("boom");

            var bound = oneOf.BindT1("ctx", (context, value) => new OneOf<int, string>($"{context}:{value}"));

            Assert.That(bound.IsT1, Is.True);
            Assert.That(bound.AsT1, Is.EqualTo("ctx:boom"));
        }

        [Test]
        public void BindT1_WithContext_OnT0_DoesNotInvokeBinder()
        {
            var oneOf = new OneOf<int, string>(42);
            var called = false;

            var bound = oneOf.BindT1("ctx", (context, value) =>
            {
                called = true;
                return new OneOf<int, string>($"{context}:{value}");
            });

            Assert.That(called, Is.False);
            Assert.That(bound.IsT0, Is.True);
            Assert.That(bound.AsT0, Is.EqualTo(42));
        }

        [Test]
        public void ToString_OnT0_FormatsBranchAndValue()
        {
            var oneOf = new OneOf<int, string>(42);

            Assert.That(oneOf.ToString(), Is.EqualTo("T0(42)"));
        }

        [Test]
        public void ToString_OnT1_FormatsBranchAndValue()
        {
            var oneOf = new OneOf<int, string>("boom");

            Assert.That(oneOf.ToString(), Is.EqualTo("T1(boom)"));
        }

        [Test]
        public void Equality_MatchesBranchAndValue()
        {
            var left = new OneOf<int, string>(42);
            var same = new OneOf<int, string>(42);
            var otherValue = new OneOf<int, string>(21);
            var otherBranch = new OneOf<int, string>("42");

            Assert.That(left.Equals(same), Is.True);
            Assert.That(left.Equals((object)same), Is.True);
            Assert.That(left == same, Is.True);
            Assert.That(left != same, Is.False);
            Assert.That(left.Equals(otherValue), Is.False);
            Assert.That(left.Equals(otherBranch), Is.False);
        }

        [Test]
        public void GetHashCode_ForEqualValues_IsSame()
        {
            var left = new OneOf<int, string>(42);
            var right = new OneOf<int, string>(42);

            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }
    }
}

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Unicore.Monads;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class OptionTests
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
        public void SomeFactory_WithValue_ReturnsPresentOption()
        {
            var option = Option.Some(42);

            Assert.That(option.IsSome, Is.True);
            Assert.That(option.IsNone, Is.False);
            Assert.That(option.Value, Is.EqualTo(42));
            Assert.That(option.GetOrThrow(), Is.EqualTo(42));
        }

        [Test]
        public void SomeFactory_WithNullReference_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Option.Some<string>(null));
            Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null));
        }

        [Test]
        public void NoneFactory_ReturnsEmptyOption()
        {
            var option = Option.None<int>();

            Assert.That(option.IsSome, Is.False);
            Assert.That(option.IsNone, Is.True);
        }

        [Test]
        public void From_ValueOverload_WithNonNullReference_ReturnsPresentOption()
        {
            const string value = "hello";

            var option = Option.From(value);

            Assert.That(option.IsSome, Is.True);
            Assert.That(option.Value, Is.EqualTo(value));
        }

        [Test]
        public void From_ValueOverload_WithNullReference_ReturnsEmptyOption()
        {
            string value = null;

            var option = Option.From(value);

            Assert.That(option.IsNone, Is.True);
        }

        [Test]
        public void From_FactoryOverload_UsesFactoryResult()
        {
            var called = false;

            var some = Option.From(() =>
            {
                called = true;
                return "value";
            });
            var none = Option.From(() => (string)null);

            Assert.That(called, Is.True);
            Assert.That(some.Value, Is.EqualTo("value"));
            Assert.That(none.IsNone, Is.True);
        }

        [Test]
        public void From_ContextOverload_PassesContextIntoFactory()
        {
            var some = Option.From("ctx", context => $"{context}:value");
            var none = Option.From("ctx", _ => (string)null);

            Assert.That(some.Value, Is.EqualTo("ctx:value"));
            Assert.That(none.IsNone, Is.True);
        }

        [UnityTest]
        public IEnumerator FromAsync_FactoryOverload_UsesFactoryResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var called = false;

                var some = await Option.FromAsync(() =>
                {
                    called = true;
                    return UniTask.FromResult("value");
                });
                var none = await Option.FromAsync(() => UniTask.FromResult<string>(null));

                Assert.That(called, Is.True);
                Assert.That(some.Value, Is.EqualTo("value"));
                Assert.That(none.IsNone, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator FromAsync_ContextOverload_PassesContextIntoFactory()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var some = await Option.FromAsync("ctx", context => UniTask.FromResult($"{context}:value"));
                var none = await Option.FromAsync("ctx", _ => UniTask.FromResult<string>(null));

                Assert.That(some.Value, Is.EqualTo("ctx:value"));
                Assert.That(none.IsNone, Is.True);
            });
        }

        [Test]
        public void ToOption_ValueExtension_WithNonNullReference_ReturnsPresentOption()
        {
            const string value = "hello";

            var option = value.ToOption();

            Assert.That(option.IsSome, Is.True);
            Assert.That(option.Value, Is.EqualTo(value));
        }

        [Test]
        public void ToOption_ValueExtension_WithNullReference_ReturnsEmptyOption()
        {
            string value = null;

            var option = value.ToOption();

            Assert.That(option.IsNone, Is.True);
        }

        [Test]
        public void ToOption_FactoryExtension_UsesFactoryResult()
        {
            var called = false;
            Func<string> someFactory = () =>
            {
                called = true;
                return "value";
            };
            Func<string> noneFactory = () => null;

            var some = someFactory.ToOption();
            var none = noneFactory.ToOption();

            Assert.That(called, Is.True);
            Assert.That(some.Value, Is.EqualTo("value"));
            Assert.That(none.IsNone, Is.True);
        }

        [Test]
        public void ToOption_ContextExtension_PassesContextIntoFactory()
        {
            var some = "ctx".ToOption(context => $"{context}:value");
            var none = "ctx".ToOption(_ => (string)null);

            Assert.That(some.Value, Is.EqualTo("ctx:value"));
            Assert.That(none.IsNone, Is.True);
        }

        [UnityTest]
        public IEnumerator ToOptionAsync_FactoryExtension_UsesFactoryResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var called = false;
                Func<UniTask<string>> someFactory = () =>
                {
                    called = true;
                    return UniTask.FromResult("value");
                };
                Func<UniTask<string>> noneFactory = () => UniTask.FromResult<string>(null);

                var some = await someFactory.ToOptionAsync();
                var none = await noneFactory.ToOptionAsync();

                Assert.That(called, Is.True);
                Assert.That(some.Value, Is.EqualTo("value"));
                Assert.That(none.IsNone, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator ToOptionAsync_ContextExtension_PassesContextIntoFactory()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var some = await "ctx".ToOptionAsync(context => UniTask.FromResult($"{context}:value"));
                var none = await "ctx".ToOptionAsync(_ => UniTask.FromResult<string>(null));

                Assert.That(some.Value, Is.EqualTo("ctx:value"));
                Assert.That(none.IsNone, Is.True);
            });
        }

        [Test]
        public void Value_OnNone_ThrowsInvalidOperationException()
        {
            var option = Option.None<int>();

            var exception = Assert.Throws<InvalidOperationException>(() => _ = option.Value);

            Assert.That(exception!.Message, Is.EqualTo("Option has no value."));
        }

        [Test]
        public void GetOrThrow_OnNone_ThrowsInvalidOperationException()
        {
            var option = Option.None<int>();

            var exception = Assert.Throws<InvalidOperationException>(() => option.GetOrThrow());

            Assert.That(exception!.Message, Is.EqualTo("Option has no value."));
        }

        [Test]
        public void Map_OnSome_UsesSomeMapperOnly()
        {
            var option = Option.Some(21);
            var noneCalled = false;

            var mapped = option.Map(
                value => value * 2,
                () =>
                {
                    noneCalled = true;
                    return -1;
                });

            Assert.That(mapped, Is.EqualTo(42));
            Assert.That(noneCalled, Is.False);
        }

        [Test]
        public void Map_OnNone_UsesNoneMapperOnly()
        {
            var option = Option.None<int>();
            var someCalled = false;

            var mapped = option.Map(
                value =>
                {
                    someCalled = true;
                    return value * 2;
                },
                () => 7);

            Assert.That(mapped, Is.EqualTo(7));
            Assert.That(someCalled, Is.False);
        }

        [Test]
        public void Map_WithContext_PassesContextIntoSelectedMapper()
        {
            var some = Option.Some(7);
            var none = Option.None<int>();

            var someValue = some.Map("ctx", (context, value) => $"{context}:{value}", _ => "x");
            var noneValue = none.Map("ctx", (context, value) => $"{context}:{value}", context => $"{context}:none");

            Assert.That(someValue, Is.EqualTo("ctx:7"));
            Assert.That(noneValue, Is.EqualTo("ctx:none"));
        }

        [UnityTest]
        public IEnumerator MapAsync_OnSome_UsesSomeMapperOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(21);
                var noneCalled = false;

                var mapped = await option.MapAsync(
                    value => UniTask.FromResult(value * 2),
                    () =>
                    {
                        noneCalled = true;
                        return UniTask.FromResult(-1);
                    });

                Assert.That(mapped, Is.EqualTo(42));
                Assert.That(noneCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator MapAsync_OnNone_UsesNoneMapperOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.None<int>();
                var someCalled = false;

                var mapped = await option.MapAsync(
                    value =>
                    {
                        someCalled = true;
                        return UniTask.FromResult(value * 2);
                    },
                    () => UniTask.FromResult(7));

                Assert.That(mapped, Is.EqualTo(7));
                Assert.That(someCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator MapAsync_WithContext_PassesContextIntoSelectedMapper()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var some = Option.Some(7);
                var none = Option.None<int>();

                var someValue = await some.MapAsync("ctx",
                    (context, value) => UniTask.FromResult($"{context}:{value}"),
                    _ => UniTask.FromResult("x"));
                var noneValue = await none.MapAsync("ctx",
                    (context, value) => UniTask.FromResult($"{context}:{value}"),
                    context => UniTask.FromResult($"{context}:none"));

                Assert.That(someValue, Is.EqualTo("ctx:7"));
                Assert.That(noneValue, Is.EqualTo("ctx:none"));
            });
        }

        [Test]
        public void Match_OnSome_InvokesSomeActionOnly()
        {
            var option = Option.Some(5);
            var someValue = 0;
            var noneCalled = false;

            option.Match(
                value => someValue = value,
                () => noneCalled = true);

            Assert.That(someValue, Is.EqualTo(5));
            Assert.That(noneCalled, Is.False);
        }

        [Test]
        public void Match_OnNone_InvokesNoneActionOnly()
        {
            var option = Option.None<int>();
            var someCalled = false;
            var noneCalled = false;

            option.Match(
                _ => someCalled = true,
                () => noneCalled = true);

            Assert.That(someCalled, Is.False);
            Assert.That(noneCalled, Is.True);
        }

        [Test]
        public void Match_WithContext_PassesContextIntoSelectedAction()
        {
            var some = Option.Some(5);
            var none = Option.None<int>();
            var someValue = string.Empty;
            var noneValue = string.Empty;

            some.Match("ctx", (context, value) => someValue = $"{context}:{value}", _ => noneValue = "bad");
            none.Match("ctx", (context, value) => someValue = $"{context}:{value}", context => noneValue = $"{context}:none");

            Assert.That(someValue, Is.EqualTo("ctx:5"));
            Assert.That(noneValue, Is.EqualTo("ctx:none"));
        }

        [UnityTest]
        public IEnumerator MatchAsync_OnSome_InvokesSomeActionOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(5);
                var someValue = 0;
                var noneCalled = false;

                await option.MatchAsync(
                    value =>
                    {
                        someValue = value;
                        return UniTask.CompletedTask;
                    },
                    () =>
                    {
                        noneCalled = true;
                        return UniTask.CompletedTask;
                    });

                Assert.That(someValue, Is.EqualTo(5));
                Assert.That(noneCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator MatchAsync_OnNone_InvokesNoneActionOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.None<int>();
                var someCalled = false;
                var noneCalled = false;

                await option.MatchAsync(
                    _ =>
                    {
                        someCalled = true;
                        return UniTask.CompletedTask;
                    },
                    () =>
                    {
                        noneCalled = true;
                        return UniTask.CompletedTask;
                    });

                Assert.That(someCalled, Is.False);
                Assert.That(noneCalled, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator MatchAsync_WithContext_PassesContextIntoSelectedAction()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var some = Option.Some(5);
                var none = Option.None<int>();
                var someValue = string.Empty;
                var noneValue = string.Empty;

                await some.MatchAsync("ctx",
                    (context, value) =>
                    {
                        someValue = $"{context}:{value}";
                        return UniTask.CompletedTask;
                    },
                    _ =>
                    {
                        noneValue = "bad";
                        return UniTask.CompletedTask;
                    });
                await none.MatchAsync("ctx",
                    (context, value) =>
                    {
                        someValue = $"{context}:{value}";
                        return UniTask.CompletedTask;
                    },
                    context =>
                    {
                        noneValue = $"{context}:none";
                        return UniTask.CompletedTask;
                    });

                Assert.That(someValue, Is.EqualTo("ctx:5"));
                Assert.That(noneValue, Is.EqualTo("ctx:none"));
            });
        }

        [Test]
        public void Bind_OnSome_InvokesBinderAndReturnsItsResult()
        {
            var source = Option.Some(6);

            var result = source.Bind(value => Option.Some(value * 7));

            Assert.That(result.IsSome, Is.True);
            Assert.That(result.Value, Is.EqualTo(42));
        }

        [Test]
        public void Bind_OnNone_DoesNotInvokeBinderAndPreservesNone()
        {
            var source = Option.None<int>();
            var binderCalled = false;

            var result = source.Bind(value =>
            {
                binderCalled = true;
                return Option.Some(value * 2);
            });

            Assert.That(binderCalled, Is.False);
            Assert.That(result.IsNone, Is.True);
        }

        [Test]
        public void Bind_WithContext_PassesContextIntoBinder()
        {
            var some = Option.Some(8);
            var none = Option.None<int>();
            var binderCalled = false;

            var someResult = some.Bind("ctx", (context, value) =>
            {
                binderCalled = true;
                return Option.Some($"{context}:{value}");
            });

            var noneResult = none.Bind("ctx", (context, value) => Option.Some($"{context}:{value}"));

            Assert.That(binderCalled, Is.True);
            Assert.That(someResult.Value, Is.EqualTo("ctx:8"));
            Assert.That(noneResult.IsNone, Is.True);
        }

        [UnityTest]
        public IEnumerator BindAsync_OnSome_InvokesBinderAndReturnsItsResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var source = Option.Some(6);

                var result = await source.BindAsync(value => UniTask.FromResult<Option<int>>(Option.Some(value * 7)));

                Assert.That(result.IsSome, Is.True);
                Assert.That(result.Value, Is.EqualTo(42));
            });
        }

        [UnityTest]
        public IEnumerator BindAsync_OnNone_DoesNotInvokeBinderAndPreservesNone()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var source = Option.None<int>();
                var binderCalled = false;

                var result = await source.BindAsync(value =>
                {
                    binderCalled = true;
                    return UniTask.FromResult<Option<int>>(Option.Some(value * 2));
                });

                Assert.That(binderCalled, Is.False);
                Assert.That(result.IsNone, Is.True);
            });
        }

        [UnityTest]
        public IEnumerator BindAsync_WithContext_PassesContextIntoBinder()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var some = Option.Some(8);
                var none = Option.None<int>();
                var binderCalled = false;

                var someResult = await some.BindAsync("ctx", (context, value) =>
                {
                    binderCalled = true;
                    return UniTask.FromResult<Option<string>>(Option.Some($"{context}:{value}"));
                });

                var noneResult = await none.BindAsync("ctx",
                    (context, value) => UniTask.FromResult<Option<string>>(Option.Some($"{context}:{value}")));

                Assert.That(binderCalled, Is.True);
                Assert.That(someResult.Value, Is.EqualTo("ctx:8"));
                Assert.That(noneResult.IsNone, Is.True);
            });
        }

        [Test]
        public void Tap_OnSome_InvokesActionAndReturnsSameInstance()
        {
            var option = Option.Some(11);
            var tappedValue = 0;

            var returned = option.Tap(value => tappedValue = value);

            Assert.That(tappedValue, Is.EqualTo(11));
            Assert.That(returned, Is.SameAs(option));
        }

        [Test]
        public void Tap_OnNone_DoesNotInvokeActionAndReturnsSameInstance()
        {
            var option = Option.None<int>();
            var tapped = false;

            var returned = option.Tap(_ => tapped = true);

            Assert.That(tapped, Is.False);
            Assert.That(returned, Is.SameAs(option));
        }

        [Test]
        public void Tap_WithContext_PassesContextIntoAction()
        {
            var option = Option.Some(4);
            var tappedValue = string.Empty;

            var returned = option.Tap("ctx", (context, value) => tappedValue = $"{context}:{value}");

            Assert.That(tappedValue, Is.EqualTo("ctx:4"));
            Assert.That(returned, Is.SameAs(option));
        }

        [UnityTest]
        public IEnumerator TapAsync_OnSome_InvokesActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(11);
                var tappedValue = 0;

                var returned = await option.TapAsync(value =>
                {
                    tappedValue = value;
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedValue, Is.EqualTo(11));
                Assert.That(returned, Is.SameAs(option));
            });
        }

        [UnityTest]
        public IEnumerator TapAsync_OnNone_DoesNotInvokeActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.None<int>();
                var tapped = false;

                var returned = await option.TapAsync(_ =>
                {
                    tapped = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(tapped, Is.False);
                Assert.That(returned, Is.SameAs(option));
            });
        }

        [UnityTest]
        public IEnumerator TapAsync_WithContext_PassesContextIntoAction()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(4);
                var tappedValue = string.Empty;

                var returned = await option.TapAsync("ctx", (context, value) =>
                {
                    tappedValue = $"{context}:{value}";
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedValue, Is.EqualTo("ctx:4"));
                Assert.That(returned, Is.SameAs(option));
            });
        }

        [Test]
        public void GetOrDefault_OnSome_IgnoresDefaults()
        {
            var option = Option.Some(9);
            var factoryCalled = false;
            var contextFactoryCalled = false;

            Assert.That(option.GetOrDefault(1), Is.EqualTo(9));
            Assert.That(option.GetOrDefault(() =>
            {
                factoryCalled = true;
                return 2;
            }), Is.EqualTo(9));
            Assert.That(option.GetOrDefault("ctx", _ =>
            {
                contextFactoryCalled = true;
                return 3;
            }), Is.EqualTo(9));
            Assert.That(factoryCalled, Is.False);
            Assert.That(contextFactoryCalled, Is.False);
        }

        [UnityTest]
        public IEnumerator GetOrDefaultAsync_OnSome_IgnoresDefaults()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(9);
                var factoryCalled = false;
                var contextFactoryCalled = false;

                Assert.That(await option.GetOrDefaultAsync(() =>
                {
                    factoryCalled = true;
                    return UniTask.FromResult(2);
                }), Is.EqualTo(9));
                Assert.That(await option.GetOrDefaultAsync("ctx", _ =>
                {
                    contextFactoryCalled = true;
                    return UniTask.FromResult(3);
                }), Is.EqualTo(9));
                Assert.That(factoryCalled, Is.False);
                Assert.That(contextFactoryCalled, Is.False);
            });
        }

        [Test]
        public void GetOrDefault_OnNone_UsesProvidedDefaults()
        {
            var option = Option.None<int>();

            Assert.That(option.GetOrDefault(1), Is.EqualTo(1));
            Assert.That(option.GetOrDefault(() => 2), Is.EqualTo(2));
            Assert.That(option.GetOrDefault("ctx", context => context.Length), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator GetOrDefaultAsync_OnNone_UsesProvidedDefaults()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.None<int>();

                Assert.That(await option.GetOrDefaultAsync(() => UniTask.FromResult(2)), Is.EqualTo(2));
                Assert.That(await option.GetOrDefaultAsync("ctx", context => UniTask.FromResult(context.Length)),
                    Is.EqualTo(3));
            });
        }

        [Test]
        public void Ensure_OnSome_ReturnsSameInstanceWithoutEvaluatingFactories()
        {
            var option = Option.Some(12);
            var factoryCalled = false;
            var contextFactoryCalled = false;

            var ensuredValue = option.Ensure(1);
            var ensuredFactory = option.Ensure<string>(() =>
            {
                factoryCalled = true;
                return 2;
            });
            var ensuredContext = option.Ensure("ctx", _ =>
            {
                contextFactoryCalled = true;
                return 3;
            });

            Assert.That(ensuredValue, Is.SameAs(option));
            Assert.That(ensuredFactory, Is.SameAs(option));
            Assert.That(ensuredContext, Is.SameAs(option));
            Assert.That(factoryCalled, Is.False);
            Assert.That(contextFactoryCalled, Is.False);
        }

        [UnityTest]
        public IEnumerator EnsureAsync_OnSome_ReturnsSameInstanceWithoutEvaluatingFactories()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.Some(12);
                var factoryCalled = false;
                var contextFactoryCalled = false;

                var ensuredFactory = await option.EnsureAsync<string>(() =>
                {
                    factoryCalled = true;
                    return UniTask.FromResult(2);
                });
                var ensuredContext = await option.EnsureAsync("ctx", _ =>
                {
                    contextFactoryCalled = true;
                    return UniTask.FromResult(3);
                });

                Assert.That(ensuredFactory, Is.SameAs(option));
                Assert.That(ensuredContext, Is.SameAs(option));
                Assert.That(factoryCalled, Is.False);
                Assert.That(contextFactoryCalled, Is.False);
            });
        }

        [Test]
        public void Ensure_OnNone_UsesProvidedFallbacks()
        {
            var option = Option.None<int>();

            var ensuredValue = option.Ensure(1);
            var ensuredFactory = option.Ensure<string>(() => 2);
            var ensuredContext = option.Ensure("ctx", context => context.Length);

            Assert.That(ensuredValue.Value, Is.EqualTo(1));
            Assert.That(ensuredFactory.Value, Is.EqualTo(2));
            Assert.That(ensuredContext.Value, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator EnsureAsync_OnNone_UsesProvidedFallbacks()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var option = Option.None<int>();

                var ensuredFactory = await option.EnsureAsync<string>(() => UniTask.FromResult(2));
                var ensuredContext = await option.EnsureAsync("ctx", context => UniTask.FromResult(context.Length));

                Assert.That(ensuredFactory.Value, Is.EqualTo(2));
                Assert.That(ensuredContext.Value, Is.EqualTo(3));
            });
        }
    }
}

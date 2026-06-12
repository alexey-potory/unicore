using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unicore.Monads;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class ResultTests
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
        public void SuccessFactory_WithoutValue_ReturnsSuccessfulUnitResult()
        {
            var result = Result.Success();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Value, Is.EqualTo(Unit.Value));
        }

        [Test]
        public void GenericSuccessFactory_WithValue_ReturnsSuccessfulResult()
        {
            var result = Result.Success(42);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(42));
            Assert.That(result.GetOrThrow(), Is.EqualTo(42));
        }

        [Test]
        public void GenericSuccessFactory_WithNullReference_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Success<string>(null));
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null));
        }

        [Test]
        public void FailureFactory_WithoutError_ReturnsUnknownFailure()
        {
            LogAssert.Expect(LogType.Warning, "Result was in failure state but contained no error.");

            var result = Result.Failure<int>();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);

            var error = Assert.Catch(() => result.GetOrThrow());
            Assert.That(error!.GetType().Name, Is.EqualTo("UnknownException"));
            Assert.That(error.Message, Is.EqualTo("An unknown error occurred."));
        }

        [Test]
        public void GenericFailureFactory_WithoutError_ReturnsUnknownFailure()
        {
            LogAssert.Expect(LogType.Warning, "Result was in failure state but contained no error.");

            var result = Result<int>.Failure();

            Assert.That(result.IsFailure, Is.True);

            var error = Assert.Catch(() => result.GetOrThrow());
            Assert.That(error!.GetType().Name, Is.EqualTo("UnknownException"));
            Assert.That(error.Message, Is.EqualTo("An unknown error occurred."));
        }

        [Test]
        public void FailureFactory_WithError_ReturnsFailedResult()
        {
            var error = new InvalidOperationException("boom");
            var result = Result.Failure<int>(error);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.Throws<InvalidOperationException>(() => _ = result.Value);
            var thrown = Assert.Throws<InvalidOperationException>(() => result.ThrowIfError());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void FailureFactory_WithNullError_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Failure<int>(null));
            Assert.Throws<ArgumentNullException>(() => Result<int>.Failure(null));
        }

        [Test]
        public void Value_OnFailure_ThrowsInvalidOperationException()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));

            var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);

            Assert.That(exception!.Message, Is.EqualTo("Result is not successful."));
        }

        [Test]
        public void Map_OnSuccess_UsesSuccessMapperOnly()
        {
            var result = Result.Success(21);
            var failureCalled = false;

            var mapped = result.Map(
                value => value * 2,
                _ =>
                {
                    failureCalled = true;
                    return -1;
                });

            Assert.That(mapped, Is.EqualTo(42));
            Assert.That(failureCalled, Is.False);
        }

        [Test]
        public void Map_OnFailure_UsesFailureMapperOnly()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));
            var successCalled = false;

            var mapped = result.Map(
                value =>
                {
                    successCalled = true;
                    return (value * 2).ToString();
                },
                error => error.Message);

            Assert.That(mapped, Is.EqualTo("boom"));
            Assert.That(successCalled, Is.False);
        }

        [Test]
        public void Map_WithContext_PassesContextIntoSelectedMapper()
        {
            var success = Result.Success(7);
            var failure = Result.Failure<int>(new InvalidOperationException("boom"));

            var successValue = success.Map("ctx", (context, value) => $"{context}:{value}", (_, _) => "x");
            var failureValue = failure.Map("ctx", (_, _) => "x", (context, error) => $"{context}:{error.Message}");

            Assert.That(successValue, Is.EqualTo("ctx:7"));
            Assert.That(failureValue, Is.EqualTo("ctx:boom"));
        }

        [UnityTest]
        public IEnumerator MapAsync_OnSuccess_UsesSuccessMapperOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(21);
                var failureCalled = false;

                var mapped = await result.MapAsync(
                    value => UniTask.FromResult(value * 2),
                    _ =>
                    {
                        failureCalled = true;
                        return UniTask.FromResult(-1);
                    });

                Assert.That(mapped, Is.EqualTo(42));
                Assert.That(failureCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator MapAsync_OnFailure_UsesFailureMapperOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Failure<int>(new InvalidOperationException("boom"));
                var successCalled = false;

                var mapped = await result.MapAsync(
                    value =>
                    {
                        successCalled = true;
                        return UniTask.FromResult((value * 2).ToString());
                    },
                    error => UniTask.FromResult(error.Message));

                Assert.That(mapped, Is.EqualTo("boom"));
                Assert.That(successCalled, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator MapAsync_WithContext_PassesContextIntoSelectedMapper()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var success = Result.Success(7);
                var failure = Result.Failure<int>(new InvalidOperationException("boom"));

                var successValue = await success.MapAsync("ctx",
                    (context, value) => UniTask.FromResult($"{context}:{value}"),
                    (_, _) => UniTask.FromResult("x"));
                var failureValue = await failure.MapAsync("ctx",
                    (_, _) => UniTask.FromResult("x"),
                    (context, error) => UniTask.FromResult($"{context}:{error.Message}"));

                Assert.That(successValue, Is.EqualTo("ctx:7"));
                Assert.That(failureValue, Is.EqualTo("ctx:boom"));
            });
        }

        [Test]
        public void Match_OnSuccess_InvokesSuccessActionOnly()
        {
            var result = Result.Success(5);
            var successValue = 0;
            Exception capturedError = null;

            result.Match(
                value => successValue = value,
                error => capturedError = error);

            Assert.That(successValue, Is.EqualTo(5));
            Assert.That(capturedError, Is.Null);
        }

        [Test]
        public void Match_OnFailure_InvokesFailureActionOnly()
        {
            var error = new InvalidOperationException("boom");
            var result = Result.Failure<int>(error);
            var successCalled = false;
            Exception capturedError = null;

            result.Match(
                _ => successCalled = true,
                failure => capturedError = failure);

            Assert.That(successCalled, Is.False);
            Assert.That(capturedError, Is.SameAs(error));
        }

        [Test]
        public void Match_WithContext_PassesContextIntoSelectedAction()
        {
            var success = Result.Success(5);
            var failure = Result.Failure<int>(new InvalidOperationException("boom"));
            var successValue = string.Empty;
            var failureValue = string.Empty;

            success.Match("ctx", (context, value) => successValue = $"{context}:{value}", (_, _) => failureValue = "bad");
            failure.Match("ctx", (_, _) => successValue = "bad", (context, error) => failureValue = $"{context}:{error.Message}");

            Assert.That(successValue, Is.EqualTo("ctx:5"));
            Assert.That(failureValue, Is.EqualTo("ctx:boom"));
        }

        [UnityTest]
        public IEnumerator MatchAsync_OnSuccess_InvokesSuccessActionOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(5);
                var successValue = 0;
                Exception capturedError = null;

                await result.MatchAsync(
                    value =>
                    {
                        successValue = value;
                        return UniTask.CompletedTask;
                    },
                    error =>
                    {
                        capturedError = error;
                        return UniTask.CompletedTask;
                    });

                Assert.That(successValue, Is.EqualTo(5));
                Assert.That(capturedError, Is.Null);
            });
        }

        [UnityTest]
        public IEnumerator MatchAsync_OnFailure_InvokesFailureActionOnly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var error = new InvalidOperationException("boom");
                var result = Result.Failure<int>(error);
                var successCalled = false;
                Exception capturedError = null;

                await result.MatchAsync(
                    _ =>
                    {
                        successCalled = true;
                        return UniTask.CompletedTask;
                    },
                    failure =>
                    {
                        capturedError = failure;
                        return UniTask.CompletedTask;
                    });

                Assert.That(successCalled, Is.False);
                Assert.That(capturedError, Is.SameAs(error));
            });
        }

        [UnityTest]
        public IEnumerator MatchAsync_WithContext_PassesContextIntoSelectedAction()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var success = Result.Success(5);
                var failure = Result.Failure<int>(new InvalidOperationException("boom"));
                var successValue = string.Empty;
                var failureValue = string.Empty;

                await success.MatchAsync("ctx",
                    (context, value) =>
                    {
                        successValue = $"{context}:{value}";
                        return UniTask.CompletedTask;
                    },
                    (_, _) =>
                    {
                        failureValue = "bad";
                        return UniTask.CompletedTask;
                    });
                await failure.MatchAsync("ctx",
                    (_, _) =>
                    {
                        successValue = "bad";
                        return UniTask.CompletedTask;
                    },
                    (context, error) =>
                    {
                        failureValue = $"{context}:{error.Message}";
                        return UniTask.CompletedTask;
                    });

                Assert.That(successValue, Is.EqualTo("ctx:5"));
                Assert.That(failureValue, Is.EqualTo("ctx:boom"));
            });
        }

        [Test]
        public void Bind_OnSuccess_InvokesBinderAndReturnsItsResult()
        {
            var source = Result.Success(6);

            var result = source.Bind(value => Result.Success(value * 7));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(42));
        }

        [Test]
        public void Bind_OnFailure_DoesNotInvokeBinderAndPreservesError()
        {
            var error = new InvalidOperationException("boom");
            var source = Result.Failure<int>(error);
            var binderCalled = false;

            var result = source.Bind(value =>
            {
                binderCalled = true;
                return Result.Success(value * 2);
            });

            Assert.That(binderCalled, Is.False);
            Assert.That(result.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Bind_WithContext_PassesContextIntoBinder()
        {
            var success = Result.Success(8);
            var failure = Result.Failure<int>(new InvalidOperationException("boom"));
            var binderCalled = false;

            var successResult = success.Bind("ctx", (context, value) =>
            {
                binderCalled = true;
                return Result.Success($"{context}:{value}");
            });

            var failureResult = failure.Bind("ctx", (context, value) => Result.Success($"{context}:{value}"));

            Assert.That(binderCalled, Is.True);
            Assert.That(successResult.Value, Is.EqualTo("ctx:8"));
            Assert.That(failureResult.IsFailure, Is.True);
        }

        [UnityTest]
        public IEnumerator BindAsync_OnSuccess_InvokesBinderAndReturnsItsResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var source = Result.Success(6);

                var result = await source.BindAsync(value => UniTask.FromResult(Result.Success(value * 7)));

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(42));
            });
        }

        [UnityTest]
        public IEnumerator BindAsync_OnFailure_DoesNotInvokeBinderAndPreservesError()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var error = new InvalidOperationException("boom");
                var source = Result.Failure<int>(error);
                var binderCalled = false;

                var result = await source.BindAsync(value =>
                {
                    binderCalled = true;
                    return UniTask.FromResult(Result.Success(value * 2));
                });

                Assert.That(binderCalled, Is.False);
                Assert.That(result.IsFailure, Is.True);
                var thrown = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
                Assert.That(thrown, Is.SameAs(error));
            });
        }

        [UnityTest]
        public IEnumerator BindAsync_WithContext_PassesContextIntoBinder()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var success = Result.Success(8);
                var failure = Result.Failure<int>(new InvalidOperationException("boom"));
                var binderCalled = false;

                var successResult = await success.BindAsync("ctx", (context, value) =>
                {
                    binderCalled = true;
                    return UniTask.FromResult(Result.Success($"{context}:{value}"));
                });

                var failureResult = await failure.BindAsync("ctx",
                    (context, value) => UniTask.FromResult(Result.Success($"{context}:{value}")));

                Assert.That(binderCalled, Is.True);
                Assert.That(successResult.Value, Is.EqualTo("ctx:8"));
                Assert.That(failureResult.IsFailure, Is.True);
            });
        }

        [Test]
        public void Tap_OnSuccess_InvokesActionAndReturnsSameInstance()
        {
            var result = Result.Success(11);
            var tappedValue = 0;

            var returned = result.Tap(value => tappedValue = value);

            Assert.That(tappedValue, Is.EqualTo(11));
            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void Tap_OnFailure_DoesNotInvokeActionAndReturnsSameInstance()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));
            var tapped = false;

            var returned = result.Tap(_ => tapped = true);

            Assert.That(tapped, Is.False);
            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void Tap_WithContext_PassesContextIntoAction()
        {
            var result = Result.Success(4);
            var tappedValue = string.Empty;

            var returned = result.Tap("ctx", (context, value) => tappedValue = $"{context}:{value}");

            Assert.That(tappedValue, Is.EqualTo("ctx:4"));
            Assert.That(returned, Is.SameAs(result));
        }

        [UnityTest]
        public IEnumerator TapAsync_OnSuccess_InvokesActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(11);
                var tappedValue = 0;

                var returned = await result.TapAsync(value =>
                {
                    tappedValue = value;
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedValue, Is.EqualTo(11));
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [UnityTest]
        public IEnumerator TapAsync_OnFailure_DoesNotInvokeActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Failure<int>(new InvalidOperationException("boom"));
                var tapped = false;

                var returned = await result.TapAsync(_ =>
                {
                    tapped = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(tapped, Is.False);
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [UnityTest]
        public IEnumerator TapAsync_WithContext_PassesContextIntoAction()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(4);
                var tappedValue = string.Empty;

                var returned = await result.TapAsync("ctx", (context, value) =>
                {
                    tappedValue = $"{context}:{value}";
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedValue, Is.EqualTo("ctx:4"));
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [Test]
        public void TapError_OnFailure_InvokesActionAndReturnsSameInstance()
        {
            var error = new InvalidOperationException("boom");
            var result = Result.Failure<int>(error);
            Exception tappedError = null;

            var returned = result.TapError(captured => tappedError = captured);

            Assert.That(tappedError, Is.SameAs(error));
            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void TapError_OnSuccess_DoesNotInvokeActionAndReturnsSameInstance()
        {
            var result = Result.Success(1);
            var called = false;

            var returned = result.TapError(_ => called = true);

            Assert.That(called, Is.False);
            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void TapError_WithContext_PassesContextIntoAction()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));
            var tappedValue = string.Empty;

            var returned = result.TapError("ctx", (context, error) => tappedValue = $"{context}:{error.Message}");

            Assert.That(tappedValue, Is.EqualTo("ctx:boom"));
            Assert.That(returned, Is.SameAs(result));
        }

        [UnityTest]
        public IEnumerator TapErrorAsync_OnFailure_InvokesActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var error = new InvalidOperationException("boom");
                var result = Result.Failure<int>(error);
                Exception tappedError = null;

                var returned = await result.TapErrorAsync(captured =>
                {
                    tappedError = captured;
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedError, Is.SameAs(error));
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [UnityTest]
        public IEnumerator TapErrorAsync_OnSuccess_DoesNotInvokeActionAndReturnsSameInstance()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(1);
                var called = false;

                var returned = await result.TapErrorAsync(_ =>
                {
                    called = true;
                    return UniTask.CompletedTask;
                });

                Assert.That(called, Is.False);
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [UnityTest]
        public IEnumerator TapErrorAsync_WithContext_PassesContextIntoAction()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Failure<int>(new InvalidOperationException("boom"));
                var tappedValue = string.Empty;

                var returned = await result.TapErrorAsync("ctx", (context, error) =>
                {
                    tappedValue = $"{context}:{error.Message}";
                    return UniTask.CompletedTask;
                });

                Assert.That(tappedValue, Is.EqualTo("ctx:boom"));
                Assert.That(returned, Is.SameAs(result));
            });
        }

        [Test]
        public void LogIfError_OnSuccess_ReturnsSameInstanceWithoutLogs()
        {
            var result = Result.Success(3);

            var returned = result.LogIfError();

            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void LogIfError_OnFailure_LogsExceptionAndReturnsSameInstance()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: boom");

            var returned = result.LogIfError();

            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void GetOrDefault_OnSuccess_IgnoresDefaults()
        {
            var result = Result.Success(9);
            var factoryCalled = false;
            var contextFactoryCalled = false;

            Assert.That(result.GetOrDefault(1), Is.EqualTo(9));
            Assert.That(result.GetOrDefault(() =>
            {
                factoryCalled = true;
                return 2;
            }), Is.EqualTo(9));
            Assert.That(result.GetOrDefault("ctx", _ =>
            {
                contextFactoryCalled = true;
                return 3;
            }), Is.EqualTo(9));
            Assert.That(factoryCalled, Is.False);
            Assert.That(contextFactoryCalled, Is.False);
        }

        [UnityTest]
        public IEnumerator GetOrDefaultAsync_OnSuccess_IgnoresDefaults()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(9);
                var factoryCalled = false;
                var contextFactoryCalled = false;

                Assert.That(await result.GetOrDefaultAsync(() =>
                {
                    factoryCalled = true;
                    return UniTask.FromResult(2);
                }), Is.EqualTo(9));
                Assert.That(await result.GetOrDefaultAsync("ctx", _ =>
                {
                    contextFactoryCalled = true;
                    return UniTask.FromResult(3);
                }), Is.EqualTo(9));
                Assert.That(factoryCalled, Is.False);
                Assert.That(contextFactoryCalled, Is.False);
            });
        }

        [Test]
        public void GetOrDefault_OnFailure_UsesProvidedDefaults()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));

            Assert.That(result.GetOrDefault(1), Is.EqualTo(1));
            Assert.That(result.GetOrDefault(() => 2), Is.EqualTo(2));
            Assert.That(result.GetOrDefault("ctx", context => context.Length), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator GetOrDefaultAsync_OnFailure_UsesProvidedDefaults()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Failure<int>(new InvalidOperationException("boom"));

                Assert.That(await result.GetOrDefaultAsync(() => UniTask.FromResult(2)), Is.EqualTo(2));
                Assert.That(await result.GetOrDefaultAsync("ctx", context => UniTask.FromResult(context.Length)),
                    Is.EqualTo(3));
            });
        }

        [Test]
        public void Ensure_OnSuccess_ReturnsSameInstanceWithoutEvaluatingFactories()
        {
            var result = Result.Success(12);
            var factoryCalled = false;
            var contextFactoryCalled = false;

            var ensuredValue = result.Ensure(1);
            var ensuredFactory = result.Ensure<string>(() =>
            {
                factoryCalled = true;
                return 2;
            });
            var ensuredContext = result.Ensure("ctx", _ =>
            {
                contextFactoryCalled = true;
                return 3;
            });

            Assert.That(ensuredValue, Is.SameAs(result));
            Assert.That(ensuredFactory, Is.SameAs(result));
            Assert.That(ensuredContext, Is.SameAs(result));
            Assert.That(factoryCalled, Is.False);
            Assert.That(contextFactoryCalled, Is.False);
        }

        [UnityTest]
        public IEnumerator EnsureAsync_OnSuccess_ReturnsSameInstanceWithoutEvaluatingFactories()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Success(12);
                var factoryCalled = false;
                var contextFactoryCalled = false;

                var ensuredFactory = await result.EnsureAsync<string>(() =>
                {
                    factoryCalled = true;
                    return UniTask.FromResult(2);
                });
                var ensuredContext = await result.EnsureAsync("ctx", _ =>
                {
                    contextFactoryCalled = true;
                    return UniTask.FromResult(3);
                });

                Assert.That(ensuredFactory, Is.SameAs(result));
                Assert.That(ensuredContext, Is.SameAs(result));
                Assert.That(factoryCalled, Is.False);
                Assert.That(contextFactoryCalled, Is.False);
            });
        }

        [Test]
        public void Ensure_OnFailure_UsesProvidedFallbacks()
        {
            var result = Result.Failure<int>(new InvalidOperationException("boom"));

            var ensuredValue = result.Ensure(1);
            var ensuredFactory = result.Ensure<string>(() => 2);
            var ensuredContext = result.Ensure("ctx", context => context.Length);

            Assert.That(ensuredValue.Value, Is.EqualTo(1));
            Assert.That(ensuredFactory.Value, Is.EqualTo(2));
            Assert.That(ensuredContext.Value, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator EnsureAsync_OnFailure_UsesProvidedFallbacks()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var result = Result.Failure<int>(new InvalidOperationException("boom"));

                var ensuredFactory = await result.EnsureAsync<string>(() => UniTask.FromResult(2));
                var ensuredContext = await result.EnsureAsync("ctx", context => UniTask.FromResult(context.Length));

                Assert.That(ensuredFactory.Value, Is.EqualTo(2));
                Assert.That(ensuredContext.Value, Is.EqualTo(3));
            });
        }

        [Test]
        public void ThrowIfError_OnSuccess_ReturnsSameInstance()
        {
            var result = Result.Success(15);

            var returned = result.ThrowIfError();

            Assert.That(returned, Is.SameAs(result));
        }

        [Test]
        public void Try_Action_OnSuccess_ReturnsSuccessfulUnitResult()
        {
            var called = false;

            var result = Result.Try(() => { called = true; });

            Assert.That(called, Is.True);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(Unit.Value));
        }

        [Test]
        public void Try_Action_OnException_ReturnsFailureWithSameError()
        {
            var error = new InvalidOperationException("boom");

            var result = Result.Try(() => throw error);

            Assert.That(result.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Try_ActionWithContext_UsesContextAndCapturesExceptions()
        {
            LogAssert.Expect(LogType.Log, "ctx");
            var success = Result.Try("ctx", context => Debug.Log(context));

            var error = new InvalidOperationException("boom");
            var failure = Result.Try("ctx", _ => throw error);

            Assert.That(success.IsSuccess, Is.True);
            Assert.That(failure.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failure.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Try_Func_OnSuccess_ReturnsValue()
        {
            var result = Result.Try(() => 42);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(42));
        }

        [Test]
        public void Try_Func_OnException_ReturnsFailure()
        {
            var error = new InvalidOperationException("boom");

            var result = Result.Try<int>(() => throw error);

            Assert.That(result.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Try_FuncWithContext_UsesContextAndReturnsMappedValue()
        {
            var success = Result.Try("ctx", context => context.Length);
            var error = new InvalidOperationException("boom");
            var failure = Result.Try<int, string>("ctx", _ => throw error);

            Assert.That(success.Value, Is.EqualTo(3));
            Assert.That(failure.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failure.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }
    }
}

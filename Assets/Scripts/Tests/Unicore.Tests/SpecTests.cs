using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unicore.Monads;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class SpecTests
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
        public void Create_WithPredicate_UsesPredicateForEvaluation()
        {
            var spec = Spec<int>.Create(value => value > 10);

            Assert.That(spec.IsSatisfiedBy(11), Is.True);
            Assert.That(spec.IsSatisfiedBy(10), Is.False);
        }

        [Test]
        public void Constructor_WithRule_UsesRuleForEvaluation()
        {
            var spec = new Spec<int>(new EvenRule());

            Assert.That(spec.IsSatisfiedBy(4), Is.True);
            Assert.That(spec.IsSatisfiedBy(5), Is.False);
        }

        [Test]
        public void IsNotSatisfiedBy_ReturnsInverseOfIsSatisfiedBy()
        {
            var spec = Spec<int>.Create(value => value % 2 == 0);

            Assert.That(spec.IsNotSatisfiedBy(3), Is.True);
            Assert.That(spec.IsNotSatisfiedBy(4), Is.False);
        }

        [Test]
        public void All_WithoutSpecs_ReturnsAlwaysTrueSpec()
        {
            var spec = Spec<int>.All();

            Assert.That(spec.IsSatisfiedBy(-1), Is.True);
            Assert.That(spec.IsSatisfiedBy(0), Is.True);
            Assert.That(spec.IsSatisfiedBy(1), Is.True);
        }

        [Test]
        public void All_WithMultipleSpecs_RequiresAllToBeSatisfied()
        {
            var spec = Spec<int>.All(
                Spec<int>.Create(value => value > 0),
                Spec<int>.Create(value => value % 2 == 0),
                Spec<int>.Create(value => value < 10));

            Assert.That(spec.IsSatisfiedBy(4), Is.True);
            Assert.That(spec.IsSatisfiedBy(-2), Is.False);
            Assert.That(spec.IsSatisfiedBy(3), Is.False);
            Assert.That(spec.IsSatisfiedBy(12), Is.False);
        }

        [Test]
        public void Any_WithoutSpecs_ReturnsAlwaysFalseSpec()
        {
            var spec = Spec<int>.Any();

            Assert.That(spec.IsSatisfiedBy(-1), Is.False);
            Assert.That(spec.IsSatisfiedBy(0), Is.False);
            Assert.That(spec.IsSatisfiedBy(1), Is.False);
        }

        [Test]
        public void Any_WithMultipleSpecs_ReturnsTrueWhenAnySpecIsSatisfied()
        {
            var spec = Spec<int>.Any(
                Spec<int>.Create(value => value < 0),
                Spec<int>.Create(value => value == 10),
                Spec<int>.Create(value => value > 100));

            Assert.That(spec.IsSatisfiedBy(-5), Is.True);
            Assert.That(spec.IsSatisfiedBy(10), Is.True);
            Assert.That(spec.IsSatisfiedBy(101), Is.True);
            Assert.That(spec.IsSatisfiedBy(5), Is.False);
        }

        [Test]
        public void StaticNot_NegatesProvidedSpec()
        {
            var spec = Spec<int>.Not(Spec<int>.Create(value => value > 0));

            Assert.That(spec.IsSatisfiedBy(0), Is.True);
            Assert.That(spec.IsSatisfiedBy(1), Is.False);
        }

        [Test]
        public void AlwaysTrue_AlwaysAcceptsValues()
        {
            var spec = Spec<int>.AlwaysTrue();

            Assert.That(spec.IsSatisfiedBy(int.MinValue), Is.True);
            Assert.That(spec.IsSatisfiedBy(int.MaxValue), Is.True);
        }

        [Test]
        public void AlwaysFalse_AlwaysRejectsValues()
        {
            var spec = Spec<int>.AlwaysFalse();

            Assert.That(spec.IsSatisfiedBy(int.MinValue), Is.False);
            Assert.That(spec.IsSatisfiedBy(int.MaxValue), Is.False);
        }

        [Test]
        public void ToResult_WithoutError_ReturnsSuccessForSatisfiedValue()
        {
            var spec = Spec<int>.Create(value => value >= 0);

            var result = spec.ToResult(3);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(3));
        }

        [Test]
        public void ToResult_WithoutError_ReturnsUnknownFailureForUnsatisfiedValue()
        {
            var spec = Spec<int>.Create(value => value >= 0);
            LogAssert.Expect(LogType.Warning, "Result was in failure state but contained no error.");

            var result = spec.ToResult(-1);

            Assert.That(result.IsFailure, Is.True);
            var error = Assert.Catch(() => result.GetOrThrow());
            Assert.That(error!.GetType().Name, Is.EqualTo("UnknownException"));
            Assert.That(error.Message, Is.EqualTo("An unknown error occurred."));
        }

        [Test]
        public void ToResult_WithError_ReturnsFailedResultWithProvidedError()
        {
            var spec = Spec<int>.Create(value => value >= 0);
            var error = new InvalidOperationException("negative");

            var result = spec.ToResult(-1, error);

            Assert.That(result.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void ToResult_WithErrorFactory_UsesFactoryOnlyOnFailure()
        {
            var spec = Spec<int>.Create(value => value >= 0);
            var successFactoryCalled = false;
            var failureFactoryCalled = false;
            var error = new InvalidOperationException("negative");

            var successResult = spec.ToResult(5, () =>
            {
                successFactoryCalled = true;
                return error;
            });

            var failureResult = spec.ToResult(-5, () =>
            {
                failureFactoryCalled = true;
                return error;
            });

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(successFactoryCalled, Is.False);
            Assert.That(failureResult.IsFailure, Is.True);
            Assert.That(failureFactoryCalled, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failureResult.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Ensure_WithError_ReturnsSuccessOrProvidedFailure()
        {
            var spec = Spec<int>.Create(value => value >= 0);
            var error = new InvalidOperationException("negative");

            var successResult = spec.Ensure(2, error);
            var failureResult = spec.Ensure(-2, error);

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(successResult.Value, Is.EqualTo(2));
            Assert.That(failureResult.IsFailure, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failureResult.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void Ensure_WithErrorFactory_UsesFactoryOnlyOnFailure()
        {
            var spec = Spec<int>.Create(value => value >= 0);
            var successFactoryCalled = false;
            var failureFactoryCalled = false;
            var error = new InvalidOperationException("negative");

            var successResult = spec.Ensure(2, () =>
            {
                successFactoryCalled = true;
                return error;
            });

            var failureResult = spec.Ensure(-2, () =>
            {
                failureFactoryCalled = true;
                return error;
            });

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(successFactoryCalled, Is.False);
            Assert.That(failureResult.IsFailure, Is.True);
            Assert.That(failureFactoryCalled, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failureResult.GetOrThrow());
            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void GetValueOrDefault_ReturnsValueWhenSatisfiedOrFallbackOtherwise()
        {
            var spec = Spec<int>.Create(value => value > 0);

            Assert.That(spec.GetValueOrDefault(3, 10), Is.EqualTo(3));
            Assert.That(spec.GetValueOrDefault(-3, 10), Is.EqualTo(10));
        }

        [Test]
        public void AsPredicate_ReturnsDelegateThatUsesSpec()
        {
            var spec = Spec<int>.Create(value => value % 3 == 0);
            var predicate = spec.AsPredicate();

            Assert.That(predicate(6), Is.True);
            Assert.That(predicate(7), Is.False);
        }

        [Test]
        public void And_ReturnsSpecThatRequiresBothInputs()
        {
            var spec = Spec<int>.Create(value => value > 0)
                .And(Spec<int>.Create(value => value % 2 == 0));

            Assert.That(spec.IsSatisfiedBy(8), Is.True);
            Assert.That(spec.IsSatisfiedBy(7), Is.False);
            Assert.That(spec.IsSatisfiedBy(-8), Is.False);
        }

        [Test]
        public void Or_ReturnsSpecThatAcceptsEitherInput()
        {
            var spec = Spec<int>.Create(value => value < 0)
                .Or(Spec<int>.Create(value => value > 10));

            Assert.That(spec.IsSatisfiedBy(-1), Is.True);
            Assert.That(spec.IsSatisfiedBy(11), Is.True);
            Assert.That(spec.IsSatisfiedBy(5), Is.False);
        }

        [Test]
        public void InstanceNot_ReturnsNegatedSpec()
        {
            var spec = Spec<int>.Create(value => value == 1).Not();

            Assert.That(spec.IsSatisfiedBy(0), Is.True);
            Assert.That(spec.IsSatisfiedBy(1), Is.False);
        }

        private sealed class EvenRule : Spec<int>.ISpecRule
        {
            public bool IsSatisfiedBy(int value) => value % 2 == 0;
        }
    }
}

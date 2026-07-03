using NUnit.Framework;
using Unicore.Monads;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class UnitTests
    {
        [Test]
        public void Value_ReturnsSameUnitValue()
        {
            var first = Unit.Value;
            var second = Unit.Value;

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Value_EqualsDefaultStructValue()
        {
            Assert.That(Unit.Value, Is.EqualTo(default(Unit)));
        }

        [Test]
        public void SuccessHelpers_UseUnitValue()
        {
            var nonGeneric = Result.Success();
            var generic = Result<Unit>.Success();

            Assert.That(nonGeneric.Value, Is.EqualTo(Unit.Value));
            Assert.That(generic.Value, Is.EqualTo(Unit.Value));
        }
    }
}

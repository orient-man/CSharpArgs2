using NUnit.Framework;

namespace ConsoleApplication
{
    [TestFixture]
    public class ArgsTests
    {
        [Test]
        public void TestSimpleBooleanPresent()
        {
            var args = new Args("x", new[] { "-x" });
            Assert.AreEqual(true, args.GetBoolean('x'));
            Assert.AreEqual(1, args.Cardinality());
        }

        [Test]
        public void GetBooleanWhenArgumentNotPresentThrows()
        {
            var args = new Args("x#", new[] { "-x", "42" });
            Assert.Throws<ArgsException>(() => args.GetBoolean('x'));
        }

        [Test]
        public void TestSimpleStringPresent()
        {
            var args = new Args("x*", new[] { "-x", "param" });
            Assert.True(args.Has('x'));
            Assert.AreEqual("param", args.GetString('x'));
            Assert.AreEqual(1, args.Cardinality());
        }

        [Test]
        public void TestMissingStringArgument()
        {
            var args = new Args("x*", new[] { "-x" });
            Assert.False(args.IsValid());
            Assert.That(
                args.ErrorMessage(),
                Is.StringContaining("Could not find string parameter"));
        }

        [Test]
        public void TestSpacesInFormat()
        {
            var args = new Args("x, y", new[] { "-xy" });
            Assert.True(args.Has('x'));
            Assert.True(args.Has('y'));
            Assert.AreEqual(2, args.Cardinality());
        }

        [Test]
        public void TestSimpleIntPresent()
        {
            var args = new Args("x#", new[] { "-x", "42" });
            Assert.True(args.Has('x'));
            Assert.AreEqual(42, args.GetInt('x'));
            Assert.AreEqual(1, args.Cardinality());
        }

        [Test]
        public void TestInvalidInteger()
        {
            var args = new Args("x#", new[] { "-x", "Forty two" });
            Assert.False(args.IsValid());
            Assert.That(
                args.ErrorMessage(),
                Is.StringContaining("expects an integer but was"));
        }

        [Test]
        public void TestMissingInteger()
        {
            var args = new Args("x#", new[] { "-x" });
            Assert.False(args.IsValid());
            Assert.That(
                args.ErrorMessage(),
                Is.StringContaining("Could not find integer parameter"));
        }

        [Test]
        public void TestExtraArguments()
        {
            var args = new Args("x,y*", new[] { "-x", "-y", "alpha", "beta" });
            Assert.True(args.GetBoolean('x'));
            Assert.AreEqual("alpha", args.GetString('y'));
            Assert.AreEqual(2, args.Cardinality());
        }
    }
}
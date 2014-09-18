﻿using NUnit.Framework;

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
        public void GetIntWhenArgumentNotPresentThrows()
        {
            var args = new Args("x", new[] { "-x" });
            Assert.Throws<ArgsException>(() => args.GetInt('x'));
        }

        [Test]
        public void GetStringWhenArgumentNotPresentThrows()
        {
            var args = new Args("x", new[] { "-x" });
            Assert.Throws<ArgsException>(() => args.GetString('x'));
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
            try
            {
                new Args("x*", new[] { "-x" });
                Assert.Fail();
            }
            catch (ArgsException e)
            {
                Assert.AreEqual(ErrorCode.MissingString, e.ErrorCode);
                Assert.That(
                    e.ErrorMessage(),
                    Is.StringContaining("Could not find string parameter"));
            }
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
            try
            {
                new Args("x#", new[] { "-x", "Forty two" });
                Assert.Fail();
            }
            catch (ArgsException e)
            {
                Assert.AreEqual(ErrorCode.InvalidInteger, e.ErrorCode);
                Assert.That(
                    e.ErrorMessage(),
                    Is.StringContaining("expects an integer but was"));
            }
        }

        [Test]
        public void TestMissingInteger()
        {
            try
            {
                new Args("x#", new[] { "-x" });
                Assert.Fail();
            }
            catch (ArgsException e)
            {
                Assert.AreEqual(ErrorCode.MissingInteger, e.ErrorCode);
                Assert.That(
                    e.ErrorMessage(),
                    Is.StringContaining("Could not find integer parameter"));
            }
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
using NUnit.Framework;

namespace ConsoleApplication
{
    [TestFixture]
    public class ArgsExceptionTests
    {
        [Test]
        public void TestUnexpectedMessage()
        {
            var e = new ArgsException(ErrorCode.UnexpectedArgument, 'x');
            Assert.AreEqual("Argument -x unexpected.", e.GetErrorMessage());
        }

        [Test]
        public void TestMissingStringMessage()
        {
            var e = new ArgsException(ErrorCode.MissingString, 'x');
            Assert.AreEqual("Could not find string parameter for -x.", e.GetErrorMessage());
        }

        [Test]
        public void TestInvalidIntegerMessage()
        {
            var e = new ArgsException(ErrorCode.InvalidInteger, 'x', "Forty two");
            Assert.AreEqual("Argument -x expects an integer but was 'Forty two'.", e.GetErrorMessage());
        }

        [Test]
        public void TestMissingIntegerMessage()
        {
            var e = new ArgsException(ErrorCode.MissingInteger, 'x');
            Assert.AreEqual("Could not find integer parameter for -x.", e.GetErrorMessage());
        }

        [Test]
        public void TestInvalidFormat()
        {
            var e = new ArgsException(ErrorCode.InvalidArgumentFormat, 'x', "$");
            Assert.AreEqual("'$' is not a valid argument format.", e.GetErrorMessage());
        }
    }
}
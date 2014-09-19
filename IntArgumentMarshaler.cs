using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class IntArgumentMarshaler : IArgumentMarshaler<int>
    {
        public int Value { get; private set; }

        public void Set(IEnumerator<string> currentArgument)
        {
            string parameter = null;

            try
            {
                parameter = currentArgument.Next();
                Value = Int32.Parse(parameter);
            }
            catch (InvalidOperationException)
            {
                throw new ArgsException(ErrorCode.MissingInteger);
            }
            catch (FormatException)
            {
                throw new ArgsException(
                    ErrorCode.InvalidInteger,
                    errorParameter: parameter);
            }
        }
    }
}
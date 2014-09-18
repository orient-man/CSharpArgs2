using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class IntArgumentMarshaler : IArgumentMarshaler
    {
        private int intValue;

        public void Set(IEnumerator<string> currentArgument)
        {
            string parameter = null;

            try
            {
                currentArgument.MoveNext();
                parameter = currentArgument.Current;
                intValue = Int32.Parse(parameter);
            }
            catch (InvalidOperationException)
            {
                throw new ArgsException(ErrorCode.MissingInteger);
            }
            catch (FormatException)
            {
                throw new ArgsException(parameter, ErrorCode.InvalidInteger);
            }
        }

        public object Get()
        {
            return intValue;
        }
    }
}
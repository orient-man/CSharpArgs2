using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class StringArgumentMarshaler : IArgumentMarshaler
    {
        private string stringValue;

        public void Set(IEnumerator<string> currentArgument)
        {
            try
            {
                currentArgument.MoveNext();
                stringValue = currentArgument.Current;
            }
            catch (InvalidOperationException)
            {
                throw new ArgsException(ErrorCode.MissingString);
            }
        }

        public object Get()
        {
            return stringValue;
        }
    }
}
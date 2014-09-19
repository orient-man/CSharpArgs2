using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class StringArgumentMarshaler : IArgumentMarshaler<string>
    {
        public string Value { get; private set; }

        public void Set(IEnumerator<string> currentArgument)
        {
            try
            {
                currentArgument.MoveNext();
                Value = currentArgument.Current;
            }
            catch (InvalidOperationException)
            {
                throw new ArgsException(ErrorCode.MissingString);
            }
        }
    }
}
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class BoolArgumentMarshaler : IArgumentMarshaler<bool>
    {
        public bool Value { get; private set; }

        public void Set(IEnumerator<string> currentArgument)
        {
            Value = true;
        }
    }
}
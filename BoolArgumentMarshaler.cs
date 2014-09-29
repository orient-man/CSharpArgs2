using System.Collections.Generic;

namespace ConsoleApplication
{
    public class BoolArgumentMarshaler : IArgumentMarshaler
    {
        public object Marshal(IEnumerator<string> currentArgument)
        {
            return true;
        }
    }
}
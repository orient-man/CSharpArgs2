using System.Collections.Generic;

namespace ConsoleApplication
{
    public class BoolArgumentMarshaler : IArgumentMarshaler
    {
        private bool boolValue;

        public void Set(IEnumerator<string> currentArgument)
        {
            boolValue = true;
        }

        public object Get()
        {
            return boolValue;
        }
    }
}
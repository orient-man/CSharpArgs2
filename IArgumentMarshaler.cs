using System.Collections.Generic;

namespace ConsoleApplication
{
    public interface IArgumentMarshaler
    {
        void Set(IEnumerator<string> currentArgument);
        object Get();
    }
}
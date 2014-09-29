using System.Collections.Generic;

namespace ConsoleApplication
{
    public interface IArgumentMarshaler
    {
        object Marshal(IEnumerator<string> currentArgument);
    }
}
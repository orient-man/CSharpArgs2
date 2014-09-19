using System;
using System.Collections.Generic;
using System.Globalization;

namespace ConsoleApplication
{
    public class DoubleArgumentMarshaler : IArgumentMarshaler<double>
    {
        public double Value { get; private set; }

        public void Set(IEnumerator<string> currentArgument)
        {
            string parameter = null;

            try
            {
                parameter = currentArgument.Next();
                Value = double.Parse(parameter, CultureInfo.InvariantCulture);
            }
            catch (InvalidOperationException)
            {
                throw new ArgsException(ErrorCode.MissingDouble);
            }
            catch (FormatException)
            {
                throw new ArgsException(
                    ErrorCode.InvalidDouble,
                    errorParameter: parameter);
            }
        }
    }
}
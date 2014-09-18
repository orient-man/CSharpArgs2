using System;

namespace ConsoleApplication
{
    class ArgsException : Exception
    {
        private readonly char errorArgumentId;
        private readonly string errorParameter;

        public ErrorCode ErrorCode { get; private set; }

        public ArgsException() : this(ErrorCode.Ok)
        {
        }

        public ArgsException(ErrorCode errorCode) : this('\0', "TILT", errorCode)
        {
        }

        public ArgsException(string errorParameter, ErrorCode errorCode)
            : this('\0', errorParameter, errorCode)
        {
        }

        public ArgsException(char errorArgumentId, ErrorCode errorCode)
            : this(errorArgumentId, "TILT", errorCode)
        {
        }

        private ArgsException(char errorArgumentId, string errorParameter, ErrorCode errorCode)
        {
            this.errorArgumentId = errorArgumentId;
            this.errorParameter = errorParameter;
            ErrorCode = errorCode;
        }

        public string ErrorMessage()
        {
            switch (ErrorCode)
            {
                case ErrorCode.Ok:
                    throw new Exception("TILT: Should not get here.");
                case ErrorCode.UnexpectedArgument:
                    return string.Format(
                        "Argument {0} unexpected.",
                        errorArgumentId);
                case ErrorCode.MissingString:
                    return string.Format(
                        "Could not find string parameter for -{0}.",
                        errorArgumentId);
                case ErrorCode.InvalidInteger:
                    return string.Format(
                        "Argument -{0} expects an integer but was '{1}'.",
                        errorArgumentId,
                        errorParameter);
                case ErrorCode.MissingInteger:
                    return string.Format(
                        "Could not find integer parameter for -{0}.",
                        errorArgumentId);
            }
            return "";
        }
    }
}
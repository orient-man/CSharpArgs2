using System;

namespace ConsoleApplication
{
    class ArgsException : Exception
    {
        private char ErrorArgumentId { get; set; }

        public string ErrorParameter { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public ArgsException() : this(ErrorCode.Ok)
        {
        }

        public ArgsException(ErrorCode errorCode) : this('\0', "TILT", errorCode)
        {
        }

        public ArgsException(char errorArgumentId, ErrorCode errorCode)
            : this(errorArgumentId, "TILT", errorCode)
        {
        }

        private ArgsException(char errorArgumentId, string errorParameter, ErrorCode errorCode)
        {
            ErrorArgumentId = errorArgumentId;
            ErrorParameter = errorParameter;
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
                        ErrorArgumentId);
                case ErrorCode.MissingString:
                    return string.Format(
                        "Could not find string parameter for -{0}.",
                        ErrorArgumentId);
                case ErrorCode.InvalidInteger:
                    return string.Format(
                        "Argument -{0} expects an integer but was '{1}'.",
                        ErrorArgumentId,
                        ErrorParameter);
                case ErrorCode.MissingInteger:
                    return string.Format(
                        "Could not find integer parameter for -{0}.",
                        ErrorArgumentId);
            }
            return "";
        }
    }
}
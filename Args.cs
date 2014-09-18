﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApplication
{
    public class Args
    {
        private readonly string schema;
        private readonly string[] args;
        private bool valid = true;
        private readonly HashSet<Char> unexpectedArguments = new HashSet<char>();

        private readonly Dictionary<char, ArgumentMarshaler> booleanArgs =
            new Dictionary<char, ArgumentMarshaler>();

        private readonly Dictionary<char, ArgumentMarshaler> stringArgs =
            new Dictionary<char, ArgumentMarshaler>();

        private readonly Dictionary<char, ArgumentMarshaler> intArgs =
            new Dictionary<char, ArgumentMarshaler>();

        private readonly IDictionary<char, ArgumentMarshaler> marshalers =
            new Dictionary<char, ArgumentMarshaler>();

        private readonly HashSet<char> argsFound = new HashSet<char>();
        private int currentArgument;
        private char errorArgumentId = '\0';
        private string errorParameter = "TILT";
        private ErrorCode errorCode = ErrorCode.Ok;

        private enum ErrorCode
        {
            Ok,
            MissingString,
            MissingInteger,
            InvalidInteger,
            UnexpectedArgument
        }

        public Args(string schema, string[] args)
        {
            this.schema = schema;
            this.args = args;
            valid = Parse();
        }

        private bool Parse()
        {
            if (schema.Length == 0 && args.Length == 0)
                return true;
            ParseSchema();
            try
            {
                ParseArguments();
            }
            catch (ArgsException)
            {
            }
            return valid;
        }

        private bool ParseSchema()
        {
            foreach (var element in schema.Split(','))
            {
                if (element.Length > 0)
                {
                    var trimmedElement = element.Trim();
                    ParseSchemaElement(trimmedElement);
                }
            }
            return true;
        }

        private void ParseSchemaElement(string element)
        {
            var elementId = element[0];
            var elementTail = element.Substring(1);
            ValidateSchemaElementId(elementId);
            if (IsBooleanSchemaElement(elementTail))
                ParseBooleanSchemaElement(elementId);
            else if (IsStringSchemaElement(elementTail))
                ParseStringSchemaElement(elementId);
            else if (IsIntegerSchemaElement(elementTail))
            {
                ParseIntegerSchemaElement(elementId);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        "Argument: {0} has invalid format: {1}.",
                        elementId,
                        elementTail));
            }
        }

        private void ValidateSchemaElementId(char elementId)
        {
            if (!char.IsLetter(elementId))
            {
                throw new ArgumentException(
                    "Bad character:" + elementId + "in Args format: " + schema);
            }
        }

        private void ParseBooleanSchemaElement(char elementId)
        {
            marshalers[elementId] = booleanArgs[elementId] = new BoolArgumentMarshaler();
        }

        private void ParseIntegerSchemaElement(char elementId)
        {
            marshalers[elementId] = intArgs[elementId] = new IntArgumentMarshaler();
        }

        private void ParseStringSchemaElement(char elementId)
        {
            marshalers[elementId] = stringArgs[elementId] = new StringArgumentMarshaler();
        }

        private static bool IsStringSchemaElement(string elementTail)
        {
            return elementTail == "*";
        }

        private static bool IsBooleanSchemaElement(string elementTail)
        {
            return elementTail.Length == 0;
        }

        private static bool IsIntegerSchemaElement(string elementTail)
        {
            return elementTail == "#";
        }

        private bool ParseArguments()
        {
            for (currentArgument = 0; currentArgument < args.Length; currentArgument++)
            {
                var arg = args[currentArgument];
                ParseArgument(arg);
            }
            return true;
        }

        private void ParseArgument(string arg)
        {
            if (arg.StartsWith("-"))
                ParseElements(arg);
        }

        private void ParseElements(string arg)
        {
            for (var i = 1; i < arg.Length; i++)
                ParseElement(arg[i]);
        }

        private void ParseElement(char argChar)
        {
            if (SetArgument(argChar))
                argsFound.Add(argChar);
            else
            {
                unexpectedArguments.Add(argChar);
                errorCode = ErrorCode.UnexpectedArgument;
                valid = false;
            }
        }

        private bool SetArgument(char argChar)
        {
            ArgumentMarshaler m;
            if (!marshalers.TryGetValue(argChar, out m))
                return true;

            try
            {
                if (m is BoolArgumentMarshaler)
                    SetBooleanArg(m);
                else if (m is StringArgumentMarshaler)
                    SetStringArg(m);
                else if (m is IntArgumentMarshaler)
                    SetIntArg(m);
                else
                    return false;
            }
            catch (ArgsException)
            {
                valid = false;
                errorArgumentId = argChar;
                throw;
            }
            return true;
        }

        private void SetIntArg(ArgumentMarshaler m)
        {
            currentArgument++;
            string parameter = null;
            try
            {
                parameter = args[currentArgument];
                m.Set(parameter);
            }
            catch (IndexOutOfRangeException)
            {
                errorCode = ErrorCode.MissingInteger;
                throw new ArgsException();
            }
            catch (ArgsException)
            {
                errorParameter = parameter;
                errorCode = ErrorCode.InvalidInteger;
                throw;
            }
        }

        private void SetStringArg(ArgumentMarshaler m)
        {
            currentArgument++;
            try
            {
                m.Set(args[currentArgument]);
            }
            catch (IndexOutOfRangeException)
            {
                errorCode = ErrorCode.MissingString;
                throw new ArgsException();
            }
        }

        private void SetBooleanArg(ArgumentMarshaler m)
        {
            m.Set("true");
        }

        public int Cardinality()
        {
            return argsFound.Count;
        }

        public string Usage()
        {
            if (schema.Length > 0)
                return "-[" + schema + "]";
            return "";
        }

        public string ErrorMessage()
        {
            switch (errorCode)
            {
                case ErrorCode.Ok:
                    throw new Exception("TILT: Should not get here.");
                case ErrorCode.UnexpectedArgument:
                    return UnexpectedArgumentMessage();
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

        private string UnexpectedArgumentMessage()
        {
            var message = new StringBuilder("Argument(s) -");
            foreach (var c in unexpectedArguments)
            {
                message.Append(c);
            }
            message.Append(" unexpected.");
            return message.ToString();
        }

        private static bool FalseIfNull(bool? b)
        {
            return b != null && b.Value;
        }

        private static int ZeroIfNull(int? i)
        {
            return i == null ? 0 : i.Value;
        }

        private static string BlankIfNull(string s)
        {
            return s ?? "";
        }

        public string GetString(char arg)
        {
            return BlankIfNull(
                stringArgs.ContainsKey(arg)
                    ? (string)stringArgs[arg].Get()
                    : null);
        }

        public int GetInt(char arg)
        {
            return ZeroIfNull(
                intArgs.ContainsKey(arg)
                    ? (int?)intArgs[arg].Get()
                    : null);
        }

        public bool GetBoolean(char arg)
        {
            return FalseIfNull(
                booleanArgs.ContainsKey(arg)
                    ? (bool?)booleanArgs[arg].Get()
                    : null);
        }

        public bool Has(char arg)
        {
            return argsFound.Contains(arg);
        }

        public bool IsValid()
        {
            return valid;
        }

        private abstract class ArgumentMarshaler
        {
            public abstract void Set(string value);

            public abstract object Get();
        }

        private class BoolArgumentMarshaler : ArgumentMarshaler
        {
            private bool boolValue;

            public override void Set(string value)
            {
                boolValue = true;
            }

            public override object Get()
            {
                return boolValue;
            }
        }

        private class StringArgumentMarshaler : ArgumentMarshaler
        {
            private string stringValue;

            public override void Set(string value)
            {
                stringValue = value;
            }

            public override object Get()
            {
                return stringValue;
            }
        }

        private class IntArgumentMarshaler : ArgumentMarshaler
        {
            private int intValue;

            public override void Set(string value)
            {
                try
                {
                    intValue = Int32.Parse(value);
                }
                catch (FormatException)
                {
                    throw new ArgsException();
                }
            }

            public override object Get()
            {
                return intValue;
            }
        }
    }

    class ArgsException : Exception
    {
    }
}
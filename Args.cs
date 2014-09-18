using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Args
    {
        private readonly string schema;
        private readonly string[] args;
        private readonly IEnumerator<string> currentArgument;

        private readonly IDictionary<char, IArgumentMarshaler> marshalers =
            new Dictionary<char, IArgumentMarshaler>();

        private readonly HashSet<char> argsFound = new HashSet<char>();

        public Args(string schema, string[] args)
        {
            this.schema = schema;
            this.args = args;
            currentArgument = args.AsEnumerable().GetEnumerator();
            Parse();
        }

        private void Parse()
        {
            if (schema.Length == 0 && args.Length == 0)
                return;

            ParseSchema();
            ParseArguments();
        }

        private void ParseSchema()
        {
            foreach (var element in schema.Split(','))
            {
                if (element.Length > 0)
                {
                    var trimmedElement = element.Trim();
                    ParseSchemaElement(trimmedElement);
                }
            }
        }

        private void ParseSchemaElement(string element)
        {
            var elementId = element[0];
            var elementTail = element.Substring(1);
            ValidateSchemaElementId(elementId);
            if (IsBooleanSchemaElement(elementTail))
                marshalers[elementId] = new BoolArgumentMarshaler();
            else if (IsStringSchemaElement(elementTail))
                marshalers[elementId] = new StringArgumentMarshaler();
            else if (IsIntegerSchemaElement(elementTail))
                marshalers[elementId] = new IntArgumentMarshaler();
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

        private void ParseArguments()
        {
            while (currentArgument.MoveNext())
            {
                var arg = currentArgument.Current;
                ParseArgument(arg);
            }
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
                throw new ArgsException(argChar, ErrorCode.UnexpectedArgument);
        }

        private bool SetArgument(char argChar)
        {
            var m = GetMarshaler(argChar);
            if (m == null)
                return false;

            try
            {
                m.Set(currentArgument);
            }
            catch (ArgsException e)
            {
                e.ErrorArgumentId = argChar;
                throw;
            }

            return true;
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

        public string GetString(char arg)
        {
            var m = GetMarshaler(arg);

            try
            {
                return m != null ? (string)m.Get() : "";
            }
            catch (InvalidCastException)
            {
                throw new ArgsException();
            }
        }

        public int GetInt(char arg)
        {
            var m = GetMarshaler(arg);

            try
            {
                return m != null ? (int)m.Get() : 0;
            }
            catch (InvalidCastException)
            {
                throw new ArgsException();
            }
        }

        public bool GetBoolean(char arg)
        {
            var m = GetMarshaler(arg);

            try
            {
                return m != null && (bool)m.Get();
            }
            catch (InvalidCastException)
            {
                throw new ArgsException();
            }
        }

        private IArgumentMarshaler GetMarshaler(char arg)
        {
            IArgumentMarshaler m;
            return !marshalers.TryGetValue(arg, out m) ? null : m;
        }

        public bool Has(char arg)
        {
            return argsFound.Contains(arg);
        }
    }
}
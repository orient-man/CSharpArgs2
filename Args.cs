﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Args
    {
        private static readonly IDictionary<string, Func<IArgumentMarshaler>> AvailableMarshalers =
            new Dictionary<string, Func<IArgumentMarshaler>>
            {
                { "", () => new BoolArgumentMarshaler() },
                { "*", () => new StringArgumentMarshaler() },
                { "#", () => new IntArgumentMarshaler() },
                { "##", () => new DoubleArgumentMarshaler() }
            };

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
            Func<IArgumentMarshaler> marshalerFactory;
            if (!AvailableMarshalers.TryGetValue(elementTail, out marshalerFactory))
                throw new ArgsException(
                    ErrorCode.InvalidArgumentFormat,
                    elementId,
                    elementTail);

            marshalers[elementId] = marshalerFactory();
        }

        private static void ValidateSchemaElementId(char elementId)
        {
            if (!char.IsLetter(elementId))
                throw new ArgsException(ErrorCode.InvalidArgumentName, elementId);
        }

        private void ParseArguments()
        {
            while (currentArgument.MoveNext())
                ParseArgument(currentArgument.Current);
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
                throw new ArgsException(ErrorCode.UnexpectedArgument, argChar);
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
            return GetMarshaler(arg).GetValueOrDefault("");
        }

        public int GetInt(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(0);
        }

        public bool GetBoolean(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(false);
        }

        public double GetDouble(char arg)
        {
            return GetMarshaler(arg).GetValueOrDefault(0.0);
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
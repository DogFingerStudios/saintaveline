using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public abstract class Token
    {
        public abstract object Evaluate(Miniscript vm);
    }

    public class KeywordToken : Token
    {
        public string Value { get; }
        public KeywordToken(string value)
        {
            Value = value;
        }

        public override object Evaluate(Miniscript vm)
        {
            throw new Exception("Cannot evaluate a keyword token directly.");
        }
    }

    public class SpecialVariableToken : Token
    {
        public string Value { get; }
        public SpecialVariableToken(string value)
        {
            Value = value;
        }

        public override object Evaluate(Miniscript vm)
        {
            if (vm.SpecialVariables.TryGetValue(Value, out var val))
            {
                return val;
            }

            throw new Exception($"Special variable '{Value}' not found.");
        }
    }

    public class StringLiteralToken : Token
    {
        public string Value { get; }
        public StringLiteralToken(string value)
        {
            Value = value;
        }

        public override object Evaluate(Miniscript vm)
        {
            return Value;
        }
    }

    public class NumberLiteralToken : Token
    {
        public double Value { get; }
        public NumberLiteralToken(double value)
        {
            Value = value;
        }

        public override object Evaluate(Miniscript vm)
        {
            return Value;
        }
    }
}

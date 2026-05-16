using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public abstract class Token;
    public class CommandToken : Token
    {
        public string Value { get; }
        public CommandToken(string value)
        {
            Value = value;
        }

        public virtual int Run(Miniscript vm)
        {
            return 0;
        }
    }

    public class PushCommandToken : CommandToken
    {
        public PushCommandToken() : base("push") { }
        public override int Run(Miniscript vm)
        {
            if (vm.OpTokens.Count >= vm.CurrentIndex + 1)
            {
                Token nextToken = vm.OpTokens[vm.CurrentIndex + 1];
                vm.Stack.Push(nextToken);
                return 1;
            }

            return 0;
        }
    }

    public class CallCommandToken : CommandToken
    {
        public CallCommandToken() : base("call") { }
        public override int Run(Miniscript vm)
        {
            if (vm.OpTokens.Count >= vm.CurrentIndex + 1)
            {
                Token nextToken = vm.OpTokens[vm.CurrentIndex + 1];
                if (nextToken is StringLiteralToken strToken)
                {
                    CallRunner.Run(strToken.Value, vm);
                    return 1;
                }
            }

            return 0;
        }
    }

    public class VariableToken : Token
    {
        public string Value { get; }
        public VariableToken(string value)
        {
            Value = value;
        }
    }

    public class StringLiteralToken : Token
    {
        public string Value { get; }
        public StringLiteralToken(string value)
        {
            Value = value;
        }
    }

    public class NumberLiteralToken : Token
    {
        public double Value { get; }
        public NumberLiteralToken(double value)
        {
            Value = value;
        }
    }
}

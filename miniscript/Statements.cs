using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public abstract class Statement
    {
        public abstract void Run(Miniscript vm);
    }

    public class PushStatement : Statement
    {
        public Token Value { get; }
        public PushStatement(Token value)
        {
            Value = value;
        }
        public override void Run(Miniscript vm)
        {
            vm.Stack.Push(Value);
        }
    }

    public class CallStatement : Statement
    {
        public Token FunctionName { get; }
        public CallStatement(Token functionName)
        {
           
            FunctionName = functionName;
        }
        public override void Run(Miniscript vm)
        {
            Console.WriteLine("Calling function: " + FunctionName.GetType().Name);
        }
    }

    public class EchoStatement : Statement
    {
        public Token Value { get; }
        public EchoStatement(Token value)
        {
            Value = value;
        }

        public override void Run(Miniscript vm)
        {
            Console.WriteLine("Calling echo: " + Value.GetType().Name);
        }
    }
}

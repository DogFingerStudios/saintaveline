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
            if (FunctionName == null)
            {
                throw new Exception("Function name is not initialized.");
            }

            string eval = (string)FunctionName.Evaluate(vm);
            if (eval == null)
            {
                throw new Exception("Function name evaluated to null.");
            }

            if (!vm.FunctionMap.ContainsKey(eval))
            {
                throw new Exception($"Function '{eval}' not found.");
            }

            if (!vm.FunctionMap.TryGetValue(eval, out var methodInfo))
            {
                throw new Exception($"Function '{eval}' not found in function map.");
            }


            MiniscriptFunctionAttribute? attribute = methodInfo.GetCustomAttribute<MiniscriptFunctionAttribute>();



            methodInfo.Invoke(vm.Implementation, null);
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

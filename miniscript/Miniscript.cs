using System.Text;
using System.Reflection;

namespace Miniscript
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MiniscriptFunctionAttribute : Attribute
    {
        public string Name { get; }
        public int ParamCount { get; }
        public MiniscriptFunctionAttribute(string name, int paramCount)
        {
            Name = name;
            ParamCount = paramCount;
        }
    }

    public class Miniscript
    {
        // this is the list of statements to comprise of the functionality of the script
        public List<Statement> OpStatements { get; } = new();

        // this is the current index of the statement being executed
        public int CurrentIndex { get; private set; } = 0;

        // this is the stack used to push and pop values during execution
        public Stack<Token> Stack = new();
        
        public object? Implementation { get; set; }

        public Miniscript(List<Statement> tokens, object? implementation = null)
        {
            OpStatements = tokens;
            Implementation = implementation;
        }

        public void Run()
        {
            for (CurrentIndex = 0; CurrentIndex < OpStatements.Count; CurrentIndex++)
            {
                var statement = OpStatements[CurrentIndex];
                statement.Run(this);
            }

            if (Stack.Count > 0)
            {
                // warning: the script finished executing but there are still items left on the stack. This could indicate a problem with the script or the implementation of the commands. 
                Stack.Clear();
            }
        }
    }
}

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
        public Dictionary<string, MethodInfo> FunctionMap = new();

        public Dictionary<string, object> SpecialVariables = new();

        public Miniscript(List<Statement> tokens, object? implementation = null)
        {
            OpStatements = tokens;
            Implementation = implementation;

            if (Implementation != null)
            {
                Type implementationType = Implementation.GetType();

                foreach (MethodInfo method in 
                    implementationType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    MiniscriptFunctionAttribute? attribute =
                        method.GetCustomAttribute<MiniscriptFunctionAttribute>();

                    if (attribute != null)
                    {
                        FunctionMap.Add(attribute.Name, method);
                        Console.WriteLine(
                            $"Found Miniscript function: Method={method.Name}, " +
                            $"Name={attribute.Name}, ParamCount={attribute.ParamCount}");
                    }
                }
            }
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

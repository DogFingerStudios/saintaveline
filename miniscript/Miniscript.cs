using System.Text;

namespace Miniscript
{
    public class CallRunner
    {
        public static void Run(string functionName, Miniscript vm)
        {

        }
    }


    public class Miniscript
    {
        public List<Token> OpTokens { get; } = new();
        public Stack<Token> Stack = new();
        public int CurrentIndex { get; private set; } = 0;

        public Miniscript(List<Token> token)
        {
            OpTokens = token;
        }

        public void Run()
        {
            for (CurrentIndex = 0; CurrentIndex < OpTokens.Count; CurrentIndex++)
            {
                Token token = OpTokens[CurrentIndex];
                if (token is CommandToken cmd)
                {
                    CurrentIndex += cmd.Run(this);
                }
                else
                {
                    throw new Exception($"Unexpected token type: {token.GetType().Name}");
                }
            }

            if (CurrentIndex < OpTokens.Count)
            {
                throw new Exception($"Execution stopped at index {CurrentIndex} with token: {OpTokens[CurrentIndex]}");
            }

            if (Stack.Count > 0)
            {
                throw new Exception($"Execution finished but stack is not empty. Remaining items: {Stack.Count}");
            }
        }
    }
}

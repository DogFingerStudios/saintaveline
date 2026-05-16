using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public class Parser
    {
        public List<Token> Tokens { get; } = new();

        private List<string> _keywords = new List<string> { "push", "call" };
        public void Parse(List<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (token.StartsWith("$"))
                {
                    string variableName = token.Substring(1);
                    Tokens.Add(new VariableToken(variableName));
                }
                else if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    string stringValue = token.Substring(1, token.Length - 2);
                    Tokens.Add(new StringLiteralToken(stringValue));
                }
                else if (double.TryParse(token, out double numberValue))
                {
                    Tokens.Add(new NumberLiteralToken(numberValue));
                }
                else if (_keywords.Contains(token))
                {
                    if (token == "push")
                    {
                        Tokens.Add(new PushCommandToken());
                    }
                    else if (token == "call")
                    {
                        Tokens.Add(new CallCommandToken());
                    }
                    else
                    {
                        throw new Exception($"Unrecognized command: {token}");
                    }
                }
                else
                {
                    throw new Exception($"Unrecognized token: {token}");
                }
            }
        }
    }
}

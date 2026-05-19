using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public class Parser
    {
        public List<Statement> Statements = new();

        private List<string> _keywords = new List<string> { "push", "call", "echo" };
        public void Parse(List<Token> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if (token is not KeywordToken)
                {
                    throw new Exception($"Unrecognized token: {token}");
                }

                KeywordToken keywordToken = (KeywordToken)token;
                if (!_keywords.Contains(keywordToken.Value))
                {
                    throw new Exception($"Unrecognized command: {keywordToken.Value}");
                }

                if (keywordToken.Value == "push")
                {
                    if (tokens.Count <= i + 1)
                    {
                        throw new Exception("Expected argument after 'push' command.");
                    }

                    Statements.Add(new PushStatement(tokens[i + 1]));
                    i++;
                }
                else if (keywordToken.Value == "call")
                {
                    if (tokens.Count <= i + 1)
                    {
                        throw new Exception("Expected argument after 'push' command.");
                    }

                    if (tokens[i+1] is not StringLiteralToken)
                    {
                        throw new Exception("Expected string literal after 'call' command.");
                    }

                    Statements.Add(new CallStatement(tokens[i + 1]));
                    i++;
                }
                else if (keywordToken.Value == "echo")
                {
                    if (tokens.Count <= i + 1)
                    {
                        throw new Exception("Expected argument after 'echo' command.");
                    }

                    Statements.Add(new EchoStatement(tokens[i + 1]));
                    i++;
                }
            }
        }
    }
}

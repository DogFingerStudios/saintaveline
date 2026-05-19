using System.Text;

namespace Miniscript
{
    public class Scanner
    {
        public string Name = "Scanner";
        public List<Token> Tokens { get; private set; } = new();

        public void Scan(TextReader reader)
        {
            while (reader.Peek() != -1)
            {
                char ch = (char)reader.Peek();
                if (char.IsWhiteSpace(ch))
                {
                    reader.Read();
                    continue;
                }
                else if (ch == '@')
                {
                    ScanSpecialVariable(reader);
                }
                else if (ch == '"')
                {
                    ScanStringLiteral(reader);
                }
                else if (char.IsDigit(ch) || ch == '-' || ch == '+')
                {
                    ScanNumberLiteral(reader);
                }
                else
                {
                    ScanKeyword(reader);
                }

                reader.Read();
            }
        }

        private void ScanSpecialVariable(TextReader reader)
        {
            StringBuilder accum = new StringBuilder();
            reader.Read(); // skip the '@'

            while (reader.Peek() != -1)
            {
                char ch = (char)reader.Peek();
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                {
                    break;
                }

                accum.Append((char)reader.Read());
            }

            Tokens.Add(new SpecialVariableToken(accum.ToString()));
        }

        private void ScanStringLiteral(TextReader reader)
        {
            StringBuilder accum = new StringBuilder();
            reader.Read(); // skip the opening '"'
            while (reader.Peek() != -1)
            {
                char ch = (char)reader.Read();
                if (ch == '"')
                {
                    break; // end of string literal
                }

                accum.Append(ch);
            }

            Tokens.Add(new StringLiteralToken(accum.ToString()));
        }

        private void ScanNumberLiteral(TextReader reader)
        {
            StringBuilder accum = new StringBuilder();
            accum.Append((char)reader.Read());
            while (reader.Peek() != -1)
            {
                char ch = (char)reader.Peek();
                if (!char.IsDigit(ch) && ch != '.')
                {
                    break;
                }

                accum.Append((char)reader.Read());
            }

            if (double.TryParse(accum.ToString(), out double numberValue))
            {
                Tokens.Add(new NumberLiteralToken(numberValue));
            }
            else
            {
                throw new Exception($"Invalid number literal: {accum}");
            }
        }

        private void ScanKeyword(TextReader reader)
        {
            StringBuilder accum = new StringBuilder();
            while (reader.Peek() != -1)
            {
                char ch = (char)reader.Peek();
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                {
                    break;
                }
                accum.Append((char)reader.Read());
            }
            
            Tokens.Add(new KeywordToken(accum.ToString()));
        }
    }

} // namespace Miniscript

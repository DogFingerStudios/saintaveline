using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miniscript
{
    public class Scanner
    {
        List<string> _items = new List<string>();
        public List<string> Items => _items;

        public void Scan(TextReader text)
        {
            StringBuilder accum = new StringBuilder();

            while (text.Peek() != -1)
            {
                char ch = (char)text.Peek();
                if (char.IsWhiteSpace(ch))
                {
                    if (accum.Length > 0)
                    {
                        _items.Add(accum.ToString());
                        accum.Clear();
                    }
                }
                else
                {
                    accum.Append(ch);
                }

                text.Read();
            }

            if (accum.Length > 0)
            {
                _items.Add(accum.ToString());
            }
        }
    }
}

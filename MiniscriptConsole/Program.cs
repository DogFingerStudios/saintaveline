using System;
using System.Text;
using System.Collections.Generic;
using Miniscript;

//push $target
//push "comfort"
//push  -0.1
//call "adjust_npc_attribute"

// push, call = keywords (strings)
// $target = variable
// "comfort" = string literal
// -0.1 = number literal

namespace MiniscriptConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string data = """
push $target
push "comfort"
push  -0.1
call "adjust_npc_attribute"

push $player
push "health"
call "get_attribute"

""";
            Scanner scanner = new Scanner();
            using (TextReader sr = new StringReader(data))
            {
                scanner.Scan(sr);
            }

            foreach (var item in scanner.Items)
            {
                Console.WriteLine($"Scanned Item: {item}");
            }

            var items = scanner.Items;

            Parser parser = new Parser();
            parser.Parse(items);

            for (var idx = 0; idx < parser.Tokens.Count; idx++)
            {
                Token token = parser.Tokens[idx];
                Console.Write($"Parsed Token {idx}: " + token.GetType().Name);
            }

            Miniscript.Miniscript vm = new Miniscript.Miniscript(parser.Tokens);
            vm.Run();

            Console.WriteLine("Hello World!");
        }
    }
}
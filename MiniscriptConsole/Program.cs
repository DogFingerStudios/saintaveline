using Miniscript;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
    public class ConsoleImplmentor
    {
        [MiniscriptFunction("adjust_npc_attribute", 3)]
        public void AdjustNpcAttribute(string arg1, string arg2, double arg3)
        {
            Console.WriteLine("arg1: " + arg1 + " arg2: " + arg2 + " arg3: " + arg3);
        }

        [MiniscriptFunction("echo", 1)]
        public void Echo(object arg)
        {
            Console.WriteLine($"Echo: {arg} (Type: {arg.GetType().Name})");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string data1 = "Matthew";

            string data = """
push "matthew"
push "desperation"
push +100
call "adjust_npc_attribute"

push @target
push "comfort"
push  -0.1
echo @target
call "adjust_npc_attribute"
""";
            Scanner scanner = new Scanner();
            using (TextReader sr = new StringReader(data))
            {
                scanner.Scan(sr);
            }

            foreach (var item in scanner.Tokens)
            {
                Console.WriteLine($"Scanned Item: {item}");
            }

            Parser parser = new Parser();
            parser.Parse(scanner.Tokens);

            foreach (var statement in parser.Statements)
            {
                Console.WriteLine($"Parsed Statement: {statement}");
            }

            ConsoleImplmentor implementor = new();
            Miniscript.Miniscript vm = new(parser.Statements, implementor);
            vm.SpecialVariables["target"] = data1;
            vm.Run();

            Console.WriteLine("End of Program!");
        }
    }
}
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
    //public class Implementor
    //{
    //    [MiniscriptFunction("adjust_npc_attribute", 3)]
    //    public void AdjustCharacterAttribute(params object[] args)
    //    {
    //        if (args.Length != 3)
    //        {
    //            throw new ArgumentException($"Expected 3 arguments, got {args.Length}");
    //        }

    //        foreach(var arg in args)
    //        {
    //            Console.WriteLine($"Argument: {arg} (Type: {arg.GetType().Name})");
    //        }
    //    }
    //}

    public class ConsoleImplmentor
    {

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string data = """
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

            //Implementor implementor = new();
            ConsoleImplmentor implementor = new();
            Miniscript.Miniscript vm = new(parser.Statements, implementor);
            vm.Run();

            Console.WriteLine("End of Program!");
        }
    }
}
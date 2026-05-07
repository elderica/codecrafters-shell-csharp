using System.Runtime.InteropServices;

class Program
{
    static bool Eval(string command)
    {
        var parsed = command.Split(" ");
        var cmd = parsed.First();
        if (cmd == "exit")
        {
            return false;
        }
        else if (cmd == "echo")
        {
            Console.WriteLine(command[5..]);
            return true;
        }
        else if (cmd == "type")
        {
            var builtins = new string[]
            {
                "exit",
                "echo",
                "type",  
            };
            if (builtins.Contains(parsed[1]))
            {
                Console.WriteLine($"{parsed[1]} is a shell builtin");
            } else
            {
                Console.WriteLine($"{parsed[1]}: not found");
            }
            return true;
        }
        else
        {
            Console.WriteLine($"{cmd}: command not found");
            return true;
        }
    }
    static void Main()
    {
        bool continueq = true;
        while (continueq)
        {
            Console.Write("$ ");
            var command = Console.ReadLine() ?? "";
            if (command != "")
            {
                continueq = Eval(command);
            }
        }

    }
}

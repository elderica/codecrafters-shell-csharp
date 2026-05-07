class Program
{
    static bool Eval(string command)
    {
        if (command == "exit")
        {
            return false;
        } else if (command.StartsWith("echo "))
        {
            Console.WriteLine(command[5..]);
            return true;
        } else
        {
            Console.WriteLine($"{command}: command not found");
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
            continueq = Eval(command);
        }

    }
}

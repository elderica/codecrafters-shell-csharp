class Program
{
    static bool Eval(string command)
    {
        switch(command)
        {
            case "exit":
                return false;
            default:
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


using System.Diagnostics;

class Program
{
    static FileInfo? SearchExecutable(string cmd)
    {
        var envpath = Environment.GetEnvironmentVariable("PATH") ?? "";

        string[]? paths;
        if (OperatingSystem.IsWindows())
        {
            paths = envpath.Split(";");
        }
        else
        {
            paths = envpath.Split(":");
        }

        foreach (var path in paths)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) continue;

            foreach (var file in dir.EnumerateFiles())
            {
                if (OperatingSystem.IsWindows())
                {
                    var basename = Path.GetFileNameWithoutExtension(file.FullName);

                    var ext = file.Extension;

                    if (basename == cmd && ext == ".exe")
                    {
                        return file;
                    }
                }
                else
                {
                    var filemode = File.GetUnixFileMode(file.FullName);
                    var execbits = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
                    if (file.Name == cmd && (filemode & execbits) != 0)
                    {
                        return file;
                    }
                }
            }
        }

        return null;
    }

    static bool Eval(string command)
    {
        var parsed = command.Split(" ") ?? [];
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

            var typecmd = parsed[1];
            if (builtins.Contains(typecmd))
            {
                Console.WriteLine($"{typecmd} is a shell builtin");
            }
            else
            {
                var file = SearchExecutable(typecmd);
                if (file == null)
                {
                    Console.WriteLine($"{typecmd}: not found");
                }
                else
                {
                    Console.WriteLine($"{typecmd} is {file}");
                }

            }
            return true;
        }
        else
        {
            var file = SearchExecutable(cmd);
            if (file == null)
            {
                Console.WriteLine($"{cmd}: command not found");
            } else
            {
                using var process = new Process();
                process.StartInfo.FileName = file.FullName;
                process.StartInfo.Arguments = string.Join(" ", parsed[1..]);
                process.Start();
            }
            
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

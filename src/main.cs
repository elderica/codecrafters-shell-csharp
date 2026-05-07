
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
            if (path == "") continue;
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

    enum LexerState { NORMAL, SINGLE_QUOTE, DOUBLE_QUOTE, ESCAPE }

    static string[] Lex(string command)
    {
        var state = LexerState.NORMAL;
        var current = "";
        List<string> words = [];

        for (var i = 0; i < command.Length; i++)
        {
            var c = command[i];
            //Console.WriteLine($"c:{c} state:{state} current:{current}");
            
            switch (state)
            {
                case LexerState.NORMAL:
                    switch (c)
                    {
                        case ' ':
                            if (current != "")
                            {
                                words.Add(current);
                                current = "";
                            }
                            break;
                        case '\'':
                            state = LexerState.SINGLE_QUOTE;
                            break;
                        case '"':
                            state = LexerState.DOUBLE_QUOTE;
                            break;
                        case '\\':
                            state = LexerState.ESCAPE;
                            break;
                        default:
                            current += c;
                            break;
                    }
                    break;
                case LexerState.SINGLE_QUOTE:
                    if (c == '\'')
                    {
                        state = LexerState.NORMAL;
                    } else
                    {
                        current += c;
                    }
                    break;
                case LexerState.DOUBLE_QUOTE:
                    if (c == '"')
                    {
                        state = LexerState.NORMAL;
                    } else if (c == '\\'  && i+1 < command.Length && "\"\\$`".Contains(command[i+1]))
                    {
                        current += command[i+1];
                        i++;
                    } else
                    {
                        current += c;
                    }
                    break;
                case LexerState.ESCAPE:
                    current += c;
                    state = LexerState.NORMAL;
                    break;
            }
        }

        if (current.Length > 0)
        {
            words.Add(current);
        }
        return [.. words];
    }

    static bool Eval(string command)
    {
        var parsed = Lex(command);
        if (parsed.Length == 0) return true;
        var cmd = parsed.First();
        if (cmd == "exit")
        {
            return false;
        }
        else if (cmd == "echo")
        {
            Console.WriteLine(string.Join(' ', parsed[1..]));
            return true;
        }
        else if (cmd == "type")
        {
            var builtins = new string[]
            {
                "exit",
                "echo",
                "type",
                "pwd",
                "cd",
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
        else if (cmd == "pwd")
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            return true;
        }
        else if (cmd == "cd")
        {
            var dirparam = parsed[1];

            if (dirparam == "~")
            {
                var homepath = Environment.GetEnvironmentVariable("HOME");
                if (homepath == null) return true;
                dirparam = homepath;
            }

            var dirpath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dirparam));
            var dir = new DirectoryInfo(dirpath);
            if (!dir.Exists)
            {
                Console.WriteLine($"cd: {dir.FullName}: No such file or directory");
                return true;
            }
            Directory.SetCurrentDirectory(dir.FullName);
            return true;
        }
        else
        {
            var file = SearchExecutable(cmd);
            if (file == null)
            {
                Console.WriteLine($"{cmd}: command not found");
            }
            else
            {
                using var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                if (OperatingSystem.IsWindows())
                {
                    process.StartInfo.FileName = file.FullName;
                    process.StartInfo.Arguments = string.Join(" ", parsed[1..]);
                }
                else
                {
                    process.StartInfo.FileName = "/bin/sh";
                    process.StartInfo.Arguments = $"-c \"{string.Join(" ", parsed)}\"";
                }
                process.Start();
                process.WaitForExit();
                Console.Write(process.StandardOutput.ReadToEnd());
                Console.Error.Write(process.StandardError.ReadToEnd());
            }

            return true;
        }
    }
    static void Main()
    {
        bool continueq = true;
        do
        {
            Console.Write("$ ");
            var command = Console.ReadLine() ?? "";
            if (command != "")
            {
                continueq = Eval(command);
            }
        } while (continueq);
    }
}

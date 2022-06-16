using System.Collections.Immutable;
using System.Text;
using BashWrapper.Commands;
using BashWrapper.Outputs;
using static System.Enum;

namespace BashWrapper;

public class BashHandler
{
    private readonly IOutput _outputMethod;
    private readonly Dictionary<string, string> _assignedVariables;
    private bool _running;
    private readonly StringBuilder _buffer;

    public BashHandler(IOutput outputMethod)
    {
        _outputMethod = outputMethod;
        _assignedVariables = new Dictionary<string, string>
        {
            ["$?"] = ExitStatus.True.ToString()
        };
        _running = true;
        _buffer = new StringBuilder();
    }

    public void AnalyzeRequests()
    {
        while (_running)
        {
            try
            {
                var inputCommand = Console.ReadLine();
                if (string.IsNullOrEmpty(inputCommand)) continue;
                var parsedCommand = CommandParser.Parse(inputCommand);
                var groupedCommands = CommandParser.SplitArgsByConnectors(parsedCommand);
                groupedCommands.ForEach(x =>
                {
                    x.ForEach(Console.WriteLine);
                    Console.WriteLine("----");
                });
                RunCommands(groupedCommands);
                _outputMethod.Output(_buffer.ToString());
                _buffer.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private void RunCommands(ImmutableList<ImmutableList<string>> groupedCommands)
    {
        foreach (var command in groupedCommands)
        {
            try
            {
                var (mainArg, commandArgs) = CommandParser.ParseCommandArgs(command);
                commandArgs = CommandParser.ReplaceLocalVariables(commandArgs, _assignedVariables);
                if (mainArg == "pwd")
                {
                    var pwdCommand = new PwdCommand(commandArgs);
                    var result = (pwdCommand.Execute() as string)!;
                    _buffer.AppendLine(result);
                }
                else if (mainArg == "ls")
                {
                    var lsCommand = new LsCommand(commandArgs);
                    var result = (lsCommand.Execute() as ImmutableList<ImmutableList<string>>)!;
                    result.ForEach(x => x.ForEach(y => _buffer.AppendLine(y)));
                }
                else if (mainArg == "cat")
                {
                    var catCommand = new CatCommand(commandArgs);
                    var result = (catCommand.Execute() as ImmutableList<ImmutableList<string>>)!;
                    result.ForEach(x => x.ForEach(y => _buffer.AppendLine(y)));
                }
                else if (mainArg == "echo")
                {
                    var echoCommand = new EchoCommand(commandArgs);
                    var result = (echoCommand.Execute() as string)!;
                    _buffer.AppendLine(result);
                }
                else if (mainArg == "&&")
                {
                    if (GetExitStatus() != ExitStatus.True) break;
                }
                else if (mainArg == "||")
                {
                    if (GetExitStatus() != ExitStatus.False) break;
                }
                /* that `if` block can be deleted,
                     it is written in order to show that it implements the functionality */
                else if (mainArg == ";")
                {
                    UpdateExitStatus(ExitStatus.True);
                    continue;
                }
                else if (mainArg == "exit")
                {
                    var exitCommand = new ExitCommand(commandArgs);
                    var exitCode = exitCommand.Execute() as string;
                    UpdateExitStatus(exitCode == "0" ? ExitStatus.True : ExitStatus.False);
                    _running = false;
                    break;
                }
                else if (mainArg == "true")
                {
                    UpdateExitStatus(ExitStatus.True);
                }
                else if (mainArg == "false")
                {
                    UpdateExitStatus(ExitStatus.False);
                }
                else if (mainArg.First() == '$')
                {
                    var splitExpression = mainArg.Split("=");
                    var (variable, value) = (splitExpression[0], splitExpression[1]);
                    if (!_assignedVariables.ContainsKey(variable)) _assignedVariables.Add(variable, value);
                    _assignedVariables[variable] = value;
                }

                UpdateExitStatus(ExitStatus.True);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                UpdateExitStatus(ExitStatus.False);
            }
        }
    }

    private void UpdateExitStatus(ExitStatus exitStatus)
    {
        _assignedVariables["$?"] = exitStatus.ToString();
    }

    private ExitStatus GetExitStatus()
    {
        var unused = TryParse(_assignedVariables["$?"], out ExitStatus status);
        return status;
    }
}
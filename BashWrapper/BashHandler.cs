using System.Collections.Immutable;
using BashWrapper.Commands;
using BashWrapper.Outputs;

namespace BashWrapper;

public class BashHandler
{
    private readonly IOutput _outputMethod;
    private readonly Dictionary<string, string> _assignedVariables;
    private ExitStatus _lastCommandExitStatus;

    public BashHandler(IOutput outputMethod)
    {
        _outputMethod = outputMethod;
        _lastCommandExitStatus = ExitStatus.True;
        _assignedVariables = new Dictionary<string, string>();
    }

    public void AnalyzeRequests()
    {
        while (true)
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
                _lastCommandExitStatus = ExitStatus.True;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _lastCommandExitStatus = ExitStatus.False;
            }
        }
    }

    private void RunCommands(ImmutableList<ImmutableList<string>> groupedCommands)
    {
        groupedCommands.ForEach(command =>
        {
            var (mainArg, commandArgs) = CommandParser.ParseCommandArgs(command);
            commandArgs = CommandParser.ReplaceLocalVariables(commandArgs, _assignedVariables);
            if (mainArg == "pwd")
            {   
                var pwdCommand = new PwdCommand(commandArgs);
                var result = (pwdCommand.Execute() as string)!;
                _outputMethod.Output(result);
            }
            else if (mainArg == "ls")
            {
                var lsCommand = new LsCommand(commandArgs);
                var result = (lsCommand.Execute() as ImmutableList<ImmutableList<string>>)!;
                result.ForEach(x => x.ForEach(y => _outputMethod.Output(y)));
            }
            else if (mainArg == "cat")
            {
                var catCommand = new CatCommand(commandArgs);
                var result = (catCommand.Execute() as ImmutableList<ImmutableList<string>>)!;
                result.ForEach(x => x.ForEach(y => _outputMethod.Output(y)));
            }
            else if (mainArg == "echo")
            {
                var echoCommand = new EchoCommand(commandArgs);
                var result = (echoCommand.Execute() as string)!;
                _outputMethod.Output(result);
            }
            else if (mainArg == "$?")
            {
                _outputMethod.Output(_lastCommandExitStatus.ToString());
            }
            else if (mainArg == "&&")
            {
                // logic here
                _outputMethod.Output("and");
            }
            else if (mainArg == "||")
            {
                // logic here
                _outputMethod.Output("or");
            }
            else if (mainArg.First() == '$')
            {
                var splitExpression = mainArg.Split("=");
                var (variable, value) = (splitExpression[0][1..], splitExpression[1]);
                _assignedVariables.Add(variable, value);
            }
        });
    }
}
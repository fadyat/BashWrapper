using System.Collections.Immutable;
using BashWrapper.Commands;
using BashWrapper.Outputs;

namespace BashWrapper;

public class BashHandler
{
    private readonly IOutput _outputMethod;

    public BashHandler(IOutput outputMethod)
    {
        _outputMethod = outputMethod;
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
                var groupedCommands = CommandParser.SplitArgsByMainCommands(parsedCommand);
                RunCommands(groupedCommands);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private void RunCommands(ImmutableList<ImmutableList<string>> groupedCommands)
    {
        groupedCommands.ForEach(command =>
        {
            var mainArg = command.First();
            var commandArgs = command.GetRange(1, command.Count - 1);
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
        });
    }
}
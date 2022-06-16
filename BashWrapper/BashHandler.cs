using System.Collections.Immutable;
using System.Text;
using BashWrapper.Commands;
using BashWrapper.Commands.RedirectCommands;
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
                var groupedCommands = CommandParser.SplitArgsByConnectorsAndRedirectors(parsedCommand);
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
        var runningCommand = true;
        foreach (var command in groupedCommands)
        {
            try
            {
                if (!_running || !runningCommand) break;

                var (mainArg, commandArgs) = CommandParser.ParseCommandArgs(command);
                commandArgs = CommandParser.ReplaceLocalVariables(commandArgs, _assignedVariables);

                if (CommandParser.IsMainCommand(mainArg)) CommandAnalyzer(mainArg, commandArgs);
                else if (CommandParser.IsConnector(mainArg)) runningCommand = ConnectorAnalyzer(mainArg);
                else if (CommandParser.IsRedirector(mainArg)) RedirectorAnalyzer(mainArg, commandArgs);
                else if (CommandParser.IsVariable(mainArg)) VariableAnalyzer(mainArg);

                UpdateExitStatus(ExitStatus.True);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                UpdateExitStatus(ExitStatus.False);
            }
        }
    }

    private void CommandAnalyzer(string mainArg, ImmutableList<string> commandArgs)
    {
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
        else if (mainArg == "exit")
        {
            var exitCommand = new ExitCommand(commandArgs);
            var exitCode = exitCommand.Execute() as string;
            UpdateExitStatus(exitCode == "0" ? ExitStatus.True : ExitStatus.False);
            _running = false;
        }
        else if (mainArg == "true")
        {
            UpdateExitStatus(ExitStatus.True);
        }
        else if (mainArg == "false")
        {
            UpdateExitStatus(ExitStatus.False);
        }
    }

    private bool ConnectorAnalyzer(string mainArg)
    {
        var continueCommand = true;
        if (mainArg == "&&")
        {
            if (GetExitStatus() != ExitStatus.True) continueCommand = false;
        }
        else if (mainArg == "||")
        {
            if (GetExitStatus() != ExitStatus.False) continueCommand = false;
        }
        /* that `if` block can be deleted,
             it is written in order to show that it implements the functionality */
        else if (mainArg == ";")
        {
            UpdateExitStatus(ExitStatus.True);
            continueCommand = true;
        }

        return continueCommand;
    }

    private void RedirectorAnalyzer(string mainArg, ImmutableList<string> commandArgs)
    {
        if (mainArg == ">")
        {
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, _buffer, true);
            _ = inputRedirectCommand.Execute() as string;
            _buffer.Clear();
        }
        else if (mainArg == ">>")
        {
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, _buffer);
            _ = inputRedirectCommand.Execute() as string;
            _buffer.Clear();
        }
        else if (mainArg == "<")
        {
            //todo: write outputRedirectCommand here
        }
    }

    private void VariableAnalyzer(string mainArg)
    {
        if (mainArg.First() != '$') return;
        var splitExpression = mainArg.Split("=");
        var (variable, value) = (splitExpression[0], splitExpression[1]);
        if (!_assignedVariables.ContainsKey(variable)) _assignedVariables.Add(variable, value);
        _assignedVariables[variable] = value;
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
using System.Collections.Immutable;
using System.Text;
using BashWrapper.Commands;
using BashWrapper.Commands.LogicCommands;
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
    private readonly Stack<AbstractCommand> _commandsStack;
    private bool _toStack;

    public BashHandler(IOutput outputMethod)
    {
        _outputMethod = outputMethod;
        _assignedVariables = new Dictionary<string, string>
        {
            ["$?"] = ((int) ExitStatus.True).ToString()
        };
        _running = true;
        _buffer = new StringBuilder();
        _commandsStack = new Stack<AbstractCommand>();
        _toStack = false;
    }

    // option for ending tests w/o -- command ; exit 0
    public void AnalyzeRequests(bool endAfterOneCommand = false)
    {
        while (_running)
        {
            try
            {
                var inputCommand = Console.ReadLine();
                if (string.IsNullOrEmpty(inputCommand)) continue;
                var parsedCommand = CommandParser.Parse(inputCommand);
                var groupedCommands = CommandParser.SplitArgsByConnectorsAndRedirectors(parsedCommand);
                // groupedCommands.ForEach(x =>
                // {
                // x.ForEach(Console.WriteLine);
                // Console.WriteLine("----");
                // });
                RunCommands(groupedCommands);
                if (_buffer.Length > 0 && _buffer[^1] == '\n') _buffer.Remove(_buffer.Length - 1, 1);
                _outputMethod.Output(_buffer.ToString());
                _buffer.Clear();
                if (endAfterOneCommand) break;
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
        for (var i = 0; i < groupedCommands.Count; i++)
        {
            try
            {
                if (!_running || !runningCommand) break;

                var command = groupedCommands[i];
                var (mainArg, commandArgs) = CommandParser.ParseCommandArgs(command);
                commandArgs = CommandParser.ReplaceLocalVariables(commandArgs, _assignedVariables);
                _toStack = false;

                if (i + 1 < groupedCommands.Count && CommandParser.IsMainCommand(mainArg) && !commandArgs.Any())
                {
                    var nextCommand = groupedCommands[i + 1];
                    var (nextCommandMain, _) = CommandParser.ParseCommandArgs(nextCommand);
                    if (nextCommandMain == "<") _toStack = true;
                }

                if (CommandParser.IsMainCommand(mainArg)) CommandAnalyzer(mainArg, commandArgs);
                else if (CommandParser.IsConnector(mainArg)) runningCommand = ConnectorAnalyzer(mainArg);
                else if (CommandParser.IsRedirector(mainArg)) RedirectorAnalyzer(mainArg, commandArgs);
                else if (CommandParser.IsVariable(mainArg)) VariableAnalyzer(mainArg);

                if (mainArg != "false") _assignedVariables["$?"] = ((int) ExitStatus.True).ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _assignedVariables["$?"] = ((int) ExitStatus.False).ToString();
            }
        }
    }

    private void CommandAnalyzer(string mainArg, ImmutableList<string> commandArgs)
    {
        AbstractCommand command;
        if (mainArg == "pwd") command = new PwdCommand(commandArgs, _buffer);
        else if (mainArg == "ls") command = new LsCommand(commandArgs, _buffer);
        else if (mainArg == "cat") command = new CatCommand(commandArgs, _buffer);
        else if (mainArg == "echo") command = new EchoCommand(commandArgs, _buffer);
        else if (mainArg == "wc") command = new WcCommand(commandArgs, _buffer);
        else if (mainArg == "exit")
        {
            command = new ExitCommand(commandArgs, _buffer, _assignedVariables);
            _running = false;
        }
        else if (mainArg == "true") command = new TrueCommand(commandArgs, _buffer, _assignedVariables);
        else command = new FalseCommand(commandArgs, _buffer, _assignedVariables);

        TryToExecute(command);
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
            continueCommand = true;
        }

        return continueCommand;
    }

    private void RedirectorAnalyzer(string mainArg, ImmutableList<string> commandArgs)
    {
        if (mainArg == ">")
        {
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, _buffer, true);
            inputRedirectCommand.Execute();
            _buffer.Clear();
        }
        else if (mainArg == ">>")
        {
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, _buffer);
            inputRedirectCommand.Execute();
            _buffer.Clear();
        }
        else if (mainArg == "<")
        {
            var outputRedirectCommand = new OutputRedirectCommand(commandArgs, _buffer, _commandsStack);
            outputRedirectCommand.Execute();
            outputRedirectCommand.ToBuffer();
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

    private ExitStatus GetExitStatus()
    {
        var unused = TryParse(_assignedVariables["$?"], out ExitStatus status);
        return status;
    }

    private void TryToExecute(AbstractCommand abstractCommand)
    {
        if (!_toStack)
        {
            abstractCommand.Execute();
            abstractCommand.ToBuffer();
            return;
        }

        _commandsStack.Push(abstractCommand);
    }
}
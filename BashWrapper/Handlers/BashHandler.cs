using System.Collections.Immutable;
using System.Text;
using BashWrapper.Commands;
using BashWrapper.Commands.LogicCommands;
using BashWrapper.Commands.RedirectCommands;
using BashWrapper.Outputs;
using static System.Enum;

namespace BashWrapper.Handlers;

public class BashHandler
{
    protected readonly IOutput OutputMethod;
    private readonly Dictionary<string, string> _assignedVariables;
    protected bool Running;
    protected readonly StringBuilder Buffer;
    private readonly Stack<AbstractCommand> _commandsStack;
    private bool _toStack;

    public BashHandler(IOutput outputMethod)
    {
        OutputMethod = outputMethod;
        _assignedVariables = new Dictionary<string, string>
        {
            ["$?"] = ((int) ExitStatus.True).ToString()
        };
        Running = true;
        Buffer = new StringBuilder();
        _commandsStack = new Stack<AbstractCommand>();
        _toStack = false;
    }

    // option for ending tests w/o -- command ; exit 0
    public virtual void AnalyzeRequests()
    {
        while (Running)
        {
            try
            {
                var inputCommand = Console.ReadLine();
                if (string.IsNullOrEmpty(inputCommand)) continue;
                var parsedCommand = CommandParser.Parse(inputCommand);
                var groupedCommands = CommandParser.SplitArgsByConnectorsAndRedirectors(parsedCommand);
                RunCommands(groupedCommands);
                OutputMethod.Output(Buffer.ToString());
                Buffer.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    protected void RunCommands(ImmutableList<ImmutableList<string>> groupedCommands)
    {
        var runningCommand = true;
        for (var i = 0; i < groupedCommands.Count; i++)
        {
            try
            {
                if (!Running || !runningCommand) break;

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
        if (mainArg == "pwd") command = new PwdCommand(commandArgs, Buffer);
        else if (mainArg == "ls") command = new LsCommand(commandArgs, Buffer);
        else if (mainArg == "cat") command = new CatCommand(commandArgs, Buffer);
        else if (mainArg == "echo") command = new EchoCommand(commandArgs, Buffer);
        else if (mainArg == "wc") command = new WcCommand(commandArgs, Buffer);
        else if (mainArg == "exit")
        {
            command = new ExitCommand(commandArgs, Buffer, _assignedVariables);
            Running = false;
        }
        else if (mainArg == "true") command = new TrueCommand(commandArgs, Buffer, _assignedVariables);
        else command = new FalseCommand(commandArgs, Buffer, _assignedVariables);

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
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, Buffer, true);
            inputRedirectCommand.Execute();
            Buffer.Clear();
        }
        else if (mainArg == ">>")
        {
            var inputRedirectCommand = new InputRedirectCommand(commandArgs, Buffer);
            inputRedirectCommand.Execute();
            Buffer.Clear();
        }
        else if (mainArg == "<")
        {
            var outputRedirectCommand = new OutputRedirectCommand(commandArgs, Buffer, _commandsStack);
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
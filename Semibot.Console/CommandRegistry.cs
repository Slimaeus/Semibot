using Semibot.Console.Interfaces;

namespace Semibot.Console;

public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands;
    public int Count { get => _commands.Count; }

    public CommandRegistry()
    {
        _commands = new Dictionary<string, ICommand>();
    }

    public void RegisterCommand(ICommand command)
    {
        _commands[command.Name] = command;
    }

    public bool TryGetCommand(string commandName, out ICommand command)
    {
        return _commands.TryGetValue(commandName, out command!);
    }
}

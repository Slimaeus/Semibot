using Telegram.Bot;
using Telegram.Bot.Types;

namespace Semibot.Console.Interfaces;

public interface ICommand
{
    string Name { get; }
    Task ExecuteAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken);
}

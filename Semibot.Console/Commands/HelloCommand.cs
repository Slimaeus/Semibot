using Semibot.Console.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Semibot.Console.Commands;

public class HelloCommand : ICommand
{
    public string Name => "/hello";

    public async Task ExecuteAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Welcome to the bot!",
            cancellationToken: cancellationToken);
    }
}

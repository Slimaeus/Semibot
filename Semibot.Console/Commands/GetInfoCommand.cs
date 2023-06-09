﻿using Semibot.Console.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Semibot.Console.Commands;

public class GetInfoCommand : ICommand
{
    public string Name => "/info";

    public async Task ExecuteAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        var userName = message.Chat.FirstName + " " + message.Chat.LastName;


        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Welcome {userName} to the bot (Id: {botClient.BotId})!",
            cancellationToken: cancellationToken);
    }
}

using Microsoft.Extensions.Configuration;
using Semibot.Console;
using Semibot.Console.Interfaces;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

internal class Program
{
    private static readonly CommandRegistry CommandRegistry = new CommandRegistry();

    private static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
        Console.WriteLine(configuration.ToString());
        Console.WriteLine(configuration.GetSection("TelegramBot").Exists());
        var botClient = new TelegramBotClient(configuration["TelegramBot:AccessToken"]!);

        // ******************
        // Get the assembly where your command classes are located
        Assembly assembly = Assembly.GetExecutingAssembly(); // Change this to the appropriate assembly if needed

        // Scan the assembly for classes implementing ICommand
        Type commandInterfaceType = typeof(ICommand);
        IEnumerable<Type> commandTypes = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && commandInterfaceType.IsAssignableFrom(type));

        // Register each command type
        foreach (Type commandType in commandTypes)
        {
            ICommand? command = Activator.CreateInstance(commandType) as ICommand;
            CommandRegistry.RegisterCommand(command!);
        }

        // ******************


        using CancellationTokenSource cts = new();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();


        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (messageText.StartsWith("/"))
            {
                Console.WriteLine($"Received a '{messageText}' command in chat {chatId} of {message.AuthorSignature} ({CommandRegistry.Count} commands).");

                var commandName = messageText.Split(' ')[0]; // Extract the command name from the message text

                // Retrieve the command from the registry
                if (CommandRegistry.TryGetCommand(commandName, out var command))
                {
                    await command.ExecuteAsync(message, botClient, cancellationToken);
                    return;
                }
            }

            // Handle non-command messages here
            // ...
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);
        }


        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
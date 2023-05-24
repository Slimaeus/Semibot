using Newtonsoft.Json;
using Semibot.Console.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Semibot.Console.Commands;

public class DailyPriceCommand : ICommand
{
    public string Name => "/dailyprice";

    public async Task ExecuteAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var symbol = message.Text.Split(' ', StringSplitOptions.TrimEntries)[1];
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.polygon.io/v1/")
        };
        var response = await httpClient.GetAsync($"open-close/{symbol}/2023-01-09?adjusted=true&apiKey=hpCA6tpDFitUoIRFI_hx0qEhaiEBy91j", cancellationToken);

        var chatId = message.Chat.Id;

        var result = JsonConvert.DeserializeObject<DailyPriceResult>(await response.Content.ReadAsStringAsync());

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Symbol: {result.symbol}\nOpen: {result.open}\nClose: {result.close}",
            cancellationToken: cancellationToken);
    }

    public record DailyPriceResult
    {
        public double afterHours { get; set; }
        public double close { get; set; }
        public string from { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double preMarket { get; set; }
        public string status { get; set; }
        public string symbol { get; set; }
        public double volume { get; set; }
    }
}

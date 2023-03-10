using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using static BotTest.Service;
using static BotTest.Constants;

namespace TelegramBot
{
    class Program
    {
        private static TelegramBotClient client;
        static void Main(string[] args)
        {
            client = new TelegramBotClient(Token);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            client.StartReceiving(
              HandleUpdateAsync,
              HandleErrorAsync,
              receiverOptions,
              cancellationToken
            );
            Console.ReadKey();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handleUpdateAsynService = new HandleUpdateAsynService(botClient);
            await handleUpdateAsynService.EchoAsync(update);
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
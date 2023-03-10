using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using static BotTest.Service;

namespace TelegramBot
{
    class Program
    {
        private static string token = "5441070326:AAHYsLzF0QMYnm-6z6bS9eHPAejhG1EjeeU";
        private static TelegramBotClient client;
        static void Main(string[] args)
        {
            client = new TelegramBotClient(token);

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
            Console.ReadLine();
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
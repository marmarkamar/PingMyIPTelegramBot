using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using System.Net.NetworkInformation;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTest
{
    internal class Service
    {
        public class HandleUpdateAsynService
        {
            private readonly ITelegramBotClient _botClient;
            public HandleUpdateAsynService(ITelegramBotClient botClient)
            {
                _botClient = botClient;
            }

            public async Task EchoAsync(Update update)
            {
                var handler = update.Type switch
                {
                    UpdateType.Message => BotOnMessageReceived(update.Message!),
                    UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
                    _ => UnknownUpdateHandlerAsync(update)
                };

                try
                {
                    await handler;
                }
                catch (Exception exception)
                {
                    await HandleErrorAsync(exception);
                }
                
            }

            private Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
            {
                if (callbackQuery == null || callbackQuery.Message == null || string.IsNullOrEmpty(callbackQuery.Message.Text))
                    return Task.CompletedTask;
                var action = callbackQuery.Data switch
                {
                    "My" => PingHost(_botClient, callbackQuery.Message),
                    "Not_My_light" => PingNotMyHost(callbackQuery.Message),
                    _ => SetIp(_botClient, callbackQuery.Message)
                };
                return Task.CompletedTask;
            }

            private Task BotOnMessageReceived(Message message)
            {
                if (message.Type != MessageType.Text)
                    return Task.CompletedTask;
                var action = message.Text!.Split(' ')[0] switch
                {
                    "/start" => SendInlineKeyboard(_botClient, message),
                    _ => SetIp(_botClient, message)
                };
                return Task.CompletedTask;
            }

            static async Task<Message> SendMessage(ITelegramBotClient bot, Message message, string text) => await bot.SendTextMessageAsync(message.Chat, text);

            static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500);

                InlineKeyboardMarkup inlineKeyboard = new(
                  new[]
                  {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("My Light", "My"),
                        InlineKeyboardButton.WithCallbackData("Not My light", "Not_My_light")
                    }
                  });

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                    text: "What do u want",
                                    replyMarkup: inlineKeyboard);
            }

            static async Task SetIp(ITelegramBotClient bot, Message message)
            {
                if (string.IsNullOrEmpty(message.Text) || !ValidateIPv4(message.Text))
                {
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                    text: "it's not ip",
                                    replyMarkup: new ReplyKeyboardRemove());
                    return;
                }
                await PingHost(bot, message, message.Text);
            }

            private static bool ValidateIPv4(string ipString)
            {
                if (String.IsNullOrWhiteSpace(ipString))
                {
                    return false;
                }

                string[] splitValues = ipString.Split('.');
                if (splitValues.Length != 4)
                {
                    return false;
                }

                byte tempForParsing;

                return splitValues.All(r => byte.TryParse(r, out tempForParsing));
            }

            private Task UnknownUpdateHandlerAsync(Update update)
            {
                return Task.CompletedTask;
            }

            public Task HandleErrorAsync(Exception exception)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                return Task.CompletedTask;
            }
            private async Task<Message> PingNotMyHost(Message message) =>
                await SendMessage(_botClient, message, "Enter your ip");

            private static async Task<Message> PingHost(ITelegramBotClient bot, Message message, string nameOrAddress = "192.168.0.1")
            {
                bool pingable = false;
                Ping pinger = null;

                try
                {
                    pinger = new Ping();
                    PingReply reply = pinger.Send(nameOrAddress);
                    pingable = reply.Status == IPStatus.Success;
                }
                finally
                {
                    if (pinger != null)
                    {
                        pinger.Dispose();
                    }
                }
                var text = pingable ? "Yes internet is here" : "It's not here";
                await SendMessage(bot, message, text);

                return await bot.SendStickerAsync(
                    chatId: message.Chat.Id,
                    sticker: pingable ? "https://tlgrm.ru/_/stickers/1b5/0ab/1b50abf8-8451-40ca-be37-ffd7aa74ec4d/2.webp" : "https://cdn.tlgrm.app/stickers/4dd/300/4dd300fd-0a89-3f3d-ac53-8ec93976495e/192/11.webp"
                    );
            }
        }
    }
}

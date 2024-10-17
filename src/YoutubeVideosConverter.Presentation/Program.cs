using MediaToolkit.Model;
using MediaToolkit;
using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeExplode;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideosConverter.Application.Implementations;

namespace YoutubeVideosConverter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var botService = serviceProvider.GetService<IBotService>();
            await botService.StartBotAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConverterService, ConverterService>();
            var botToken = "telegram-api-key-here";
            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(botToken));
            services.AddSingleton<IBotService, BotService>();
        }

        private static InlineKeyboardMarkup InitInlineKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
   {
            [
                InlineKeyboardButton.WithCallbackData("🔍 გადავიყვანოთ Youtube-ის ლინკები mp3 ფორმატში", "convert_toMp3"),
            ],
            new []
            {
                InlineKeyboardButton.WithCallbackData("🔍 გადავიყვანო Youtube-ის ლინკები Voice ფორმატში", "convert_toVoice"),
            }
        });
        }
    }
}
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using YoutubeVideosConverter.Application.Abstractions;

namespace YoutubeVideosConverter.Application.Implementations
{
    public class BotService : IBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IConverterService _converterService;

        public BotService(ITelegramBotClient botClient, IConverterService greetingService)
        {
            _botClient = botClient;
            _converterService = greetingService;
        }

        public async Task StartBotAsync()
        {
            var cts = new CancellationTokenSource();

            await ClearMessageQueue(_botClient);

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                cancellationToken: cts.Token);

            Console.WriteLine("Bot is running...");
            await Task.Delay(-1, cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                Console.WriteLine($"Request from {update.Message.From.Username} came!Message - {update.Message.Text}.  Starting working with it. Current time - {DateTime.Now}");
                var message = update.Message;

                switch (message.Text)
                {
                    case "/start":
                        await client.SendTextMessageAsync(update.Message.Chat.Id,
                        "გამარჯობა! ეს არის ტელეგრამის ბოტი, რომელიც გადაიყვანს შენს მიერ მოწოდებული youtube-ის ლინკს mp3 ფორმატში." +
                        " ამის მერე შეძლებ ჩვეულებრივად მოუსმინო საყვარელ სიმღერას შენს ტელეფონზე/კომპიუტერზე.\n\n გაითვალისწინე," +
                        " რომ ტელეგრამის შეზღუდვის თანახმად," +
                        " *შეგიძლია გადმოწერო მაქსიმუმ 50 მეგაბაიტის მქონე აუდიო (რაც დაახლოვებით 140 წუთის ტოლია)* "
                        , parseMode: ParseMode.Markdown);
                        return;
                    default:
                        break;
                }
                await client.SendTextMessageAsync(update.Message.Chat.Id, "დავიწყეთ შენთვის mp3-ის გამზადება 🔥");
                var result = await _converterService.ConvertToMp3Async(message.Text);

                if (!result.IsSucceeded)
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, result.ErrorMessage);
                    return;
                }

                var data = result.GetResponse();

                var resultAudioStream = InputFile.FromStream(data.Stream);
                await client.SendAudioAsync(update.Message.Chat.Id, resultAudioStream, performer: data.AudioAuthor, title: data.AudioName, cancellationToken: token);

                await data.Stream.DisposeAsync();
                Console.WriteLine($"Request from {update.Message.From.Username} finished! Current time - {DateTime.Now}");

                return;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            return Task.CompletedTask;
        }
        public static async Task ClearMessageQueue(ITelegramBotClient _botClient)
        {
            var updates = await _botClient.GetUpdatesAsync();

            if (updates.Length > 0)
            {
                var lastUpdateId = updates.Last().Id;

                await _botClient.GetUpdatesAsync(offset: lastUpdateId + 1);

                Console.WriteLine("Message queue cleared.");
            }
            else
            {
                Console.WriteLine("No messages to clear.");
            }
        }
    }
}
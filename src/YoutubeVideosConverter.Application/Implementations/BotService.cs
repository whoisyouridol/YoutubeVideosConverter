using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using YoutubeVideoConverter.Infrastructure.SQL.Models;
using AngleSharp.Dom;
using YoutubeVideoConverter.Infrastructure.SQL.Enums;
using System.Diagnostics;
using YoutubeExplode;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace YoutubeVideosConverter.Application.Implementations;

public class BotService : IBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConverterService _converterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public BotService(ITelegramBotClient botClient, IConverterService converterService, IUnitOfWork unitOfWork, IConfiguration config)
    {
        _botClient = botClient;
        _converterService = converterService;
        _unitOfWork = unitOfWork;
        _config = config;
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
            var userReqResp = new UserRequestResponse
            {
                Username = update.Message.From.Username,
                Message = update.Message.Text,
                RequestDate = DateTime.Now,
            };
            _unitOfWork.GetRepository<UserRequestResponse>().AddAsync(userReqResp);
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
                    await _unitOfWork.SaveChangesAsync();
                    return;
                default:
                    if (HasValidPattern(update.Message.Text))
                    {
                        var _convertLog = new ConvertLog
                        {
                            Url = update.Message.Text,
                            ConversionDestination = Destination.AudioFile,
                            ConvertTo = ConvertType.ToMp3,
                        };
                        await client.SendTextMessageAsync(update.Message.Chat.Id, "დავიწყეთ შენთვის mp3-ის გამზადება 🔥", cancellationToken: token);

                        //this one is temporary, then I will add user`s choices context, will save in db his last preferences
                        //(for example take his selected destination from db and use needed switch case),
                        //and return him response according his choice.
                        var destination = "mp3";
                        switch (destination)
                        {
                            case "mp3":
                                _convertLog.ConvertTo = ConvertType.ToMp3;

                                var _sw = Stopwatch.StartNew();

                                var _result = await _converterService.ConvertToMp3Async(message.Text);

                                _sw.Stop();

                                _convertLog.ConversionTime = _sw.Elapsed;
                                userReqResp.ConvertLogs.Add(_convertLog);
                                _unitOfWork.GetRepository<UserRequestResponse>().AddAsync(userReqResp);
                                if (_result.IsSucceeded)
                                {
                                    var data = _result.GetResponse();
                                    _convertLog.ConvertedSuccessfully = true;
                                    userReqResp.RequestSucceeded = true;
                                    _convertLog.VideoName = data.AudioName;
                                    var resultAudioStream = InputFile.FromStream(data.Stream);
                                    await client.SendAudioAsync(update.Message.Chat.Id, resultAudioStream, performer: data.AudioAuthor, title: data.AudioName, cancellationToken: token);

                                    await data.Stream.DisposeAsync();
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(update.Message.Chat.Id, _result.ErrorMessage, cancellationToken: token);
                                    userReqResp.RequestSucceeded = false;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        await client.SendTextMessageAsync(update.Message.Chat.Id, "გაგზავნილი ლინკი არის არასწორ ფორმატში!");
                    }
                    await _unitOfWork.SaveChangesAsync();

                    break;
            }
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
    private bool HasValidPattern(string url)
    {
        try
        {
            string pattern = _config["YoutubeLinkValidationRegex"];
            Regex regex = new(pattern);
            return regex.IsMatch(url);
        }
        catch
        {
            return false;
        }
    }
}
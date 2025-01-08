using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideosConverter.Application.Implementations;
using YoutubeVideoConverter.Infrastructure.SQL.Persistance;
using Microsoft.Extensions.Configuration;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using YoutubeVideoConverter.Infrastructure.SQL.Implementations.UOW;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions;
using YoutubeVideoConverter.Infrastructure.SQL.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YoutubeVideosConverter.Presentation;

internal class Program
{
    private static IConfiguration _configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false)
        .Build();
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
        var botToken = "7745616857:AAFmYK1qm4O4es297jQkmD2AS0i6FsjSzes";
        services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(botToken));

        services.AddSqlServer<AppDbContext>(_configuration.GetConnectionString("DbConnection"));
        services.AddSingleton(_configuration);
        services.AddTransient<IConverterService, ConverterService>();
        services.AddTransient<IConvertLogRepository, ConvertLogRepository>();
        services.AddTransient<IUserRequestResponseRepository, UserRequestResponseRepository>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IBotService, BotService>();
    }
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DbConnection"));

            return new AppDbContext(optionsBuilder.Options);
        }
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
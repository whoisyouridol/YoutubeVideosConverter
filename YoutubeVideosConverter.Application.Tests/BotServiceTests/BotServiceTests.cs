using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideosConverter.Application.Implementations;

namespace YoutubeVideosConverter.Application.Tests.BotServiceTests
{
    public class BotServiceTests
    {
        [Fact]
        public async Task HandleUpdateAsync_StartMessage_ShouldReturnCorrectMessage()
        {
            //Arrange
            var tgClientMock = new Mock<ITelegramBotClient>();
            var updateMessage = new Update
            {
                Message = new Message
                {
                    Text = "/start",
                }
            };
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var mockConfig = new Mock<IConfiguration>();
            var converterService = new Mock<IConverterService>();

            var botService = new BotService(tgClientMock.Object,
                                            converterService.Object,
                                            mockUnitOfWork.Object,
                                            mockConfig.Object);

            //Act

            await botService.StartBotAsync();

            //Assert
        }
    }
}

using Microsoft.Extensions.Configuration;
using Moq;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using YoutubeVideosConverter.Application.Implementations;
using YoutubeVideosConverter.Application.Models.Response;

namespace YoutubeVideosConverter.Application.Tests.ConverterServiceTests
{
    public class ConverterServiceTests
    {
        /// <summary>
        /// Test for successfull convertation, if :
        ///  - URL is valid,
        ///  - In video exists audio-stream,
        ///  - FFmpeg configured correctly,
        ///  - no exception occured.
        /// </summary>
        [Fact]
        public async Task ConvertToMp3Async_ValidUrl_ShouldReturnSuccessExecutionResult()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FFMpegPath"]).Returns("ffmpeg");

            var service = new ConverterService(mockUnitOfWork.Object, mockConfig.Object);

            var testUrl = "https://www.youtube.com/watch?v=xwTPvcPYaOo";

            // Act
            var result = await service.ConvertToMp3Async(testUrl);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSucceeded, "Ожидался успех при валидном URL и корректной конвертации");

            Assert.NotNull(result.GetResponse());
            Assert.IsType<AudioResponseModel>(result.GetResponse());

            Assert.False(string.IsNullOrEmpty(result.GetResponse().AudioName));
            Assert.False(string.IsNullOrEmpty(result.GetResponse().AudioAuthor));
        }

        /// <summary>
        /// Test for case if URL is incorrect or some another exception is thrown.
        /// Awaiting ExecutionResult with IsSuccess = false value.
        /// </summary>
        [Fact]
        public async Task ConvertToMp3Async_InvalidUrl_ShouldReturnFailExecutionResult()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["FFMpegPath"]).Returns("ffmpeg");
            var service = new ConverterService(mockUnitOfWork.Object, mockConfig.Object);

            var invalidUrl = "https://www.youtube.com/watch?v=invalid_youtube_id";

            // Act
            var result = await service.ConvertToMp3Async(invalidUrl);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSucceeded, "Ожидался неудачный результат при невалидной ссылке");
            Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
        }
    }
}
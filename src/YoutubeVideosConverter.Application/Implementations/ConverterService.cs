using System.Diagnostics;
using YoutubeExplode;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideosConverter.Application.Models;
using YoutubeVideosConverter.Application.Models.Response;
using YoutubeVideosConverter.Application.Models.Generic;
using System.Text.RegularExpressions;
using YoutubeVideoConverter.Infrastructure.SQL.Abstractions.UOW;
using Microsoft.Extensions.Configuration;
using YoutubeVideoConverter.Infrastructure.SQL.Models;
using YoutubeVideoConverter.Infrastructure.SQL.Enums;
namespace YoutubeVideosConverter.Application.Implementations;

public class ConverterService : IConverterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public ConverterService(IUnitOfWork unitOfWork,IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<ExecutionResult<AudioResponseModel>> ConvertToMp3Async(string url)
    {
        var youtube = new YoutubeClient();
        try
        {
            var video = await youtube.Videos.GetAsync(url);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(x => x.Bitrate).FirstOrDefault() 
                ?? throw new Exception("ამ ვიდეოში ვერ მოიძებნა აუდიო სტრიმი!");
            var tempWebmFilePath = $"{Path.GetTempFileName()}.webm";
            var tempMp3FilePath = $"{Path.GetTempFileName()}.mp3";

            try
            {
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempWebmFilePath);

                await ConvertWebmToMp3(tempWebmFilePath, tempMp3FilePath);

                CustomFileStream mp3Stream = new(tempMp3FilePath, FileMode.Open, FileAccess.Read);
                return ExecutionResult<AudioResponseModel>.Sucesss(new AudioResponseModel(mp3Stream, video.Title, video.Author.ChannelTitle));
            }
            finally
            {
                if (File.Exists(tempWebmFilePath)) File.Delete(tempWebmFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            return ExecutionResult<AudioResponseModel>.Fail($"მოხდა პრობლემა ვიდეოს კონვერტაციის დროს");
        }
    }
    public async Task ConvertWebmToMp3(string inputWebmFilePath, string outputMp3FilePath)
    {
        string ffmpegPath = _config["FFMpegPath"];

        string arguments = $"-i \"{inputWebmFilePath}\" \"{outputMp3FilePath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string errors = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Conversion completed successfully.");
            }
            else
            {
                Console.WriteLine($"FFmpeg encountered an error: {errors}");
            }
        }
    }
}
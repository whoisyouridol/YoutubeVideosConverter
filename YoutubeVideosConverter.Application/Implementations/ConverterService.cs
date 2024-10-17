using System.Diagnostics;
using YoutubeExplode;
using YoutubeVideosConverter.Application.Abstractions;
using YoutubeVideosConverter.Application.Models;
using YoutubeVideosConverter.Application.Models.Response;
using YoutubeVideosConverter.Application.Models.Generic;
using System.Text.RegularExpressions;

namespace YoutubeVideosConverter.Application.Implementations
{
    public class ConverterService : IConverterService
    {
        public async Task<ExecutionResult<AudioResponseModel>> ConvertToMp3Async(string url)
        {
            if (!HasValidPattern(url))
            {
                return ExecutionResult<AudioResponseModel>.Fail("მოცემული ტექსტი არის არასწორ ფორმატში! აუცილებლად უნდა იყოს Youtube-ის URL", failReason: ExecutionFailReason.BAD_REQUEST);
            }
            var youtube = new YoutubeClient();
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var video = await youtube.Videos.GetAsync(url);
                Console.WriteLine($"Requested video URL: {video.Url}");
                Console.WriteLine("Getting video url time: " + sw.ElapsedMilliseconds);

                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(x => x.Bitrate).FirstOrDefault() ?? throw new Exception("No audio stream found for the video.");
                var tempWebmFilePath = Path.GetTempFileName() + ".webm";
                var tempMp3FilePath = Path.GetTempFileName() + ".mp3";

                try
                {
                    await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempWebmFilePath);
                    Console.WriteLine("Video downloaded : " + sw.ElapsedMilliseconds);

                    await ConvertWebmToMp3(tempWebmFilePath, tempMp3FilePath);

                    Console.WriteLine("Conversion completed successfully!");
                    Console.WriteLine("Converted to mp3 : " + sw.ElapsedMilliseconds);

                    CustomFileStream mp3Stream = new(tempMp3FilePath, FileMode.Open, FileAccess.Read);
                    return ExecutionResult<AudioResponseModel>.Sucesss(new AudioResponseModel(mp3Stream, video.Title, video.Author.ChannelTitle));
                }
                finally
                {
                    sw.Stop();
                    Console.WriteLine("Elapsed Milliseconds : " + sw.ElapsedMilliseconds);
                    if (System.IO.File.Exists(tempWebmFilePath)) System.IO.File.Delete(tempWebmFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return ExecutionResult<AudioResponseModel>.Fail($"მოხდა პრობლემა ვიდეოს კონვერტაციის დროს");
            }
        }
        private static bool HasValidPattern(string url)
        {
            try
            {
                string pattern = @"(?:https?:\/\/)?(?:www\.)?youtu\.?be(?:\.com)?\/?.*(?:watch|embed)?(?:.*v=|v\/|\/)([\w\-_]+)\&?";
                Regex regex = new(pattern);
                return regex.IsMatch(url);
            }
            catch
            {
                return false;
            }
        }
        public async Task ConvertWebmToMp3(string inputWebmFilePath, string outputMp3FilePath)
        {
            string ffmpegPath = @"C:\Users\User\Desktop\ffmpeg\ffmpeg-7.0.2-essentials_build\bin\ffmpeg.exe"; // Adjust this path to where ffmpeg.exe is located

            // Define the FFmpeg arguments
            string arguments = $"-i \"{inputWebmFilePath}\" \"{outputMp3FilePath}\"";

            // Set up the process start info to run FFmpeg
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,  // The command to run FFmpeg (it's in your system's PATH)
                Arguments = arguments, // The arguments for input and output files
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true // Don't show a command window
            };

            // Start the FFmpeg process
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Capture and log output (optional)
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
}
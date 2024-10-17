using YoutubeVideosConverter.Application.Models.Generic;
using YoutubeVideosConverter.Application.Models.Response;

namespace YoutubeVideosConverter.Application.Abstractions;

public interface IConverterService
{
    Task<ExecutionResult<AudioResponseModel>> ConvertToMp3Async(string url);
}
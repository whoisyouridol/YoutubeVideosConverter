namespace YoutubeVideosConverter.Application.Models.Generic;

public class ExecutionResult : IEquatable<ExecutionResult>
{
    public bool IsSucceeded { get; set; }
    public string? ErrorMessage { get; set; }
    public ExecutionFailReason? FailReason { get; set; } = ExecutionFailReason.OTHER;
    public ExecutionResult(bool isSucceeded, string? errorMessage, ExecutionFailReason? failReason = ExecutionFailReason.OTHER)
    {
        IsSucceeded = isSucceeded;
        ErrorMessage = errorMessage;
        FailReason = failReason;
    }
    //public static ExecutionResult<TResponse> Success<TResponse>(TResponse response) => ExecutionResult<TResponse>.Success(response);
    //public static ExecutionResult Success() => new ExecutionResult(true, default);
    public static ExecutionResult Fail(string errorMessage) => new ExecutionResult(false, errorMessage);

    public bool Equals(ExecutionResult? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return other.IsSucceeded == IsSucceeded && other.ErrorMessage == ErrorMessage;
    }

 
}
public class ExecutionResult<TResult> : ExecutionResult
{
    private TResult _response;
    public ExecutionResult(bool isSucceeded, string? errorMessage, TResult response, ExecutionFailReason? failReason = ExecutionFailReason.OTHER) : base(isSucceeded, errorMessage, failReason)
    {
        _response = response;
    }
    public new TResult? GetResponse() => _response;
    public static ExecutionResult<TResult> Sucesss(TResult response) => new(true, default, response);
    public static ExecutionResult<TResult> Fail(string errorMessage) => new ExecutionResult<TResult>(false, errorMessage, default);
    public static ExecutionResult<TResult> Fail(string errorMessage,ExecutionFailReason failReason) => new ExecutionResult<TResult>(false, errorMessage, default,failReason);
}
public enum ExecutionFailReason
{
    NOT_FOUND,
    BAD_REQUEST,
    OTHER
}
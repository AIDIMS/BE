namespace AIDIMS.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static Result<T> Failure(string message, IEnumerable<string>? errors = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static Result Success(string? message = null)
    {
        return new Result
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static Result Failure(string message, IEnumerable<string>? errors = null)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };
    }
}

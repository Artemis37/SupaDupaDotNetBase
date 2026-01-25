namespace Shared.Application.Models;

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; private set; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new Result(true, string.Empty);
    public static Result Fail(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; private set; }

    private Result(bool isSuccess, T value, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, value, string.Empty);
    public static new Result<T> Fail(string error) => new Result<T>(false, default, error);
}

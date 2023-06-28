namespace Template.MobileApp.Models;

public interface IResult<out T>
{
    bool IsSuccess { get; }

    T Value { get; }
}

public static class Result
{
    public static IResult<T> Success<T>(T value) => new ResultImpl<T>(true, value);

    public static IResult<T> Failed<T>() => ResultImpl<T>.FailedResult;

    private sealed class ResultImpl<T> : IResult<T>
    {
        public static readonly ResultImpl<T> FailedResult = new(false, default!);

        public bool IsSuccess { get; }

        public T Value { get; }

        public ResultImpl(bool isSuccess, T value)
        {
            IsSuccess = isSuccess;
            Value = value;
        }
    }
}

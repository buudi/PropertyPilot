namespace PropertyPilot.Services.Generics;

public class AttemptResult<T>
{
    public T? Value { get; }
    public int? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => ErrorCode == null;

    public AttemptResult(T value)
    {
        Value = value;
    }

    public AttemptResult(int errorCode, string? errorMessage = null)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}

//public static class Attempt
//{
//    public static AttemptResult<T> Execute<T>(Func<T> action, Func<Exception, int> errorHandler)
//    {
//        try
//        {
//            return new AttemptResult<T>(action());
//        }
//        catch (Exception ex)
//        {
//            return new AttemptResult<T>(errorHandler(ex));
//        }
//    }
//}

//
// Attempt Usage Example
//var result = Attempt.Execute(
//    () => SomeOperation(),
//    ex => ex is InvalidOperationException ? 400 : 500 // Map exceptions to error codes
//);

using System.Runtime.Serialization;

namespace Common.Domain.Validation;

public class GlobalException : Exception
{
    public GlobalException()
    {
    }

    public GlobalException(string? message) : base(message)
    {
    }

    public GlobalException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GlobalException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

using System.Runtime.Serialization;

namespace Nixvale.Core.Exceptions;

[Serializable]
public class NixvaleException : Exception
{
    public NixvaleException()
    {
    }

    public NixvaleException(string message)
        : base(message)
    {
    }

    public NixvaleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected NixvaleException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
} 
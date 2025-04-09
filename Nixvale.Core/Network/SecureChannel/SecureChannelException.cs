using System.Runtime.Serialization;

namespace Nixvale.Core.Network.SecureChannel;

/// <summary>
/// Exception thrown when secure channel operations fail
/// </summary>
[Serializable]
public class SecureChannelException : Exception
{
    public SecureChannelException()
    {
    }

    public SecureChannelException(string message)
        : base(message)
    {
    }

    public SecureChannelException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected SecureChannelException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
} 
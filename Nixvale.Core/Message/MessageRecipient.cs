namespace Nixvale.Core.Message;

/// <summary>
/// Represents a recipient of a message and tracks delivery status
/// </summary>
public class MessageRecipient
{
    /// <summary>
    /// ID of the recipient
    /// </summary>
    public required byte[] Id { get; set; }

    /// <summary>
    /// When the message was delivered to this recipient
    /// </summary>
    public required DateTime DeliveredAt { get; set; }

    /// <summary>
    /// Whether the message has been read by the recipient
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// When the message was read by the recipient, if it has been read
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Creates a new message recipient with the given ID
    /// </summary>
    public static MessageRecipient Create(byte[] id)
    {
        return new MessageRecipient
        {
            Id = id,
            DeliveredAt = DateTime.UtcNow,
            IsRead = false,
            ReadAt = null
        };
    }

    /// <summary>
    /// Marks the message as read by this recipient
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Writes the recipient to a binary writer
    /// </summary>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Id.Length);
        writer.Write(Id);
        writer.Write(DeliveredAt.ToBinary());
        writer.Write(IsRead);
        writer.Write(ReadAt.HasValue);
        if (ReadAt.HasValue)
            writer.Write(ReadAt.Value.ToBinary());
    }

    /// <summary>
    /// Reads a recipient from a binary reader
    /// </summary>
    public static MessageRecipient ReadFrom(BinaryReader reader)
    {
        var idLength = reader.ReadInt32();
        var recipient = new MessageRecipient
        {
            Id = reader.ReadBytes(idLength),
            DeliveredAt = DateTime.FromBinary(reader.ReadInt64()),
            IsRead = reader.ReadBoolean()
        };

        if (reader.ReadBoolean())
            recipient.ReadAt = DateTime.FromBinary(reader.ReadInt64());

        return recipient;
    }
} 
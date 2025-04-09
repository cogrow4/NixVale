namespace Nixvale.Core.Message;

/// <summary>
/// Represents a message in the Nixvale network
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// ID of the sender
    /// </summary>
    public byte[] SenderId { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Optional ID of the recipient (for private messages)
    /// </summary>
    public byte[]? RecipientId { get; set; }

    /// <summary>
    /// Type of message
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional file data for file messages
    /// </summary>
    public FileData? File { get; set; }

    /// <summary>
    /// Optional reference to another message (for replies)
    /// </summary>
    public Guid? ReplyToId { get; set; }

    /// <summary>
    /// List of recipients who have received the message
    /// </summary>
    public List<MessageRecipient> Recipients { get; set; } = new();

    /// <summary>
    /// Optional group ID
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// Creates a new text message
    /// </summary>
    public static Message CreateText(byte[] senderId, string content, byte[]? recipientId = null)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SenderId = senderId,
            RecipientId = recipientId,
            Type = MessageType.Text,
            Content = content
        };
    }

    /// <summary>
    /// Creates a new file message
    /// </summary>
    public static Message CreateFile(byte[] senderId, FileData file, byte[]? recipientId = null)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SenderId = senderId,
            RecipientId = recipientId,
            Type = MessageType.File,
            File = file
        };
    }

    /// <summary>
    /// Creates a new typing notification
    /// </summary>
    public static Message CreateTyping(byte[] senderId, byte[]? recipientId = null)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SenderId = senderId,
            RecipientId = recipientId,
            Type = MessageType.Typing
        };
    }

    /// <summary>
    /// Creates a new group join message
    /// </summary>
    public static Message CreateGroupJoin(Guid groupId, byte[] memberId)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SenderId = memberId,
            Type = MessageType.GroupJoin,
            GroupId = groupId
        };
    }

    /// <summary>
    /// Creates a new group leave message
    /// </summary>
    public static Message CreateGroupLeave(Guid groupId, byte[] memberId)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SenderId = memberId,
            Type = MessageType.GroupLeave,
            GroupId = groupId
        };
    }

    /// <summary>
    /// Creates a new group image message
    /// </summary>
    public static Message CreateGroupImage(Guid groupId, byte[]? image)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Type = MessageType.GroupImage,
            GroupId = groupId,
            Content = image != null ? Convert.ToBase64String(image) : null
        };
    }

    /// <summary>
    /// Writes the message to a binary writer
    /// </summary>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Id.ToByteArray());
        writer.Write(Timestamp.ToBinary());
        writer.Write(SenderId.Length);
        writer.Write(SenderId);
        
        writer.Write(RecipientId != null);
        if (RecipientId != null)
        {
            writer.Write(RecipientId.Length);
            writer.Write(RecipientId);
        }

        writer.Write((byte)Type);
        if (GroupId.HasValue)
        {
            writer.Write(true);
            writer.Write(GroupId.Value.ToByteArray());
        }
        else
        {
            writer.Write(false);
        }
        writer.Write(Content);

        writer.Write(File != null);
        if (File != null)
        {
            writer.Write(File.Name);
            writer.Write(File.Size);
            writer.Write(File.Hash);
        }

        writer.Write(ReplyToId.HasValue);
        if (ReplyToId.HasValue)
            writer.Write(ReplyToId.Value.ToByteArray());

        writer.Write(Recipients.Count);
        foreach (var recipient in Recipients)
        {
            writer.Write(recipient.Id);
            writer.Write(recipient.DeliveredAt.ToBinary());
        }
    }

    /// <summary>
    /// Reads a message from a binary reader
    /// </summary>
    public static Message ReadFrom(BinaryReader reader)
    {
        var message = new Message
        {
            Id = new Guid(reader.ReadBytes(16)),
            Timestamp = DateTime.FromBinary(reader.ReadInt64())
        };

        var senderIdLength = reader.ReadInt32();
        message.SenderId = reader.ReadBytes(senderIdLength);

        if (reader.ReadBoolean())
        {
            var recipientIdLength = reader.ReadInt32();
            message.RecipientId = reader.ReadBytes(recipientIdLength);
        }

        message.Type = (MessageType)reader.ReadByte();
        if (reader.ReadBoolean())
        {
            message.GroupId = new Guid(reader.ReadBytes(16));
        }
        message.Content = reader.ReadString();

        if (reader.ReadBoolean())
        {
            message.File = new FileData
            {
                Name = reader.ReadString(),
                Size = reader.ReadInt64(),
                Hash = reader.ReadString()
            };
        }

        if (reader.ReadBoolean())
            message.ReplyToId = new Guid(reader.ReadBytes(16));

        var recipientCount = reader.ReadInt32();
        for (var i = 0; i < recipientCount; i++)
        {
            message.Recipients.Add(new MessageRecipient
            {
                Id = reader.ReadBytes(32),
                DeliveredAt = DateTime.FromBinary(reader.ReadInt64())
            });
        }

        return message;
    }
}

/// <summary>
/// Type of message
/// </summary>
public enum MessageType : byte
{
    Text = 0,
    File = 1,
    Typing = 2,
    GroupJoin = 3,
    GroupLeave = 4,
    GroupImage = 5
}

/// <summary>
/// File data for file messages
/// </summary>
public class FileData
{
    public required string Name { get; set; }
    public required long Size { get; set; }
    public required string Hash { get; set; }
} 
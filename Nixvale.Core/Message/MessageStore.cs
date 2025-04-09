using System.Security.Cryptography;

namespace Nixvale.Core.Message;

/// <summary>
/// Stores and manages encrypted messages
/// </summary>
public class MessageStore : IDisposable
{
    private readonly string _storePath;
    private readonly byte[] _key;
    private readonly Dictionary<Guid, MessageMetadata> _messageIndex = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private bool _isDisposed;

    public MessageStore(string storePath, byte[] key)
    {
        _storePath = storePath;
        _key = key;

        // Create store directory if it doesn't exist
        Directory.CreateDirectory(Path.GetDirectoryName(storePath)!);

        // Load message index
        LoadIndex();
    }

    /// <summary>
    /// Adds a message to the store
    /// </summary>
    public void AddMessage(Message message)
    {
        _lock.EnterWriteLock();
        try
        {
            // Generate message ID if not set
            if (message.Id == Guid.Empty)
                message.Id = Guid.NewGuid();

            // Encrypt message
            var encryptedData = EncryptMessage(message);

            // Add to index
            var metadata = new MessageMetadata
            {
                Id = message.Id,
                Timestamp = message.Timestamp,
                SenderId = message.SenderId,
                Type = message.Type,
                Position = GetEndOfFile(),
                Length = encryptedData.Length
            };
            _messageIndex[message.Id] = metadata;

            // Write to file
            using var fs = File.Open(_storePath, FileMode.OpenOrCreate, FileAccess.Write);
            fs.Seek(metadata.Position, SeekOrigin.Begin);
            fs.Write(encryptedData);

            // Update index file
            SaveIndex();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a message by ID
    /// </summary>
    public Message? GetMessage(Guid id)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_messageIndex.TryGetValue(id, out var metadata))
                return null;

            using var fs = File.OpenRead(_storePath);
            fs.Seek(metadata.Position, SeekOrigin.Begin);
            var encryptedData = new byte[metadata.Length];
            fs.Read(encryptedData, 0, encryptedData.Length);

            return DecryptMessage(encryptedData);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets messages in a time range
    /// </summary>
    public IEnumerable<Message> GetMessages(DateTime start, DateTime end)
    {
        _lock.EnterReadLock();
        try
        {
            var messages = _messageIndex.Values
                .Where(m => m.Timestamp >= start && m.Timestamp <= end)
                .OrderBy(m => m.Timestamp)
                .Select(m => GetMessage(m.Id))
                .Where(m => m != null);

            return messages!;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private void LoadIndex()
    {
        var indexPath = _storePath + ".idx";
        if (!File.Exists(indexPath))
            return;

        _lock.EnterWriteLock();
        try
        {
            var encryptedData = File.ReadAllBytes(indexPath);
            using var aes = CreateAes();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData);
            cs.FlushFinalBlock();

            var data = ms.ToArray();
            using var reader = new BinaryReader(new MemoryStream(data));
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var metadata = new MessageMetadata
                {
                    Id = new Guid(reader.ReadBytes(16)),
                    Timestamp = DateTime.FromBinary(reader.ReadInt64()),
                    SenderId = reader.ReadBytes(32),
                    Type = (MessageType)reader.ReadByte(),
                    Position = reader.ReadInt64(),
                    Length = reader.ReadInt32()
                };
                _messageIndex[metadata.Id] = metadata;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void SaveIndex()
    {
        var indexPath = _storePath + ".idx";
        
        _lock.EnterReadLock();
        try
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(_messageIndex.Count);
            foreach (var metadata in _messageIndex.Values)
            {
                writer.Write(metadata.Id.ToByteArray());
                writer.Write(metadata.Timestamp.ToBinary());
                writer.Write(metadata.SenderId);
                writer.Write((byte)metadata.Type);
                writer.Write(metadata.Position);
                writer.Write(metadata.Length);
            }

            var data = ms.ToArray();
            using var aes = CreateAes();
            using var fs = File.Create(indexPath);
            using var cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data);
            cs.FlushFinalBlock();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private byte[] EncryptMessage(Message message)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        message.WriteTo(writer);
        var data = ms.ToArray();

        using var aes = CreateAes();
        using var encryptedMs = new MemoryStream();
        using var cs = new CryptoStream(encryptedMs, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(data);
        cs.FlushFinalBlock();

        return encryptedMs.ToArray();
    }

    private Message DecryptMessage(byte[] encryptedData)
    {
        using var aes = CreateAes();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(encryptedData);
        cs.FlushFinalBlock();

        var data = ms.ToArray();
        using var reader = new BinaryReader(new MemoryStream(data));
        return Message.ReadFrom(reader);
    }

    private Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = new byte[16]; // Use zero IV since each message has unique ID
        return aes;
    }

    private long GetEndOfFile()
    {
        if (!File.Exists(_storePath))
            return 0;
        return new FileInfo(_storePath).Length;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _lock.Dispose();
            _isDisposed = true;
        }
    }

    private class MessageMetadata
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[] SenderId { get; set; } = Array.Empty<byte>();
        public MessageType Type { get; set; }
        public long Position { get; set; }
        public int Length { get; set; }
    }
} 
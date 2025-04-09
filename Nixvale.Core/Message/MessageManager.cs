using System.Collections.Concurrent;
using Nixvale.Core.Network.SecureChannel;

namespace Nixvale.Core.Message;

/// <summary>
/// Manages message routing, delivery, and synchronization
/// </summary>
public class MessageManager : IDisposable
{
    private readonly MessageStore _store;
    private readonly byte[] _nodeId;
    private readonly ConcurrentDictionary<string, WeakReference<Stream>> _activeChannels = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastSyncTimes = new();
    private readonly Timer _syncTimer;
    private bool _isDisposed;

    public event EventHandler<Message>? MessageReceived;
    public event EventHandler<MessageRecipient>? MessageDelivered;
    public event EventHandler<MessageRecipient>? MessageRead;

    public MessageManager(MessageStore store, byte[] nodeId)
    {
        _store = store;
        _nodeId = nodeId;
        _syncTimer = new Timer(SyncWithPeers, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Sends a message to one or more recipients
    /// </summary>
    public async Task SendMessageAsync(Message message, Stream channel)
    {
        // Add message to local store
        _store.AddMessage(message);

        // Write message to channel
        using var writer = new BinaryWriter(channel, System.Text.Encoding.UTF8, true);
        message.WriteTo(writer);
        await channel.FlushAsync();

        // Add recipient tracking
        foreach (var recipientId in GetMessageRecipients(message))
        {
            var recipient = MessageRecipient.Create(recipientId);
            message.Recipients.Add(recipient);
            MessageDelivered?.Invoke(this, recipient);
        }
    }

    /// <summary>
    /// Registers a secure channel for message delivery
    /// </summary>
    public void RegisterChannel(string peerId, Stream channel)
    {
        _activeChannels.TryAdd(peerId, new WeakReference<Stream>(channel));
        _ = StartReceivingAsync(peerId, channel);
    }

    /// <summary>
    /// Marks a message as read by the local node
    /// </summary>
    public void MarkMessageAsRead(Guid messageId)
    {
        var message = _store.GetMessage(messageId);
        if (message == null) return;

        var recipient = message.Recipients.FirstOrDefault(r => r.Id.SequenceEqual(_nodeId));
        if (recipient == null) return;

        recipient.MarkAsRead();
        MessageRead?.Invoke(this, recipient);

        // Propagate read status to peers
        BroadcastReadStatus(messageId);
    }

    private async Task StartReceivingAsync(string peerId, Stream channel)
    {
        try
        {
            using var reader = new BinaryReader(channel, System.Text.Encoding.UTF8, true);
            while (!_isDisposed)
            {
                var message = Message.ReadFrom(reader);
                
                // Skip if we already have this message
                if (_store.GetMessage(message.Id) != null)
                    continue;

                // Store and notify
                _store.AddMessage(message);
                MessageReceived?.Invoke(this, message);

                // Add recipient and propagate
                if (IsMessageForUs(message))
                {
                    var recipient = MessageRecipient.Create(_nodeId);
                    message.Recipients.Add(recipient);
                    MessageDelivered?.Invoke(this, recipient);
                }

                // Propagate to other peers
                await PropagateMessageAsync(message, peerId);
            }
        }
        catch (Exception)
        {
            // Channel closed or error occurred
            _activeChannels.TryRemove(peerId, out _);
        }
    }

    private async Task PropagateMessageAsync(Message message, string excludePeerId)
    {
        foreach (var pair in _activeChannels)
        {
            if (pair.Key == excludePeerId) continue;
            if (!pair.Value.TryGetTarget(out var channel)) continue;

            try
            {
                using var writer = new BinaryWriter(channel, System.Text.Encoding.UTF8, true);
                message.WriteTo(writer);
                await channel.FlushAsync();
            }
            catch
            {
                // Channel error - will be cleaned up on next receive error
            }
        }
    }

    private void BroadcastReadStatus(Guid messageId)
    {
        foreach (var pair in _activeChannels)
        {
            if (!pair.Value.TryGetTarget(out var channel)) continue;

            try
            {
                using var writer = new BinaryWriter(channel, System.Text.Encoding.UTF8, true);
                writer.Write((byte)1); // Read status update
                writer.Write(messageId.ToByteArray());
                writer.Write(_nodeId.Length);
                writer.Write(_nodeId);
                writer.Write(DateTime.UtcNow.ToBinary());
            }
            catch
            {
                // Channel error - will be cleaned up on next receive error
            }
        }
    }

    private void SyncWithPeers(object? state)
    {
        foreach (var pair in _activeChannels)
        {
            if (!pair.Value.TryGetTarget(out var channel)) continue;

            try
            {
                var lastSync = _lastSyncTimes.GetOrAdd(pair.Key, DateTime.MinValue);
                var messages = _store.GetMessages(lastSync, DateTime.UtcNow);

                using var writer = new BinaryWriter(channel, System.Text.Encoding.UTF8, true);
                foreach (var message in messages)
                {
                    message.WriteTo(writer);
                }

                _lastSyncTimes[pair.Key] = DateTime.UtcNow;
            }
            catch
            {
                // Channel error - will be cleaned up on next receive error
            }
        }
    }

    private byte[][] GetMessageRecipients(Message message)
    {
        if (message.RecipientId != null)
            return new[] { message.RecipientId };

        // For group messages, we would get group members here
        return Array.Empty<byte[]>();
    }

    private bool IsMessageForUs(Message message)
    {
        if (message.RecipientId == null)
            return true; // Broadcast message

        return message.RecipientId.SequenceEqual(_nodeId);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _syncTimer.Dispose();
            _isDisposed = true;
        }
    }
} 
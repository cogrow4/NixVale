using System.Collections.Concurrent;
using System.Security.Cryptography;
using Nixvale.Core.Debug;
using Nixvale.Core.Exceptions;

namespace Nixvale.Core.Node;

/// <summary>
/// Represents a node in the Nixvale network
/// </summary>
public sealed class NixvaleNode : IAsyncDisposable
{
    private readonly IDebug _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly ConcurrentDictionary<byte[], Profile> _knownProfiles = new(ByteArrayComparer.Instance);
    private bool _isDisposed;

    private Profile _profile;
    private NodeConfiguration _config;
    private NodeState _state;

    /// <summary>
    /// Event raised when a network invitation is received
    /// </summary>
    public event EventHandler<NetworkInvitationEventArgs>? InvitationReceived;

    /// <summary>
    /// Event raised when the node's state changes
    /// </summary>
    public event EventHandler<NodeStateEventArgs>? StateChanged;

    /// <summary>
    /// Event raised when the node's profile changes
    /// </summary>
    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    /// <summary>
    /// Creates a new Nixvale node
    /// </summary>
    public NixvaleNode(
        Profile profile,
        NodeConfiguration config,
        IDebug logger)
    {
        _profile = profile;
        _config = config;
        _logger = logger;
        _state = NodeState.CreateInitial(config.LocalServicePort);

        // Start background tasks
        _ = StartBackgroundTasksAsync();
    }

    /// <summary>
    /// Gets the current node state
    /// </summary>
    public NodeState GetCurrentState()
    {
        return _state;
    }

    /// <summary>
    /// Gets the current profile
    /// </summary>
    public Profile GetCurrentProfile()
    {
        return _profile;
    }

    /// <summary>
    /// Updates the node's profile
    /// </summary>
    public async Task UpdateProfileAsync(
        string? newDisplayName = null,
        ProfileStatus? newStatus = null,
        string? newStatusMessage = null,
        byte[]? newDisplayImage = null,
        CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            var updatedProfile = _profile.WithUpdates(
                newDisplayName,
                newStatus,
                newStatusMessage,
                newDisplayImage);

            _profile = updatedProfile;
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(updatedProfile));
            
            // TODO: Announce profile changes to network
        }
        finally
        {
            _stateLock.Release();
        }
    }

    /// <summary>
    /// Updates the node's configuration
    /// </summary>
    public async Task UpdateConfigurationAsync(NodeConfiguration newConfig, CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            _config = newConfig;
            // TODO: Apply configuration changes
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private async Task StartBackgroundTasksAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                // TODO: Implement DHT announcements, connection management, etc.
                await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.WriteException(ex, "Background tasks failed");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        try
        {
            _cts.Cancel();
            // TODO: Clean up connections, DHT, etc.
        }
        finally
        {
            _cts.Dispose();
            _stateLock.Dispose();
        }
    }
}

/// <summary>
/// Event arguments for network invitations
/// </summary>
public class NetworkInvitationEventArgs : EventArgs
{
    public required byte[] NetworkId { get; init; }
    public required string? InvitationMessage { get; init; }
}

/// <summary>
/// Event arguments for node state changes
/// </summary>
public class NodeStateEventArgs : EventArgs
{
    public required NodeState NewState { get; init; }
}

/// <summary>
/// Event arguments for profile changes
/// </summary>
public class ProfileChangedEventArgs : EventArgs
{
    public required Profile NewProfile { get; init; }
}

/// <summary>
/// Comparer for byte arrays
/// </summary>
internal class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public static ByteArrayComparer Instance { get; } = new();

    public bool Equals(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        return x.AsSpan().SequenceEqual(y);
    }

    public int GetHashCode(byte[] obj)
    {
        return obj.Aggregate(17, (current, b) => current * 31 + b);
    }
} 
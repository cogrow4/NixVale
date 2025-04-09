using Nixvale.Core.Debug;
using Nixvale.Core.Node;
using System.Collections.ObjectModel;

namespace NixvaleNew.Maui.Services;

public class NixvaleService
{
    private readonly IDebug _logger;
    private NixvaleNode? _node;
    private readonly ObservableCollection<Profile> _knownProfiles = new();

    public event EventHandler<NodeStateEventArgs>? NodeStateChanged;
    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;
    public event EventHandler<NetworkInvitationEventArgs>? InvitationReceived;

    public bool IsRunning => _node != null;
    public IReadOnlyCollection<Profile> KnownProfiles => _knownProfiles;

    public NixvaleService(IDebug logger)
    {
        _logger = logger;
    }

    public async Task StartNodeAsync(Profile profile, NodeConfiguration config)
    {
        if (_node != null)
            throw new InvalidOperationException("Node is already running");

        try
        {
            _node = new NixvaleNode(profile, config, _logger);
            
            // Wire up events
            _node.StateChanged += (s, e) => NodeStateChanged?.Invoke(this, e);
            _node.ProfileChanged += (s, e) => ProfileChanged?.Invoke(this, e);
            _node.InvitationReceived += (s, e) => InvitationReceived?.Invoke(this, e);

            _logger.Write(LogLevel.Information, "Nixvale node started successfully");
        }
        catch (Exception ex)
        {
            _logger.WriteException(ex, "Failed to start Nixvale node");
            throw;
        }
    }

    public async Task StopNodeAsync()
    {
        if (_node == null)
            return;

        try
        {
            await _node.DisposeAsync();
            _node = null;
            _knownProfiles.Clear();
            _logger.Write(LogLevel.Information, "Nixvale node stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.WriteException(ex, "Failed to stop Nixvale node");
            throw;
        }
    }

    public async Task UpdateProfileAsync(
        string? newDisplayName = null,
        ProfileStatus? newStatus = null,
        string? newStatusMessage = null,
        byte[]? newDisplayImage = null,
        CancellationToken cancellationToken = default)
    {
        if (_node == null)
            throw new InvalidOperationException("Node is not running");

        await _node.UpdateProfileAsync(
            newDisplayName,
            newStatus,
            newStatusMessage,
            newDisplayImage,
            cancellationToken);
    }

    public async Task UpdateConfigurationAsync(
        NodeConfiguration newConfig,
        CancellationToken cancellationToken = default)
    {
        if (_node == null)
            throw new InvalidOperationException("Node is not running");

        await _node.UpdateConfigurationAsync(newConfig, cancellationToken);
    }

    public NodeState? GetCurrentState()
    {
        return _node?.GetCurrentState();
    }

    public Profile? GetCurrentProfile()
    {
        return _node?.GetCurrentProfile();
    }
} 
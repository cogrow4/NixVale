using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nixvale.Core.Debug;
using Nixvale.Core.Node;
using NixvaleNew.Maui.Services;
using System.Collections.ObjectModel;

namespace NixvaleNew.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly NixvaleService _nixvaleService;
    private readonly MauiDebugLogger _logger;

    [ObservableProperty]
    private bool _isNodeRunning;

    [ObservableProperty]
    private string _currentStatus = "Stopped";

    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private NodeState? _currentState;

    public ObservableCollection<LogEntry> RecentLogs => _logger.RecentLogs as ObservableCollection<LogEntry>;

    public MainViewModel(NixvaleService nixvaleService, MauiDebugLogger logger)
    {
        _nixvaleService = nixvaleService;
        _logger = logger;

        // Wire up events
        _nixvaleService.NodeStateChanged += OnNodeStateChanged;
        _nixvaleService.ProfileChanged += OnProfileChanged;
        _nixvaleService.InvitationReceived += OnInvitationReceived;
    }

    [RelayCommand]
    private async Task StartNodeAsync()
    {
        if (IsNodeRunning)
            return;

        try
        {
            var defaultProfile = Profile.CreateNew("New User");
            var defaultConfig = NodeConfiguration.CreateDefaultP2P(
                12345, // Default port
                Path.Combine(FileSystem.AppDataDirectory, "Downloads")
            );

            await _nixvaleService.StartNodeAsync(defaultProfile, defaultConfig);
            IsNodeRunning = true;
            CurrentStatus = "Running";
            CurrentProfile = defaultProfile;
            CurrentState = _nixvaleService.GetCurrentState();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task StopNodeAsync()
    {
        if (!IsNodeRunning)
            return;

        try
        {
            await _nixvaleService.StopNodeAsync();
            IsNodeRunning = false;
            CurrentStatus = "Stopped";
            CurrentProfile = null;
            CurrentState = null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task UpdateProfileAsync(string newDisplayName)
    {
        if (!IsNodeRunning)
            return;

        try
        {
            await _nixvaleService.UpdateProfileAsync(newDisplayName: newDisplayName);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnNodeStateChanged(object? sender, NodeStateEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentState = e.NewState;
            UpdateStatusFromState(e.NewState);
        });
    }

    private void OnProfileChanged(object? sender, ProfileChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentProfile = e.NewProfile;
        });
    }

    private async void OnInvitationReceived(object? sender, NetworkInvitationEventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var accept = await Shell.Current.DisplayAlert(
                "Network Invitation",
                $"You received an invitation to join a network.{(e.InvitationMessage != null ? $"\n\nMessage: {e.InvitationMessage}" : "")}",
                "Accept",
                "Decline"
            );

            if (accept)
            {
                // TODO: Handle network invitation acceptance
            }
        });
    }

    private void UpdateStatusFromState(NodeState state)
    {
        CurrentStatus = (state.IPv4Status, state.IPv6Status) switch
        {
            (ConnectivityStatus.Connected, _) or (_, ConnectivityStatus.Connected) => "Connected",
            (ConnectivityStatus.Connecting, _) or (_, ConnectivityStatus.Connecting) => "Connecting...",
            (ConnectivityStatus.Failed, ConnectivityStatus.Failed) => "Connection Failed",
            _ => "Disconnected"
        };
    }
} 
using System.Net;

namespace Nixvale.Core.Node;

/// <summary>
/// Represents the current state of a Nixvale node
/// </summary>
public record NodeState
{
    /// <summary>
    /// Current active local service port
    /// </summary>
    public int ActiveLocalServicePort { get; init; }

    /// <summary>
    /// IPv4 DHT node identifier
    /// </summary>
    public byte[]? IPv4DhtNodeId { get; init; }

    /// <summary>
    /// IPv6 DHT node identifier
    /// </summary>
    public byte[]? IPv6DhtNodeId { get; init; }

    /// <summary>
    /// Tor DHT node identifier
    /// </summary>
    public byte[]? TorDhtNodeId { get; init; }

    /// <summary>
    /// Total number of nodes in IPv4 DHT
    /// </summary>
    public int IPv4DhtTotalNodes { get; init; }

    /// <summary>
    /// Total number of nodes in IPv6 DHT
    /// </summary>
    public int IPv6DhtTotalNodes { get; init; }

    /// <summary>
    /// Total number of nodes in Tor DHT
    /// </summary>
    public int TorDhtTotalNodes { get; init; }

    /// <summary>
    /// Total number of nodes in LAN DHT
    /// </summary>
    public int LanDhtTotalNodes { get; init; }

    /// <summary>
    /// IPv4 Internet connectivity status
    /// </summary>
    public ConnectivityStatus IPv4Status { get; init; }

    /// <summary>
    /// IPv6 Internet connectivity status
    /// </summary>
    public ConnectivityStatus IPv6Status { get; init; }

    /// <summary>
    /// Whether Tor is currently running
    /// </summary>
    public bool IsTorRunning { get; init; }

    /// <summary>
    /// Tor hidden service endpoint
    /// </summary>
    public EndPoint? TorHiddenEndPoint { get; init; }

    /// <summary>
    /// UPnP device status
    /// </summary>
    public UPnPStatus UPnPStatus { get; init; }

    /// <summary>
    /// UPnP device IP address
    /// </summary>
    public IPAddress? UPnPDeviceIP { get; init; }

    /// <summary>
    /// External IP address provided by UPnP
    /// </summary>
    public IPAddress? UPnPExternalIP { get; init; }

    /// <summary>
    /// External IPv4 endpoint
    /// </summary>
    public EndPoint? IPv4ExternalEndPoint { get; init; }

    /// <summary>
    /// External IPv6 endpoint
    /// </summary>
    public EndPoint? IPv6ExternalEndPoint { get; init; }

    /// <summary>
    /// Creates an initial state for a new node
    /// </summary>
    public static NodeState CreateInitial(int localServicePort) => new()
    {
        ActiveLocalServicePort = localServicePort,
        IPv4Status = ConnectivityStatus.Unknown,
        IPv6Status = ConnectivityStatus.Unknown,
        UPnPStatus = UPnPStatus.Unknown
    };
} 
using System.Net;

namespace Nixvale.Core.Node;

/// <summary>
/// Configuration settings for a Nixvale node
/// </summary>
public record NodeConfiguration
{
    /// <summary>
    /// Type of node (P2P or Anonymous)
    /// </summary>
    public required NodeType Type { get; init; }

    /// <summary>
    /// Port number for the local service
    /// </summary>
    public required ushort LocalServicePort { get; init; }

    /// <summary>
    /// Path to store downloaded files
    /// </summary>
    public required string DownloadFolder { get; init; }

    /// <summary>
    /// Whether to enable UPnP for port forwarding
    /// </summary>
    public bool EnableUPnP { get; init; }

    /// <summary>
    /// Whether to allow inbound invitations
    /// </summary>
    public bool AllowInboundInvitations { get; init; }

    /// <summary>
    /// Whether to only allow invitations from local network
    /// </summary>
    public bool AllowOnlyLocalInboundInvitations { get; init; }

    /// <summary>
    /// Bootstrap DHT nodes for IPv4
    /// </summary>
    public EndPoint[] IPv4BootstrapNodes { get; init; } = Array.Empty<EndPoint>();

    /// <summary>
    /// Bootstrap DHT nodes for IPv6
    /// </summary>
    public EndPoint[] IPv6BootstrapNodes { get; init; } = Array.Empty<EndPoint>();

    /// <summary>
    /// Bootstrap DHT nodes for Tor
    /// </summary>
    public EndPoint[] TorBootstrapNodes { get; init; } = Array.Empty<EndPoint>();

    /// <summary>
    /// Proxy configuration for the node
    /// </summary>
    public ProxyConfiguration? Proxy { get; init; }

    /// <summary>
    /// Creates default configuration for a P2P node
    /// </summary>
    public static NodeConfiguration CreateDefaultP2P(ushort port, string downloadFolder) => new()
    {
        Type = NodeType.P2P,
        LocalServicePort = port,
        DownloadFolder = downloadFolder,
        EnableUPnP = true,
        AllowInboundInvitations = true,
        AllowOnlyLocalInboundInvitations = false
    };

    /// <summary>
    /// Creates default configuration for an anonymous node
    /// </summary>
    public static NodeConfiguration CreateDefaultAnonymous(ushort port, string downloadFolder) => new()
    {
        Type = NodeType.Anonymous,
        LocalServicePort = port,
        DownloadFolder = downloadFolder,
        EnableUPnP = false,
        AllowInboundInvitations = true,
        AllowOnlyLocalInboundInvitations = true
    };
}

/// <summary>
/// Configuration for proxy settings
/// </summary>
public record ProxyConfiguration
{
    /// <summary>
    /// Type of proxy (SOCKS, HTTP, etc.)
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Proxy server address
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Proxy server port
    /// </summary>
    public required ushort Port { get; init; }

    /// <summary>
    /// Optional credentials for proxy authentication
    /// </summary>
    public NetworkCredential? Credentials { get; init; }
} 
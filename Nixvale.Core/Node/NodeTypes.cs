namespace Nixvale.Core.Node;

/// <summary>
/// Defines the type of node in the Nixvale network
/// </summary>
public enum NodeType
{
    /// <summary>
    /// Invalid or uninitialized node
    /// </summary>
    Invalid = 0,

    /// <summary>
    /// Peer-to-peer node with direct connections
    /// </summary>
    P2P = 1,

    /// <summary>
    /// Anonymous node using Tor network
    /// </summary>
    Anonymous = 2
}

/// <summary>
/// Defines the current status of a profile
/// </summary>
public enum ProfileStatus
{
    /// <summary>
    /// Profile has no status set
    /// </summary>
    None = 0,

    /// <summary>
    /// Profile is active and available
    /// </summary>
    Active = 1,

    /// <summary>
    /// Profile is inactive or away
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Profile is busy
    /// </summary>
    Busy = 3
}

/// <summary>
/// Represents the connectivity status for different network types
/// </summary>
public enum ConnectivityStatus
{
    /// <summary>
    /// Network connectivity status is unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Network is connected and working
    /// </summary>
    Connected = 1,

    /// <summary>
    /// Network is connecting or trying to establish connection
    /// </summary>
    Connecting = 2,

    /// <summary>
    /// Network is disconnected
    /// </summary>
    Disconnected = 3,

    /// <summary>
    /// Network connection failed
    /// </summary>
    Failed = 4
}

/// <summary>
/// Represents the status of UPnP device configuration
/// </summary>
public enum UPnPStatus
{
    /// <summary>
    /// UPnP status is unknown
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// UPnP is disabled
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// UPnP device was found and is working
    /// </summary>
    Found = 2,

    /// <summary>
    /// UPnP device was not found
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// UPnP configuration failed
    /// </summary>
    Failed = 4
} 
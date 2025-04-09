using System.Security.Cryptography;

namespace Nixvale.Core.Node;

/// <summary>
/// Represents a user profile in the Nixvale network
/// </summary>
public record Profile
{
    /// <summary>
    /// Unique identifier for the profile
    /// </summary>
    public required byte[] UserId { get; init; }

    /// <summary>
    /// Display name for the profile
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Current status of the profile
    /// </summary>
    public required ProfileStatus Status { get; init; }

    /// <summary>
    /// Optional status message
    /// </summary>
    public string? StatusMessage { get; init; }

    /// <summary>
    /// Last modified timestamp in UTC
    /// </summary>
    public required DateTime LastModifiedUtc { get; init; }

    /// <summary>
    /// Optional profile image
    /// </summary>
    public byte[]? DisplayImage { get; init; }

    /// <summary>
    /// Timestamp when the display image was last modified in UTC
    /// </summary>
    public DateTime? DisplayImageLastModifiedUtc { get; init; }

    /// <summary>
    /// Creates a new profile with updated properties
    /// </summary>
    public Profile WithUpdates(
        string? newDisplayName = null,
        ProfileStatus? newStatus = null,
        string? newStatusMessage = null,
        byte[]? newDisplayImage = null)
    {
        return this with
        {
            DisplayName = newDisplayName ?? DisplayName,
            Status = newStatus ?? Status,
            StatusMessage = newStatusMessage ?? StatusMessage,
            LastModifiedUtc = DateTime.UtcNow,
            DisplayImage = newDisplayImage ?? DisplayImage,
            DisplayImageLastModifiedUtc = newDisplayImage != null ? DateTime.UtcNow : DisplayImageLastModifiedUtc
        };
    }

    /// <summary>
    /// Creates a new profile with a new randomly generated user ID
    /// </summary>
    public static Profile CreateNew(string displayName)
    {
        return new Profile
        {
            UserId = GenerateNewUserId(),
            DisplayName = displayName,
            Status = ProfileStatus.Active,
            LastModifiedUtc = DateTime.UtcNow
        };
    }

    private static byte[] GenerateNewUserId()
    {
        var id = new byte[32]; // 256-bit identifier
        RandomNumberGenerator.Fill(id);
        return id;
    }
} 
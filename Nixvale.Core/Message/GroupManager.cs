using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Nixvale.Core.Message;

/// <summary>
/// Manages group creation, membership, and message routing
/// </summary>
public class GroupManager
{
    private readonly MessageStore _store;
    private readonly byte[] _nodeId;
    private readonly ConcurrentDictionary<Guid, Group> _groups = new();
    private readonly string _groupStorePath;

    public event EventHandler<Group>? GroupCreated;
    public event EventHandler<Group>? GroupUpdated;
    public event EventHandler<(Guid GroupId, byte[] MemberId)>? MemberJoined;
    public event EventHandler<(Guid GroupId, byte[] MemberId)>? MemberLeft;

    public GroupManager(MessageStore store, byte[] nodeId, string groupStorePath)
    {
        _store = store;
        _nodeId = nodeId;
        _groupStorePath = groupStorePath;
        LoadGroups();
    }

    /// <summary>
    /// Creates a new group
    /// </summary>
    public Group CreateGroup(string name, string? description = null)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _nodeId,
            Members = new List<GroupMember>
            {
                new()
                {
                    Id = _nodeId,
                    JoinedAt = DateTime.UtcNow,
                    Role = GroupRole.Admin
                }
            }
        };

        _groups[group.Id] = group;
        SaveGroups();
        GroupCreated?.Invoke(this, group);

        // Broadcast group creation
        var message = Message.CreateGroupJoin(group.Id, _nodeId);
        _store.AddMessage(message);

        return group;
    }

    /// <summary>
    /// Gets a group by ID
    /// </summary>
    public Group? GetGroup(Guid groupId)
    {
        return _groups.TryGetValue(groupId, out var group) ? group : null;
    }

    /// <summary>
    /// Gets all groups the local node is a member of
    /// </summary>
    public IEnumerable<Group> GetGroups()
    {
        return _groups.Values.Where(g => g.Members.Any(m => m.Id.SequenceEqual(_nodeId)));
    }

    /// <summary>
    /// Updates a group's properties
    /// </summary>
    public void UpdateGroup(Guid groupId, string? name = null, string? description = null, byte[]? image = null)
    {
        var group = GetGroup(groupId);
        if (group == null) return;

        var member = group.Members.FirstOrDefault(m => m.Id.SequenceEqual(_nodeId));
        if (member?.Role != GroupRole.Admin) return;

        if (name != null) group.Name = name;
        if (description != null) group.Description = description;
        if (image != null)
        {
            group.Image = image;
            group.ImageLastModifiedUtc = DateTime.UtcNow;
        }

        SaveGroups();
        GroupUpdated?.Invoke(this, group);

        // Broadcast group update
        var message = Message.CreateGroupImage(groupId, image);
        _store.AddMessage(message);
    }

    /// <summary>
    /// Adds a member to a group
    /// </summary>
    public void AddMember(Guid groupId, byte[] memberId, GroupRole role = GroupRole.Member)
    {
        var group = GetGroup(groupId);
        if (group == null) return;

        var currentMember = group.Members.FirstOrDefault(m => m.Id.SequenceEqual(_nodeId));
        if (currentMember?.Role != GroupRole.Admin) return;

        if (group.Members.Any(m => m.Id.SequenceEqual(memberId))) return;

        group.Members.Add(new GroupMember
        {
            Id = memberId,
            JoinedAt = DateTime.UtcNow,
            Role = role
        });

        SaveGroups();
        MemberJoined?.Invoke(this, (groupId, memberId));

        // Broadcast member join
        var message = Message.CreateGroupJoin(groupId, memberId);
        _store.AddMessage(message);
    }

    /// <summary>
    /// Removes a member from a group
    /// </summary>
    public void RemoveMember(Guid groupId, byte[] memberId)
    {
        var group = GetGroup(groupId);
        if (group == null) return;

        var currentMember = group.Members.FirstOrDefault(m => m.Id.SequenceEqual(_nodeId));
        if (currentMember?.Role != GroupRole.Admin) return;

        var member = group.Members.FirstOrDefault(m => m.Id.SequenceEqual(memberId));
        if (member == null) return;

        group.Members.Remove(member);
        SaveGroups();
        MemberLeft?.Invoke(this, (groupId, memberId));

        // Broadcast member leave
        var message = Message.CreateGroupLeave(groupId, memberId);
        _store.AddMessage(message);
    }

    /// <summary>
    /// Leaves a group
    /// </summary>
    public void LeaveGroup(Guid groupId)
    {
        var group = GetGroup(groupId);
        if (group == null) return;

        var member = group.Members.FirstOrDefault(m => m.Id.SequenceEqual(_nodeId));
        if (member == null) return;

        group.Members.Remove(member);
        
        if (group.Members.Count == 0)
        {
            _groups.TryRemove(groupId, out _);
        }
        
        SaveGroups();
        MemberLeft?.Invoke(this, (groupId, _nodeId));

        // Broadcast leave
        var message = Message.CreateGroupLeave(groupId, _nodeId);
        _store.AddMessage(message);
    }

    private void LoadGroups()
    {
        if (!File.Exists(_groupStorePath)) return;

        try
        {
            var json = File.ReadAllText(_groupStorePath);
            var groups = System.Text.Json.JsonSerializer.Deserialize<List<Group>>(json);
            if (groups == null) return;

            foreach (var group in groups)
            {
                _groups[group.Id] = group;
            }
        }
        catch
        {
            // Ignore load errors
        }
    }

    private void SaveGroups()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_groups.Values);
            File.WriteAllText(_groupStorePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}

/// <summary>
/// Represents a group in the network
/// </summary>
public class Group
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required byte[] CreatedBy { get; set; }
    public byte[]? Image { get; set; }
    public DateTime? ImageLastModifiedUtc { get; set; }
    public required List<GroupMember> Members { get; set; }
}

/// <summary>
/// Represents a member of a group
/// </summary>
public class GroupMember
{
    public required byte[] Id { get; set; }
    public required DateTime JoinedAt { get; set; }
    public required GroupRole Role { get; set; }
}

/// <summary>
/// Role of a group member
/// </summary>
public enum GroupRole : byte
{
    Member = 0,
    Admin = 1
} 
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nixvale.Core.Message;
using Nixvale.Core.Node;

namespace NixvaleNew.Maui.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly IMessageStore _messageStore;
    private readonly GroupManager _groupManager;

    [ObservableProperty]
    private ObservableCollection<Message> _messages = new();

    [ObservableProperty]
    private ObservableCollection<ChatListItem> _chats = new();

    [ObservableProperty]
    private ChatListItem? _selectedChat;

    [ObservableProperty]
    private string _messageText = string.Empty;

    public ChatViewModel(IMessageStore messageStore, GroupManager groupManager)
    {
        _messageStore = messageStore;
        _groupManager = groupManager;

        // Subscribe to message events
        _messageStore.MessageReceived += OnMessageReceived;
        _messageStore.MessageSent += OnMessageSent;
        _groupManager.GroupCreated += OnGroupCreated;
        _groupManager.GroupUpdated += OnGroupUpdated;
        _groupManager.MemberAdded += OnMemberAdded;
        _groupManager.MemberRemoved += OnMemberRemoved;

        LoadChats();
    }

    private void LoadChats()
    {
        // Load direct message chats
        foreach (var contact in _messageStore.GetContacts())
        {
            Chats.Add(new ChatListItem
            {
                Id = contact.UserId.ToString(),
                Name = contact.DisplayName,
                LastMessage = _messageStore.GetLastMessage(contact.UserId)?.Content ?? "",
                IsGroup = false,
                Image = contact.DisplayImage
            });
        }

        // Load group chats
        foreach (var group in _groupManager.GetGroups())
        {
            Chats.Add(new ChatListItem
            {
                Id = group.Id.ToString(),
                Name = group.Name,
                LastMessage = _messageStore.GetLastGroupMessage(group.Id)?.Content ?? "",
                IsGroup = true,
                Image = group.Image
            });
        }
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || SelectedChat == null)
            return;

        if (SelectedChat.IsGroup)
        {
            var groupId = Guid.Parse(SelectedChat.Id);
            await _messageStore.SendGroupMessageAsync(groupId, MessageText);
        }
        else
        {
            var recipientId = Convert.FromHexString(SelectedChat.Id);
            await _messageStore.SendMessageAsync(recipientId, MessageText);
        }

        MessageText = string.Empty;
    }

    private void OnMessageReceived(object? sender, Message message)
    {
        if (SelectedChat?.Id == message.SenderId.ToString())
        {
            Messages.Add(message);
        }

        UpdateChatLastMessage(message.SenderId.ToString(), message.Content);
    }

    private void OnMessageSent(object? sender, Message message)
    {
        if (SelectedChat?.Id == message.RecipientId.ToString())
        {
            Messages.Add(message);
        }

        UpdateChatLastMessage(message.RecipientId.ToString(), message.Content);
    }

    private void OnGroupCreated(object? sender, Group group)
    {
        Chats.Add(new ChatListItem
        {
            Id = group.Id.ToString(),
            Name = group.Name,
            IsGroup = true,
            Image = group.Image
        });
    }

    private void OnGroupUpdated(object? sender, Group group)
    {
        var chat = Chats.FirstOrDefault(c => c.Id == group.Id.ToString());
        if (chat != null)
        {
            chat.Name = group.Name;
            chat.Image = group.Image;
        }
    }

    private void OnMemberAdded(object? sender, (Guid GroupId, Profile Member) args)
    {
        // Handle member added event
    }

    private void OnMemberRemoved(object? sender, (Guid GroupId, byte[] MemberId) args)
    {
        // Handle member removed event
    }

    private void UpdateChatLastMessage(string chatId, string message)
    {
        var chat = Chats.FirstOrDefault(c => c.Id == chatId);
        if (chat != null)
        {
            chat.LastMessage = message;
        }
    }

    partial void OnSelectedChatChanged(ChatListItem? value)
    {
        if (value == null)
            return;

        Messages.Clear();
        if (value.IsGroup)
        {
            var groupId = Guid.Parse(value.Id);
            var messages = _messageStore.GetGroupMessages(groupId);
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }
        else
        {
            var contactId = Convert.FromHexString(value.Id);
            var messages = _messageStore.GetMessages(contactId);
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }
    }
}

public class ChatListItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public byte[]? Image { get; set; }
} 
﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public MessageHub(IMessageRepository messageRepository,
    IUserRepository userRepository, IMapper mapper
    )
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var currentUsername = Context.User.GetUsername();
        var otherUsername = httpContext.Request.Query["user"];

        var groupName = GetGroupName(currentUsername, otherUsername);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);

        var messages = await _messageRepository.GetMessageThread(currentUsername, otherUsername);
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await RemoveFromMessageGroup();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = Context.User.GetUsername();
        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
        {
            throw new HubException("You cannot send messages for yourself");
        }
        var sender = await _userRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower()) ?? throw new HubException(string.Format("User {0} was not found", createMessageDto.RecipientUsername));
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = senderUsername,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        // if group is active- all messages will automaticly will be marked as read
        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _messageRepository.GetMessageGroup(groupName);
        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }

        _messageRepository.AddMessage(message);
        if (await _messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("SendMessage", _mapper.Map<MessageDto>(message));
        }
    }

    private static string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);
        return await _messageRepository.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
        var connection = await _messageRepository.GetConnection(Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);
        await _messageRepository.SaveAllAsync();
    }
}
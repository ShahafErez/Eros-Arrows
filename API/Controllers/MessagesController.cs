﻿using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.GetUsername();
        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot send messages for yourself");
        }
        var sender = await _userRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());
        if (recipient == null) return NotFound(String.Format("User {0} was not found", createMessageDto.RecipientUsername));

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = senderUsername,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _messageRepository.AddMessage(message);
        if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Failed to save message");

    }

}
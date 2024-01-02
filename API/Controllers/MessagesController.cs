using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.GetUsername();
        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot send messages for yourself");
        }
        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower());
        if (recipient == null) return NotFound(string.Format("User {0} was not found", createMessageDto.RecipientUsername));

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = senderUsername,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _unitOfWork.MessageRepository.AddMessage(message);
        if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

        return BadRequest("Failed to save message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();
        var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
        return messages;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await _unitOfWork.MessageRepository.GetMessage(id);

        if (message == null) return NotFound(string.Format("Message with ID {0} was not found", id));

        if (message.SenderUsername != username) return Unauthorized();
        if (message.DateRead != null) return BadRequest("You cannot delete a message after it has been read");

        _unitOfWork.MessageRepository.DeleteMessage(message);
        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting message");
    }
}

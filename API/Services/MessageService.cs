using API.DTOs;
using API.Entities;
using API.Errors;
using API.Helpers;
using API.Interfaces;
using AutoMapper;

namespace API.Services;

public class MessageService : IMessageService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public MessageService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> CreateMessage(CreateMessageDto createMessageDto, string senderUsername)
    {
        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
        {
            throw new BadHttpRequestException("You cannot send messages for yourself");
        }

        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername.ToLower()) ?? throw new NotFoundException(string.Format("User {0} was not found", createMessageDto.RecipientUsername));

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = senderUsername,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _unitOfWork.MessageRepository.AddMessage(message);
        if (await _unitOfWork.Complete()) return _mapper.Map<MessageDto>(message);

        throw new BadHttpRequestException("Failed to save message");
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        return await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
    }

    public async Task DeleteMessage(int id, string username)
    {
        var message = await _unitOfWork.MessageRepository.GetMessage(id) ?? throw new NotFoundException(string.Format("Message with ID {0} was not found", id));
        if (message.SenderUsername != username) throw new UnauthorizedAccessException("A message can only be deleted by the sender of the message");
        if (message.DateRead != null) throw new BadHttpRequestException("A message cannot be deleted after it has been read");

        _unitOfWork.MessageRepository.DeleteMessage(message);

        if (!await _unitOfWork.Complete()) throw new BadHttpRequestException("An unexpected problem occured while deleting message");
    }


}

using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageService
{
    Task<MessageDto> CreateMessage(CreateMessageDto createMessageDto, string senderUsername);
    Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
    Task DeleteMessage(int id, string username);

}

using API.Interfaces;
using API.DTOs;
using API.Helpers;
using AutoMapper;
using API.Errors;

namespace API.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedList<MemberDto>> GetUsers(UserParams userParams, string username)
    {
        var currentUser = await _unitOfWork.UserRepository.GetUserGenderByUsername(username);

        // if the desired gender is not specified- the opposite gender will be selected
        if (string.IsNullOrEmpty(userParams.DesiredGender))
        {
            userParams.DesiredGender = currentUser == "male" ? "female" : "male";
        }
        return await _unitOfWork.UserRepository.GetMembersAsync(userParams);
    }


    public async Task<MemberDto> GetUser(string username)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    public async Task UpdateUser(MemberUpdateDto memberUpdateDto, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ??
        throw new NotFoundException(string.Format("User {0} was not found", username));

        _mapper.Map(memberUpdateDto, user);
        if (!await _unitOfWork.Complete()) throw new BadHttpRequestException("Failed to update user");
    }
}

using Form.DTOs;
namespace Form.Interfaces;

public interface IUserService
{
    Task<List<UserSummaryDtos>> GetStudentsAsync();
    Task<CreatedUserDto> CreateUserAsync(CreateUserRequest request);

}
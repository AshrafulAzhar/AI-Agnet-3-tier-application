using System.Collections.Generic;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;

namespace UserManagement.Domain.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request);
        Task<UserResponse> UpdateUserAsync(string id, UserUpdateRequest request);
        Task<UserResponse> GetUserByIdAsync(string id);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    }
}

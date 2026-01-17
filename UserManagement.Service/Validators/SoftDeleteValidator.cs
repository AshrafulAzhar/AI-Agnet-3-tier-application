using System;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Service.Validators
{
    public class SoftDeleteValidator : IValidator<UserRegistrationRequest>
    {
        private readonly IUserRepository _userRepository;

        public SoftDeleteValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(UserRegistrationRequest context)
        {
            var deletedUser = await _userRepository.GetDeletedUserByEmailOrPhoneAsync(context.Email, context.PhoneNumber);
            if (deletedUser != null)
            {
                throw new Exception("This account was previously deleted. Please contact an admin for restoration.");
            }
        }
    }
}

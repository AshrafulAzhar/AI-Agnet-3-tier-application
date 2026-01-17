using System;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Service.Validators
{
    public class EmailUniquenessValidator : IValidator<UserRegistrationRequest>
    {
        private readonly IUserRepository _userRepository;

        public EmailUniquenessValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(UserRegistrationRequest context)
        {
            var existingUser = await _userRepository.GetByEmailAsync(context.Email);
            if (existingUser != null)
            {
                throw new Exception("Email is already in use.");
            }
        }
    }
}

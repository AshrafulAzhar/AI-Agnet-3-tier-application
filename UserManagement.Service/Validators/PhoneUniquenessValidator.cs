using System;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Service.Validators
{
    public class PhoneUniquenessValidator<T> : IValidator<T> where T : IUserProfile
    {
        private readonly IUserRepository _userRepository;

        public PhoneUniquenessValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(T context)
        {
            if (string.IsNullOrEmpty(context.PhoneNumber)) return;

            var existingUser = await _userRepository.GetByPhoneNumberAsync(context.PhoneNumber);
            if (existingUser != null)
            {
                // In case of update, exclude the current user
                if (context is UserUpdateRequest updateRequest && existingUser.Id == updateRequest.Id)
                {
                    return;
                }
                
                throw new Exception("Phone number is already in use by another account.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.Models;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEnumerable<IValidator<UserRegistrationRequest>> _registrationValidators;
        private readonly IEnumerable<IValidator<UserUpdateRequest>> _updateValidators;

        public UserService(
            IUserRepository userRepository, 
            IEnumerable<IValidator<UserRegistrationRequest>> registrationValidators,
            IEnumerable<IValidator<UserUpdateRequest>> updateValidators)
        {
            _userRepository = userRepository;
            _registrationValidators = registrationValidators;
            _updateValidators = updateValidators;
        }

        public async Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request)
        {
            // 1. Normalization
            request.Email = NormalizeEmail(request.Email);
            request.PhoneNumber = NormalizePhone(request.PhoneNumber);

            // 2. Run Registration Validators
            foreach (var validator in _registrationValidators)
            {
                await validator.ValidateAsync(request);
            }

            // 3. Build User Model
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) 
                    ? $"{request.FirstName} {request.LastName}" 
                    : request.DisplayName.Trim(),
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BC.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            return MapToResponse(user);
        }

        public async Task<UserResponse> UpdateUserAsync(string id, UserUpdateRequest request)
        {
            // 0. Ensure user exists
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) throw new Exception("User not found.");

            // 1. Normalization
            request.Id = id; // Set context for uniqueness check
            request.PhoneNumber = NormalizePhone(request.PhoneNumber);

            // 2. Run Update Validators
            foreach (var validator in _updateValidators)
            {
                await validator.ValidateAsync(request);
            }

            // 3. Update User Model
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? $"{request.FirstName} {request.LastName}"
                : request.DisplayName.Trim();
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(id, user);

            return MapToResponse(user);
        }

        public async Task<UserResponse> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null && !user.IsDeleted ? MapToResponse(user) : null;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Where(u => !u.IsDeleted).Select(MapToResponse);
        }

        private string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return email.Trim().ToLowerInvariant();
        }

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0") && digits.Length == 11)
            {
                return "+88" + digits;
            }
            return "+" + digits;
        }

        private UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

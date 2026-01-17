using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.DTOs;
using UserManagement.Domain.Enums;
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
            request.Email = NormalizeEmail(request.Email);
            request.PhoneNumber = NormalizePhone(request.PhoneNumber);

            foreach (var validator in _registrationValidators)
            {
                await validator.ValidateAsync(request);
            }

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
                Role = UserRole.User,
                Status = UserStatus.Active,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            return MapToResponse(user);
        }

        public async Task<UserResponse> UpdateUserAsync(string id, UserUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) throw new Exception("User not found.");

            request.Id = id;
            request.PhoneNumber = NormalizePhone(request.PhoneNumber);

            foreach (var validator in _updateValidators)
            {
                await validator.ValidateAsync(request);
            }

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

        public async Task<UserResponse> UpdateUserRoleStatusAsync(string id, UserRoleStatusUpdateRequest request, string performingUserId)
        {
            // 1. Service Level Authorization
            var performer = await _userRepository.GetByIdAsync(performingUserId);
            if (performer == null || performer.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Only administrators can perform role or status changes.");
            }

            // 2. Fetch Target User
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) throw new Exception("Target user not found.");

            // 3. Process Role Change
            if (request.Role.HasValue && user.Role != request.Role.Value)
            {
                Console.WriteLine($"[AUDIT] User {id} role changed from {user.Role} to {request.Role.Value} by Admin {performingUserId} at {DateTime.UtcNow}");
                user.Role = request.Role.Value;
            }

            // 4. Process Status Change
            if (request.Status.HasValue && user.Status != request.Status.Value)
            {
                Console.WriteLine($"[AUDIT] User {id} status changed from {user.Status} to {request.Status.Value} by Admin {performingUserId} at {DateTime.UtcNow}");
                user.Status = request.Status.Value;
            }

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
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

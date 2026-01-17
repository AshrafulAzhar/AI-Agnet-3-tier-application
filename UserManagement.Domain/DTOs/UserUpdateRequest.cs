using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Domain.DTOs
{
    public class UserUpdateRequest : IUserProfile
    {
        public string Id { get; set; } // Internal use for validation context

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; set; }

        public string DisplayName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}

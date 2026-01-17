using System;

namespace UserManagement.Domain.Interfaces
{
    public interface IUserProfile
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string DisplayName { get; set; }
        string PhoneNumber { get; set; }
        DateTime? DateOfBirth { get; set; }
    }
}

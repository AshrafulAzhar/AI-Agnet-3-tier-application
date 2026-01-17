using System;
using System.Threading.Tasks;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Service.Validators
{
    public class AgePolicyValidator<T> : IValidator<T> where T : IUserProfile
    {
        public Task ValidateAsync(T context)
        {
            var dob = context.DateOfBirth;
            if (dob.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - dob.Value.Year;
                if (dob.Value.Date > today.AddYears(-age)) age--;

                if (age < 13) throw new Exception("Users must be at least 13 years old.");
            }
            return Task.CompletedTask;
        }
    }
}

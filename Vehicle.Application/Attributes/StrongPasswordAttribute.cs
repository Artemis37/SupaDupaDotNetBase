using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vehicle.Application.Attributes
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Password is required");
            }

            var password = value.ToString()!;

            // Check minimum length
            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long");
            }

            // Check for uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter");
            }

            // Check for lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter");
            }

            // Check for digit
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return new ValidationResult("Password must contain at least one digit");
            }

            // Check for special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
            {
                return new ValidationResult("Password must contain at least one special character");
            }

            return ValidationResult.Success;
        }
    }
}

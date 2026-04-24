namespace Sery.Application.Auth;

public static class PasswordValidator
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static PasswordValidationResult Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
        {
            return PasswordValidationResult.TooShort;
        }

        if (password.Length > MaxLength)
        {
            return PasswordValidationResult.TooLong;
        }

        if (!password.Any(char.IsUpper))
        {
            return PasswordValidationResult.MissingUppercase;
        }

        if (!password.Any(char.IsLower))
        {
            return PasswordValidationResult.MissingLowercase;
        }

        if (!password.Any(char.IsDigit))
        {
            return PasswordValidationResult.MissingDigit;
        }

        return PasswordValidationResult.Valid;
    }
}

public enum PasswordValidationResult
{
    Valid = 0,
    TooShort = 1,
    TooLong = 2,
    MissingUppercase = 3,
    MissingLowercase = 4,
    MissingDigit = 5
}

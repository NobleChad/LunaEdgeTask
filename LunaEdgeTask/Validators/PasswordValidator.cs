namespace LunaEdgeTask.Validators
{
    public static class PasswordValidator
    {
        public static bool IsValid(string password)
        {
            // Password must be at least 8 chars, include upper, lower, digit, and special character
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8) return false;
            return password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}

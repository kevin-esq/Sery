namespace Sery.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DisplayName { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }

        DisplayName = displayName.Trim();
    }
}

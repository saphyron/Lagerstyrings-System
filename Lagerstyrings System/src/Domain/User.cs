// Domain/User.cs
public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordClear { get; set; } = string.Empty; // For creation only
    public byte AuthEnum { get; set; }                       // FK to AuthRoles.AuthEnum
}

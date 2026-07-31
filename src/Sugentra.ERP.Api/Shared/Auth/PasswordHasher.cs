namespace Sugentra.ERP.Api.Shared.Auth;

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) => BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public bool Verify(string plainTextPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
}

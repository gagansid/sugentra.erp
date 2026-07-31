using System.Reflection;
using System.Security.Cryptography;
using DbUp;
using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("SUGENTRA_ERP_CONNECTION_STRING")
    ?? (args.Length > 0 ? args[0] : null);

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Missing connection string. Set SUGENTRA_ERP_CONNECTION_STRING env var or pass it as the first argument.");
    return 1;
}

EnsureDatabase.For.SqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains(".Scripts."))
    .JournalToSqlTable("dbo", "SchemaVersionsJournal")
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine("Schema migration succeeded.");

SeedSuperAdmin(connectionString);
return 0;

static void SeedSuperAdmin(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    var existingId = new SqlCommand("SELECT Id FROM Identity_Users WHERE Username = 'superadmin'", connection)
        .ExecuteScalar();
    if (existingId is not null)
    {
        Console.WriteLine("Super Admin user already exists, skipping seed.");
        return;
    }

    // Never hardcode a default password: generate one at random and surface it once so the operator can store it securely.
    var password = Environment.GetEnvironmentVariable("SUGENTRA_ERP_SUPERADMIN_PASSWORD") ?? GenerateRandomPassword();
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

    using var transaction = connection.BeginTransaction();

    var insertUser = new SqlCommand(
        "INSERT INTO Identity_Users (Username, Email, PasswordHash, FullName, IsActive, CreatedBy) " +
        "OUTPUT INSERTED.Id VALUES ('superadmin', 'superadmin@sugentra.local', @PasswordHash, 'Super Administrator', 1, NULL)",
        connection, transaction);
    insertUser.Parameters.AddWithValue("@PasswordHash", passwordHash);
    var userId = (long)insertUser.ExecuteScalar()!;

    var linkRole = new SqlCommand(
        "INSERT INTO Identity_UserRoles (UserId, RoleId, CreatedBy) " +
        "SELECT @UserId, Id, NULL FROM Identity_Roles WHERE Name = 'SuperAdmin'",
        connection, transaction);
    linkRole.Parameters.AddWithValue("@UserId", userId);
    linkRole.ExecuteNonQuery();

    transaction.Commit();

    Console.WriteLine("Seeded Super Admin user 'superadmin'.");
    if (Environment.GetEnvironmentVariable("SUGENTRA_ERP_SUPERADMIN_PASSWORD") is null)
    {
        Console.WriteLine($"Generated password (store this securely, it will not be shown again): {password}");
    }
}

static string GenerateRandomPassword()
{
    const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
    var bytes = RandomNumberGenerator.GetBytes(20);
    var result = new char[20];
    for (var i = 0; i < bytes.Length; i++)
    {
        result[i] = chars[bytes[i] % chars.Length];
    }
    return new string(result);
}

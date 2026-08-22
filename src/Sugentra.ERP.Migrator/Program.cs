using System.Reflection;
using System.Security.Cryptography;
using DbUp;
using DbUp.Engine;
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
    .WithScriptNameComparer(new ScriptModuleOrderComparer())
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
SeedStaffUser(connectionString, "gagansuganda", "gagansuganda@sugentra.local", "Gagan Suganda", "Staff");
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

static void SeedStaffUser(string connectionString, string username, string email, string fullName, string roleName)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    var existingId = new SqlCommand("SELECT Id FROM Identity_Users WHERE Username = @Username", connection);
    existingId.Parameters.AddWithValue("@Username", username);
    if (existingId.ExecuteScalar() is not null)
    {
        Console.WriteLine($"User '{username}' already exists, skipping seed.");
        return;
    }

    var password = Environment.GetEnvironmentVariable("SUGENTRA_ERP_GAGANSUGANDA_PASSWORD") ?? GenerateRandomPassword();
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

    using var transaction = connection.BeginTransaction();

    var insertUser = new SqlCommand(
        "INSERT INTO Identity_Users (Username, Email, PasswordHash, FullName, IsActive, CreatedBy) " +
        "OUTPUT INSERTED.Id VALUES (@Username, @Email, @PasswordHash, @FullName, 1, NULL)",
        connection, transaction);
    insertUser.Parameters.AddWithValue("@Username", username);
    insertUser.Parameters.AddWithValue("@Email", email);
    insertUser.Parameters.AddWithValue("@PasswordHash", passwordHash);
    insertUser.Parameters.AddWithValue("@FullName", fullName);
    var userId = (long)insertUser.ExecuteScalar()!;

    var linkRole = new SqlCommand(
        "INSERT INTO Identity_UserRoles (UserId, RoleId, CreatedBy) " +
        "SELECT @UserId, Id, NULL FROM Identity_Roles WHERE Name = @RoleName",
        connection, transaction);
    linkRole.Parameters.AddWithValue("@UserId", userId);
    linkRole.Parameters.AddWithValue("@RoleName", roleName);
    linkRole.ExecuteNonQuery();

    transaction.Commit();

    Console.WriteLine($"Seeded user '{username}' with role '{roleName}'.");
    if (Environment.GetEnvironmentVariable("SUGENTRA_ERP_GAGANSUGANDA_PASSWORD") is null)
    {
        Console.WriteLine($"Generated password (store this securely, it will not be shown again): {password}");
    }
}

// DbUp's default script ordering sorts ALL embedded scripts alphabetically by full resource name across
// every module folder combined (e.g. "Identity" < "MasterData" < "Settings"). MasterData has FK dependencies
// on reference data owned by Settings (Currencies, UnitsOfMeasurement), so on a truly fresh database the
// plain alphabetical order tries to create MasterData tables before their Settings FK targets exist and fails.
// This comparer keeps each module's own scripts in their existing numeric order (so already-applied
// environments are unaffected - script names/journal entries never change) but runs whole modules in a
// fixed, dependency-safe sequence instead of alphabetical: Identity, then Settings, then MasterData.
sealed class ScriptModuleOrderComparer : IComparer<string>
{
    private static readonly string[] ModuleOrder = { "Identity", "Settings", "MasterData", "Approvals", "Inventory" };

    public int Compare(string? x, string? y)
    {
        var priorityX = GetModulePriority(x);
        var priorityY = GetModulePriority(y);
        return priorityX != priorityY
            ? priorityX.CompareTo(priorityY)
            : string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetModulePriority(string? resourceName)
    {
        if (resourceName is null) return ModuleOrder.Length;
        var marker = ".Scripts.";
        var idx = resourceName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return ModuleOrder.Length;
        var rest = resourceName[(idx + marker.Length)..];
        var dot = rest.IndexOf('.');
        var module = dot < 0 ? rest : rest[..dot];
        var pos = Array.IndexOf(ModuleOrder, module);
        return pos < 0 ? ModuleOrder.Length : pos;
    }
}

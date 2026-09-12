using System.Diagnostics;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sugentra.ERP.Api.Modules.Identity;
using Sugentra.ERP.Api.Modules.Inventory;
using Sugentra.ERP.Api.Modules.MasterData;
using Sugentra.ERP.Api.Modules.Settings;
using Sugentra.ERP.Api.Modules.Approvals;
using Sugentra.ERP.Api.Modules.Procurement;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Middleware;
using Sugentra.ERP.Api.Shared.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Dev convenience: a previous `dotnet run` left running (e.g. terminal closed without Ctrl+C) would otherwise
// block this run with "address already in use" — free the configured port(s) first, Development only.
if (builder.Environment.IsDevelopment())
{
    FreeDevelopmentPorts();
}

// Add services to the container.

builder.Services.AddControllers();
// Auto-validates action parameters against registered FluentValidation validators and folds failures into
// ModelState, so they flow through the existing InvalidModelStateResponseFactory (422 ApiResponse) below.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Sugentra.ERP.Api.Modules.Procurement.Validators.CreatePurchaseRequisitionRequestValidator>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the access token only — the 'Bearer ' prefix is added automatically."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Replaces the default ValidationProblemDetails 400 with the same ApiResponse envelope used everywhere else.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        var response = new ApiResponse<object?>
        {
            Success = false,
            Message = "One or more validation errors occurred.",
            Errors = errors
        };

        return new ObjectResult(response) { StatusCode = StatusCodes.Status422UnprocessableEntity };
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Required by UseExceptionHandler() even though GlobalExceptionHandler writes the response itself (ApiResponse, not ProblemDetails).
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<Sugentra.ERP.Api.Shared.Persistence.GenericRepository<Sugentra.ERP.Api.Shared.Uploads.UploadFile>>();
builder.Services.AddScoped<Sugentra.ERP.Api.Shared.Uploads.IFileStorageService, Sugentra.ERP.Api.Shared.Uploads.FileStorageService>();
builder.Services.AddScoped<Sugentra.ERP.Api.Shared.Persistence.GenericRepository<Sugentra.ERP.Api.Shared.ErrorLogging.ErrorLog>>();
builder.Services.AddScoped<Sugentra.ERP.Api.Shared.ErrorLogging.IErrorLogService, Sugentra.ERP.Api.Shared.ErrorLogging.ErrorLogService>();
builder.Services.AddScoped<Sugentra.ERP.Api.Shared.ErrorLogging.ErrorLogQuery>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AccountLockoutOptions>(builder.Configuration.GetSection(AccountLockoutOptions.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };

        // Default JwtBearer/Authorization behavior returns an empty 401/403 body — replace both with the
        // same ApiResponse envelope used everywhere else so clients can distinguish "not logged in" (401)
        // from "logged in but missing permission" (403) instead of getting silence.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return WriteApiResponseAsync(context.Response, StatusCodes.Status401Unauthorized,
                    "Authentication required. Please log in again.");
            },
            OnForbidden = context =>
                WriteApiResponseAsync(context.Response, StatusCodes.Status403Forbidden,
                    "You do not have permission to perform this action.")
        };
    });

// Dynamic permission-based authorization: [Authorize(Policy = "PermissionCode")] resolved on demand,
// no per-permission policy registration needed — see PermissionPolicyProvider/PermissionAuthorizationHandler.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

builder.Services.AddIdentityModule();
builder.Services.AddSettingsModule();
builder.Services.AddMasterDataModule();
builder.Services.AddApprovalsModule();
builder.Services.AddInventoryModule();
builder.Services.AddProcurementModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

// Serves uploaded files (wwwroot/uploads/**) - public logos/icons are read directly via this static URL.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static Task WriteApiResponseAsync(HttpResponse response, int statusCode, string message)
{
    response.StatusCode = statusCode;
    response.ContentType = "application/json";
    var body = new ApiResponse<object?> { Success = false, Message = message };
    return response.WriteAsJsonAsync(body);
}

static void FreeDevelopmentPorts()
{
    // lsof-based; only macOS/Linux dev machines are in scope here (not a production concern).
    if (!OperatingSystem.IsMacOS() && !OperatingSystem.IsLinux())
    {
        return;
    }

    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    if (string.IsNullOrWhiteSpace(urls))
    {
        return;
    }

    var ports = urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(u => Uri.TryCreate(u, UriKind.Absolute, out var uri) ? uri.Port : (int?)null)
        .Where(p => p.HasValue)
        .Select(p => p!.Value)
        .Distinct();

    foreach (var port in ports)
    {
        KillProcessListeningOnPort(port);
    }
}

static void KillProcessListeningOnPort(int port)
{
    try
    {
        using var lookup = Process.Start(new ProcessStartInfo("lsof", $"-ti tcp:{port}") { RedirectStandardOutput = true, UseShellExecute = false });
        if (lookup is null)
        {
            return;
        }

        var output = lookup.StandardOutput.ReadToEnd();
        lookup.WaitForExit();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(line.Trim(), out var pid) || pid == Environment.ProcessId)
            {
                continue;
            }

            try
            {
                Process.GetProcessById(pid).Kill(entireProcessTree: true);
                Console.WriteLine($"Freed port {port}: killed leftover process {pid}.");
            }
            catch
            {
                // Already exited between lsof snapshot and Kill() — ignore.
            }
        }
    }
    catch
    {
        // lsof unavailable or failed — non-fatal, Kestrel will surface the real bind error if the port is still busy.
    }
}

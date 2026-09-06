using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Sugentra.ERP.UI.Auth;
using Sugentra.ERP.UI.Services;
using Sugentra.ERP.UI.Services.Approvals;
using Sugentra.ERP.UI.Services.Identity;
using Sugentra.ERP.UI.Services.Inventory;
using Sugentra.ERP.UI.Services.MasterData;
using Sugentra.ERP.UI.Services.Settings;
using Sugentra.ERP.UI.Services.Shared;

DevPortKiller.KillStaleListeners(5037, 7022);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14); // matches API refresh token lifetime
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<RoleApiService>();
builder.Services.AddScoped<PermissionApiService>();
builder.Services.AddScoped<BusinessPartnerApiService>();
builder.Services.AddScoped<ItemApiService>();
builder.Services.AddScoped<PriceListApiService>();
builder.Services.AddScoped<BillOfMaterialApiService>();
builder.Services.AddScoped<CurrencyApiService>();
builder.Services.AddScoped<UnitOfMeasurementApiService>();
builder.Services.AddScoped<WarehouseApiService>();
builder.Services.AddScoped<DocumentNumberingApiService>();
builder.Services.AddScoped<BatchApiService>();
builder.Services.AddScoped<GoodsReceiptApiService>();
builder.Services.AddScoped<LandedCostAllocationApiService>();
builder.Services.AddScoped<LandedCostDocumentApiService>();
builder.Services.AddScoped<QuarantineHoldApiService>();
builder.Services.AddScoped<InventoryStatsApiService>();
builder.Services.AddScoped<StockBalanceApiService>();
builder.Services.AddScoped<StockLedgerApiService>();
builder.Services.AddScoped<StockMutationApiService>();
builder.Services.AddScoped<StockOpnameApiService>();
builder.Services.AddScoped<ApprovalFlowApiService>();
builder.Services.AddScoped<ApprovalRequestApiService>();
builder.Services.AddScoped<ApprovalRoleCategoryApiService>();
builder.Services.AddScoped<AuditLogApiService>();
builder.Services.AddScoped<ModuleApiService>();
builder.Services.AddScoped<ModuleIconLookupService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<HolidayApiService>();
builder.Services.AddScoped<MenuApiService>();
builder.Services.AddScoped<CompanyProfileApiService>();
builder.Services.AddScoped<UploadApiService>();
builder.Services.AddScoped<EmailSettingApiService>();
builder.Services.AddScoped<EmailTemplateApiService>();
builder.Services.AddScoped<EmailTemplateParameterApiService>();
builder.Services.AddScoped<SystemParameterApiService>();
builder.Services.AddScoped<ParamFormatOptionApiService>();
builder.Services.AddScoped<EmailHistoryApiService>();
builder.Services.AddScoped<ErrorLogApiService>();

var app = builder.Build();

// Always show a generic "contact the developer" page instead of the raw exception - even in Development -
// so end users never see a stack trace; full details are captured server-side to Shared_ErrorLogs (see ErrorController).
app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "workspace-index",
    pattern: "Workspace/{id}",
    defaults: new { controller = "Workspace", action = "Index" });

// Lets module-scoped pages (Users, Roles, ...) be reached/generated as Workspace/{moduleCode}/{controller}/...
// instead of their bare {controller}/{action} URL - moduleCode is purely cosmetic, routing still resolves by
// controller name. Must come before "default" so link generation prefers it when moduleCode is supplied
// (see asp-route-moduleCode in _MenuNode.cshtml), and after "workspace-index" so /Workspace/{id} isn't
// swallowed by the {controller} segment here.
app.MapControllerRoute(
    name: "workspace-module",
    pattern: "Workspace/{moduleCode}/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

/// <summary>Dev-time convenience: kills any stale process still bound to our dev ports before Kestrel
/// starts, so a leftover process from a previous "dotnet run" never causes an AddressInUse crash.</summary>
static class DevPortKiller
{
    public static void KillStaleListeners(params int[] ports)
    {
        if (!OperatingSystem.IsMacOS() && !OperatingSystem.IsLinux()) return;

        foreach (var port in ports)
        {
            try
            {
                var lsof = Process.Start(new ProcessStartInfo("lsof", $"-tiTCP:{port} -sTCP:LISTEN")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
                if (lsof is null) continue;

                var output = lsof.StandardOutput.ReadToEnd();
                lsof.WaitForExit();

                var currentPid = Environment.ProcessId;
                foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (int.TryParse(line.Trim(), out var pid) && pid != currentPid)
                    {
                        Process.Start("kill", $"-9 {pid}")?.WaitForExit();
                    }
                }
            }
            catch
            {
                // Best-effort only — if lsof/kill aren't available, Kestrel will surface its own bind error.
            }
        }
    }
}


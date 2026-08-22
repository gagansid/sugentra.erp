using Microsoft.AspNetCore.Mvc;

namespace Sugentra.ERP.UI.Controllers;

/// <summary>Static UI-kit reference for the plain Bootstrap table styles used across the app
/// (no DataTables JS — see templates/AspnetCoreMvcFull/Views/Tables/Basic.cshtml for the source styles).</summary>
public class TablesController : Controller
{
    public IActionResult Basic() => View();
}

using Microsoft.AspNetCore.Http;
using Sugentra.ERP.UI.Models;

namespace Sugentra.ERP.UI.Services;

/// <summary>Thin wrapper around the API's generic api/uploads endpoint - any feature needing file upload uses this.</summary>
public class UploadApiService(ApiClient apiClient)
{
    public Task<ApiResult<UploadResult>> UploadAsync(IFormFile file, string category, string? replaceUrl = null)
    {
        var fields = new Dictionary<string, string> { ["category"] = category };
        if (!string.IsNullOrEmpty(replaceUrl))
        {
            fields["replaceUrl"] = replaceUrl;
        }

        return apiClient.PostFileAsync<UploadResult>("api/uploads", file, fields);
    }
}

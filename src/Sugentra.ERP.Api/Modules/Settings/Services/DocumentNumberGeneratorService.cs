using Sugentra.ERP.Api.Modules.Settings.Repositories;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Settings.Services;

/// <summary>Thin wrapper so callers (future Sales/Export Documentation modules) never need to know about the stored proc directly.</summary>
public class DocumentNumberGeneratorService(IDocumentNumberingRepository repository) : IDocumentNumberGeneratorService
{
    public async Task<Shared.Contracts.NextDocumentNumberResult> GetNextAsync(string documentType)
    {
        var result = await repository.GetNextAsync(documentType);
        return new Shared.Contracts.NextDocumentNumberResult(result.CurrentNumber, result.FormattedNumber);
    }
}

namespace Sugentra.ERP.Api.Shared.Contracts;

public record NextDocumentNumberResult(int CurrentNumber, string FormattedNumber);

/// <summary>Cross-module contract for generating the next running document number (e.g. StockMutation, StockOpname).
/// Implemented by Settings (backed by usp_Setting_DocumentNumbering_GetNext) — callers never touch the stored proc directly.</summary>
public interface IDocumentNumberGeneratorService
{
    Task<NextDocumentNumberResult> GetNextAsync(string documentType);
}

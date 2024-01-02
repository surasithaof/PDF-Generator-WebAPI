namespace PDF.Generator.WebAPI.Services.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdf(string templateFullPath, object templateData, string? headerText = null, bool hasPageNumber = true);

    byte[] ConcatenatePdfs(IEnumerable<byte[]> documents);

    byte[] StampMergedPdfNumber(byte[] document);
}
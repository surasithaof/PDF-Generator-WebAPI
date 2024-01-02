namespace PDF.Generator.WebAPI.Services.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfFromTemplate(string templateFullPath, object templateData, string? headerText = null, bool hasPageNumber = true);
    Task<byte[]> GeneratePdfFromSvgContent(string svgContent);
    byte[] ConcatenatePdfs(IEnumerable<byte[]> documents);

    byte[] StampMergedPdfNumber(byte[] document);
}
namespace PDF.Generator.WebAPI.Services.Interfaces;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateCustomerDetailReportAsync<T>(List<T> dataList, string templateFileName, bool saveAsFile,
        string outputFileName = null, bool hasPageNumber = true);

    byte[] ConcatenatePdfs(IEnumerable<byte[]> documents);
}
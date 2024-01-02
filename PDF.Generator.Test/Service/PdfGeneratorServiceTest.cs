
using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using PDF.Generator.Test.Models;
using PDF.Generator.WebAPI.Services;
using PDF.Generator.WebAPI.Services.Interfaces;

namespace PDF.Generator.Test;

public class PdfGeneratorServiceTest
{
    private IConfiguration _configuration;
    private IPdfGeneratorService _pdfGeneratorService;

    [SetUp]
    public void Setup()
    {
        _pdfGeneratorService = new PdfGeneratorService(NullLogger<PdfGeneratorService>.Instance);

        var inMemorySettings = new List<KeyValuePair<string, string?>> {
            new KeyValuePair<string, string?>("ReportTemplatePath", "/Users/surasith.kae/Projects/PDF-Generator-WebAPI/templates"),
            new KeyValuePair<string, string?>("ReportOutputPath", "/Users/surasith.kae/Projects/PDF-Generator-WebAPI/output")
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Test]
    public void TestGeneratePdfFromTemplate()
    {
        string templateName = "greeting-template.html";
        List<GreetingModel> greetings = new List<GreetingModel>()
            {
                new GreetingModel("French", "Bonjour", "Salut"),
                new GreetingModel("Spanish", "Hola", "¿Qué tal? (What’s up?)"),
                new GreetingModel("Italian", "Buongiorno", "Ciao"),
                new GreetingModel("Chinese", "你好!", null),
                new GreetingModel("Bulgarian", "Здравей!", "Здравейте!"),
                new GreetingModel("Japanese", "こんにちは!", "おーい!"),
                new GreetingModel("Hebrew", "!שלום", null),
                new GreetingModel("Hindi", "नमस्ते", null),
                new GreetingModel("Korean", "안영하세요", null),
                new GreetingModel("Thai", "สวัสดี", null),
            };

        var templateDataObject = new { Greetings = greetings };

        string templatePath = _configuration.GetValue<string>("ReportTemplatePath") ?? "../templates";
        string templateFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, templateName);

        byte[] pdfResult = Array.Empty<byte>();
        Assert.DoesNotThrowAsync(async () =>
        {
            pdfResult = await _pdfGeneratorService.GeneratePdfFromTemplate(templateFullPath: templateFullPath,
                templateData: templateDataObject);
        });

        // Save the pdf to the disk
        string outputPath = _configuration.GetValue<string>("ReportOutputPath") ?? "../output";
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, outputPath))) Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, outputPath));
        string outputFileName = $"test_output_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.pdf";
        string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

        Assert.DoesNotThrowAsync(async () =>
        {
            await File.WriteAllBytesAsync(outputFullPath, pdfResult);
        });
    }

    [Test]
    public void TestGeneratePdfFromSvgContent()
    {
        string templateName = "mock-certificate.svg";
        string templatePath = _configuration.GetValue<string>("ReportTemplatePath") ?? "../templates";
        string templateFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, templateName);
        string svgContent = File.ReadAllText(templateFullPath);

        var signatureFileUrl = "mock-signature.png";
        string signatureFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, signatureFileUrl);
        var signatureBytes = File.ReadAllBytes(signatureFullPath);
        string signatureFileBase64 = Convert.ToBase64String(signatureBytes);
        string signatureFileObjectURL = "data:image/png;base64," + signatureFileBase64;

        string finishDate = DateTimeOffset.Now.ToString("dd MMMM พ.ศ. yyyy", new CultureInfo("th-TH"));

        svgContent = svgContent.Replace("{{userFullName}}", "FirstName LastName")
                                .Replace("{{courseName}}", "Test Course ทดสอบ")
                                .Replace("{{finishCourseDate}}", finishDate)
                                .Replace("{{courseHours}}", "8")
                                .Replace("{{certifierFullName}}", "Test CertifierName")
                                .Replace("{{certifierPosition}}", "Test CertifierPostion")
                                .Replace("{{signatureImageURL}}", signatureFileObjectURL);

        byte[] pdfResult = Array.Empty<byte>();
        Assert.DoesNotThrowAsync(async () =>
        {
            pdfResult = await _pdfGeneratorService.GeneratePdfFromSvgContent(svgContent);
        });

        // Save the pdf to the disk
        string outputPath = _configuration.GetValue<string>("ReportOutputPath") ?? "../output";
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, outputPath))) Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, outputPath));
        string outputFileName = $"test_output_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.pdf";
        string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

        Assert.DoesNotThrowAsync(async () =>
        {
            await File.WriteAllBytesAsync(outputFullPath, pdfResult);
        });
    }
}
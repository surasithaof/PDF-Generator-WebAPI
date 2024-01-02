using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PDF.Generator.WebAPI.Services.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Scriban;

namespace PDF.Generator.WebAPI.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly ILogger<PdfGeneratorService> _logger;
    public PdfGeneratorService(ILogger<PdfGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfFromTemplate(string templateFullPath, object templateData, string? headerText = null,
        bool hasPageNumber = true)
    {
        try
        {
            var templateContent = await File.ReadAllTextAsync(templateFullPath);
            var template = Template.Parse(templateContent);
            var pageContent = await template.RenderAsync(templateData);

            var dataUrl = "data:text/html;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(pageContent));

            //Generate PDF using Puppeteer
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] {
                        "--no-sandbox"
                    }
                });

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(dataUrl);

            const string headerStyle = "\"" +
                                           "font-family:'Sarabun';" +
                                           "font-size:11px; " +
                                           "width: 100%;" +
                                           "padding-right: 55px;" +
                                           "padding-left: 55px;" +
                                           "margin-right: auto;" +
                                           "margin-left: auto;" +
                                           "margin-top: 10px;" +
                                       "\"";

            const string footerStyle = "\"" +
                                           "font-family:'Sarabun';" +
                                           "text-align: right;" +
                                           "font-size: 11px;" +
                                           "width: 100%;" +
                                           "padding-right: 55px;" +
                                           "padding-left: 55px;" +
                                           "margin-right: auto;" +
                                           "margin-left: auto;" +
                                           "margin-bottom: 10px;" +
                                       "\"";

            var output = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                DisplayHeaderFooter = true,
                MarginOptions = new MarginOptions { Top = "80px", Right = "20px", Bottom = "80px", Left = "20px" },
                PreferCSSPageSize = true,
                HeaderTemplate = "<div id=\"header-template\" style=" + headerStyle + ">" + headerText + "</div>",
                FooterTemplate =
                    "<div id=\"footer-template\" style=" + footerStyle + ">" + (hasPageNumber
                        ? "<span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>"
                        : ""),
                PrintBackground = true
            });

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError("Generate PDF from template error: {ex}", ex);
            throw;
        }
    }

    public byte[] ConcatenatePdfs(IEnumerable<byte[]> documents)
    {
        try
        {
            using (var ms = new MemoryStream())
            {
                var outputDocument = new Document();
                var writer = new PdfCopy(outputDocument, ms);
                outputDocument.Open();

                foreach (var doc in documents)
                {
                    var reader = new PdfReader(doc);
                    for (var i = 1; i <= reader.NumberOfPages; i++)
                    {
                        writer.AddPage(writer.GetImportedPage(reader, i));
                    }

                    writer.FreeReader(reader);
                    reader.Close();
                }

                writer.Close();
                outputDocument.Close();
                var allPagesContent = ms.GetBuffer();
                ms.Flush();

                return allPagesContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Concatenate PDFs error: {ex}", ex);
            throw;
        }
    }

    public byte[] StampMergedPdfNumber(byte[] document)
    {
        try
        {
            byte[] output;
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(document);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    const float xLocation = 50;
                    const float yLocation = 20;

                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        // Font blackFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
                        var bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font font = new Font(bf, 11, Font.NORMAL, BaseColor.BLACK);

                        PdfContentByte pdfContent = stamper.GetOverContent(page);
                        Rectangle mediaBox = reader.GetPageSize(page);

                        ColumnText.ShowTextAligned(pdfContent, Element.ALIGN_LEFT,
                            new Phrase($"{page} of {reader.NumberOfPages}", font), mediaBox.Width - xLocation,
                            yLocation, 0);
                    }
                }

                output = stream.ToArray();
            }

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError("Stamp merged PDFs error: {ex}", ex);
            throw;
        }
    }

    public async Task<byte[]> GeneratePdfFromSvgContent(string svgContent)
    {
        try
        {
            byte[] pdfResult = Array.Empty<byte>();

            var encoding = new UnicodeEncoding();
            var svgUrl = "data:image/svg+xml;base64," + Convert.ToBase64String(encoding.GetBytes(svgContent));

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = new[] {
                        "--no-sandbox"
                    }
                });

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(svgUrl);
            pdfResult = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                Landscape = true,
                DisplayHeaderFooter = false,
                MarginOptions = new MarginOptions
                {
                    Top = "0px",
                    Right = "0px",
                    Bottom = "0px",
                    Left = "0px"
                },
                PreferCSSPageSize = true,
                PrintBackground = true
            });

            return pdfResult;
        }
        catch (Exception ex)
        {
            _logger.LogError("Generate PDF from SVG content error: {ex}", ex);
            throw;
        }
    }
}
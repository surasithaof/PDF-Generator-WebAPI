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
    private readonly IConfiguration _configuration;
    
    public PdfGeneratorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<byte[]> GenerateCustomerDetailReportAsync<T>(List<T> dataList, string templateFileName, bool saveAsFile, string outputFileName = null, bool hasPageNumber = true)
    {
        try
        {
            // Generate Report as HTML using Scriban template
            string templatePath = _configuration.GetValue<string>("ReportTemplatePath");
            string templateFullPath = Path.Combine(templatePath, templateFileName);

            var templateContent = File.ReadAllText(templateFullPath);
            var template = Template.Parse(templateContent);

            var templateData = new { DataList = dataList };
            
            var pageContent = await template.RenderAsync(templateData);

            var dataUrl = "data:text/html;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(pageContent));

            //Generate PDF using Puppeteer
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions {Headless = true});
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(dataUrl);

            string headerStyle = "" +
                                 "\"" +
                                 "font-family:'Sarabun'; " +
                                 "font-size:11px; " +
                                 "width: 100%;" +
                                 "padding-right: 55px;" +
                                 "padding-left: 55px;" +
                                 "margin-right: auto;" +
                                 "margin-left: auto;" +
                                 "margin-top: 10px;" +
                                 "\"";

            string footerStyle = "\"" +
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
                MarginOptions = new MarginOptions
                {
                    Top = "80px",
                    Right = "20px",
                    Bottom = "80px",
                    Left = "20px"
                },
                PreferCSSPageSize = true,
                HeaderTemplate = "<div id=\"header-template\" style=" + headerStyle + ">" + "Example Report" +"</div>",
                FooterTemplate = "<div id=\"footer-template\" style=" + footerStyle + ">" + (hasPageNumber ? "<span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>" : "")
            });

            // Save the pdf to the disk
            if (saveAsFile)
            {
                string outputPath = _configuration.GetValue<string>("ReportOutputPath");
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                outputFileName = !String.IsNullOrEmpty(outputFileName) ? outputFileName : "output.pdf";
                string outputFullPath = Path.Combine(outputPath, outputFileName);

                await File.WriteAllBytesAsync(outputFullPath, output);
            }
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, ex);
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
            Console.WriteLine(ex.Message, ex);
            throw;
        }
    }
    
    public byte[] StampMergedPdfNumberAfter(byte[] document)
    {
        try
        {
            byte[] output;
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(document);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    float xLocation = 50;
                    float yLocation = 20;
                    
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        // Font blackFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
                        var bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font font = new Font(bf, 11, Font.NORMAL, BaseColor.BLACK);

                        PdfContentByte pdfContent = stamper.GetOverContent(page);
                        Rectangle mediabox = reader.GetPageSize(page);

                        ColumnText.ShowTextAligned(pdfContent, Element.ALIGN_LEFT, new Phrase($"{page} of {reader.NumberOfPages}", font), mediabox.Width - xLocation, yLocation, 0);
                    }
                }
                output = stream.ToArray();
            }
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, ex);
            throw;
        }
    }

}
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PDF.Generator.WebAPI.Models.PdfGenerator;
using PDF.Generator.WebAPI.Services.Interfaces;

namespace PDF.Generator.WebAPI.Controllers;

[Route("pdfs")]
[Produces(MediaTypeNames.Application.Json)]
public class PdfGeneratorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PdfGeneratorController> _logger;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public PdfGeneratorController(
        IConfiguration configuration,
        ILogger<PdfGeneratorController> logger,
        IPdfGeneratorService pdfGeneratorService)
    {
        _configuration = configuration;
        _logger = logger;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpPost("generate-from-template")]
    public async Task<IActionResult> GeneratePDF([FromBody] GeneratePdfReq req)
    {
        try
        {
            string templatePath = _configuration.GetValue<string>("ReportTemplatePath");
            string templateFullPath = Path.Combine(Environment.CurrentDirectory, templatePath, req.TemplateName);
            byte[] pdfResult = await _pdfGeneratorService.GeneratePdfFromTemplate(templateFullPath: templateFullPath,
                templateData: req.TemplateDataObject, headerText: req.HeaderText,
                hasPageNumber: req.HasPageNumber ?? true);

            // Save the pdf to the disk
            if (req.SaveAsFile == true)
            {
                string outputPath = _configuration.GetValue<string>("ReportOutputPath");
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                string outputFileName = !String.IsNullOrEmpty(req.OutputFileName) ? req.OutputFileName : "output.pdf";
                string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

                await System.IO.File.WriteAllBytesAsync(outputFullPath, pdfResult);
            }

            var result = new { statusCode = HttpStatusCode.OK, success = true, message = "Success", data = pdfResult };
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Generate PDF error: {exception}", ex);

            var result = new
            {
                statusCode = HttpStatusCode.NotImplemented,
                success = false,
                message = "the server has an error, please try again later.",
            };
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergePDF([FromBody] MergePDFReq req)
    {
        try
        {
            byte[] pdfResult = Array.Empty<byte>();
            byte[] mergedResult = _pdfGeneratorService.ConcatenatePdfs(req.Documents);
            pdfResult = mergedResult;
            if (req.HasPageNumber == true)
            {
                byte[] stampPageNumberResult = _pdfGeneratorService.StampMergedPdfNumber(mergedResult);
                pdfResult = stampPageNumberResult;
            }

            // Save the pdf to the disk
            if (req.SaveAsFile == true)
            {
                string outputPath = _configuration.GetValue<string>("ReportOutputPath");
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                string outputFileName = !String.IsNullOrEmpty(req.OutputFileName) ? req.OutputFileName : "output.pdf";
                string outputFullPath = Path.Combine(Environment.CurrentDirectory, outputPath, outputFileName);

                await System.IO.File.WriteAllBytesAsync(outputFullPath, pdfResult);
            }

            var result = new { statusCode = HttpStatusCode.OK, success = true, message = "Success", data = pdfResult };
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Merge PDF error: {exception}", ex);

            var result = new
            {
                statusCode = HttpStatusCode.InternalServerError,
                success = false,
                message = "the server has an error, please try again later.",
            };
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }
    }
}
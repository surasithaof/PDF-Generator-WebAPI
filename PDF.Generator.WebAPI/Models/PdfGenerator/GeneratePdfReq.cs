using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PDF.Generator.WebAPI.Models.PdfGenerator;

public class GeneratePdfReq
{
    [Required]
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = "";

    [JsonPropertyName("headerText")]
    public string? HeaderText { get; set; }

    [JsonPropertyName("hasPageNumber")]
    public bool? HasPageNumber { get; set; }

    [JsonPropertyName("saveAsFile")]
    public bool? SaveAsFile { get; set; }

    [JsonPropertyName("outputFileName")]
    public string? OutputFileName { get; set; }

    [Required]
    [JsonPropertyName("templateDataObject")]
    public object TemplateDataObject { get; set; } = new object();
}
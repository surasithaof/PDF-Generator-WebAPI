using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PDF.Generator.WebAPI.Models.PdfGenerator;

public class MergePDFReq
{
    [Required]
    [JsonPropertyName("documents")]
    public IEnumerable<byte[]> Documents { get; set; } = new List<byte[]>();

    [JsonPropertyName("hasPageNumber")]
    public bool? HasPageNumber { get; set; }

    [JsonPropertyName("saveAsFile")]
    public bool? SaveAsFile { get; set; }

    [JsonPropertyName("outputFileName")]
    public string? OutputFileName { get; set; }
}
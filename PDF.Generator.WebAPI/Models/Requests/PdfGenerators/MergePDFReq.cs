using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PDF.Generator.WebAPI.Models.Requests.PdfGenerators;

public class MergePDFReq
{
    [Required]
    [JsonPropertyName("documents")]
    public IEnumerable<byte[]> Documents { get; set; }
    
    [JsonPropertyName("hasPageNumber")]
    public bool? HasPageNumber { get; set; }
    
    [JsonPropertyName("saveAsFile")]
    public bool? SaveAsFile { get; set; }
    
    [JsonPropertyName("outputFileName")]
    public string? OutputFileName { get; set; }
}
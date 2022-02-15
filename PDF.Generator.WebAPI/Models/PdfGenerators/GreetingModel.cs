namespace PDF.Generator.WebAPI.Models.PdfGenerators;

public class GreetingModel
{
    public GreetingModel(string language, string greetingTextFormal, string greetingTextInformal)
    {
        this.Language = language;
        this.GreetingTextFormal = greetingTextFormal;
        this.GreetingTextInformal = greetingTextInformal;
    }
    public string Language { get; set; }
    public string GreetingTextFormal { get; set; }
    public string? GreetingTextInformal { get; set; }
}
namespace Statesman.BillScraper.Utilities.ImageScanner;

public interface IGoogleCloudVisionService
{
    Task<string> ExtractTextFromImage(byte[] imageBytes, string languageHint = "bg");
}

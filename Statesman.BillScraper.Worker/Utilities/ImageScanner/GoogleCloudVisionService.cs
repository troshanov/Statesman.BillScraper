using Google.Cloud.Vision.V1;

namespace Statesman.BillScraper.Utilities.ImageScanner;

public class GoogleCloudVisionService : IGoogleCloudVisionService
{
    private readonly ImageAnnotatorClient _client;

    public GoogleCloudVisionService()
    {
        _client = ImageAnnotatorClient.Create();
    }

    public async Task<string> ExtractTextFromImage(byte[] imageBytes, string languageHint = "bg")
    {
        try
        {
            Image image = Image.FromBytes(imageBytes);
            var imageContext = new ImageContext
            {
                LanguageHints = { languageHint }
            };

            var response = await _client.DetectDocumentTextAsync(image, imageContext);
            return response.Text;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}

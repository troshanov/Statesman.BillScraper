using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Statesman.BillScraper.Utilities.ImageExtractor;

public static class PdfImageExtractor
{
    public static ExtractedImage[] ExtractImages(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            throw new ArgumentException("PDF bytes cannot be null or empty", nameof(pdfBytes));
        
        var result = new List<ExtractedImage>();
        
        try
        {
            using (var stream = new MemoryStream(pdfBytes))
            using (var document = PdfDocument.Open(stream))
            {
                Console.WriteLine($"Processing PDF with {document.NumberOfPages} pages");

                for (int i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    ProcessPageImages(page, result);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process PDF: {ex.Message}");
        }

        return result.ToArray();
    }

    private static void ProcessPageImages(Page page, List<ExtractedImage> result)
    {
        // Process regular images
        foreach (var image in page.GetImages())
        {
            try
            {
                var extractedImage =  ExtractImage(image);
                if (extractedImage != null)
                {
                    result.Add(extractedImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract image from page {page.Number}: {ex.Message}");
            }
        }
    }

    private static ExtractedImage? ExtractImage(IPdfImage image)
    {
        var imageData = image.RawBytes.ToArray();
        var format = DetermineImageFormat(imageData);

        if (format == null)
        {
            Console.WriteLine("Unsupported image format");
            return null;
        }

        return new ExtractedImage
        {
            Data = imageData,
            Format = format
        };
    }

    private static string? DetermineImageFormat(byte[] imageData)
    {
        // Simple magic number check for common formats
        if (imageData.Length >= 2)
        {
            if (imageData[0] == 0xFF && imageData[1] == 0xD8)
                return "jpg";
            if (imageData[0] == 0x89 && imageData[1] == 0x50)
                return "png";
            if (imageData[0] == 0x47 && imageData[1] == 0x49)
                return "gif";
        }

        return null; // Return raw if format cannot be determined
    }
}

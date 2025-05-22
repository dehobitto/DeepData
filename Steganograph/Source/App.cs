using System.Text;
using DeepData.Core;
using DeepData.Core.Extensions;
using DeepData.Core.Methods;
using System.IO;

namespace DeepData;

public class App
{
    public void Run()
    {
        string fileToHidePath = Path.Combine(Constants.ProjectRoot, "PoP.zip");
        string carrierImagePath = Path.Combine(Constants.ProjectRoot, "Docs", "1.jpg");
        string embeddedOutputPath = Path.Combine(Constants.ProjectRoot, "output.jpg");
        string extractedOutputFileName = "extracted_output";

        string originalFileExtension = Path.GetExtension(fileToHidePath).TrimStart('.');
        byte[] fileDataToHide = File.ReadAllBytes(fileToHidePath);

        byte[] extensionBytes = Encoding.UTF8.GetBytes(originalFileExtension);
        byte[] dataToEmbed = new byte[fileDataToHide.Length + extensionBytes.Length];
        Buffer.BlockCopy(fileDataToHide, 0, dataToEmbed, 0, fileDataToHide.Length);
        Buffer.BlockCopy(extensionBytes, 0, dataToEmbed, fileDataToHide.Length, extensionBytes.Length);

        var steganographyMethod = new DctJpeg { Options = new Options() };

        Func<long, double> toMegabytes = bytes => (double)bytes / 1024 / 1024;

        Console.WriteLine("--- Steganography Process ---");

        using (FileStream carrierFs = File.OpenRead(carrierImagePath))
        {
            long maxCapacityBytes = steganographyMethod.GetCapacityBytes(carrierFs);
            Console.WriteLine($"Maximum carrier capacity: {toMegabytes(maxCapacityBytes):F5} MB");
            Console.WriteLine($"Data to embed size: {toMegabytes(dataToEmbed.Length):F5} MB");

            if (dataToEmbed.Length > maxCapacityBytes)
            {
                Console.WriteLine("Error: Data to embed exceeds carrier capacity. Aborting.");
                return;
            }
        }

        using (FileStream carrierFs = File.OpenRead(carrierImagePath))
        using (Stream embeddedResultStream = steganographyMethod.Embed(carrierFs, dataToEmbed))
        {
            using (FileStream outputFs = File.Create(embeddedOutputPath))
            {
                embeddedResultStream.CopyTo(outputFs);
            }
        }

        Console.WriteLine($"Embedded image saved to: {embeddedOutputPath}");

        byte[] extractedData;
        using (FileStream embeddedFs = File.OpenRead(embeddedOutputPath))
        {
            extractedData = steganographyMethod.Extract(embeddedFs);
        }
        Console.WriteLine($"Extracted data size: {extractedData.Length} bytes");

        string extractedExtension = string.Empty;
        if (extractedData.Length >= originalFileExtension.Length)
        {
            byte[] potentialExtensionBytes = new byte[originalFileExtension.Length];
            Buffer.BlockCopy(extractedData, extractedData.Length - originalFileExtension.Length, potentialExtensionBytes, 0, originalFileExtension.Length);
            extractedExtension = Encoding.UTF8.GetString(potentialExtensionBytes);

            byte[] extractedFileContent = new byte[extractedData.Length - originalFileExtension.Length];
            Buffer.BlockCopy(extractedData, 0, extractedFileContent, 0, extractedFileContent.Length);
            extractedData = extractedFileContent;
        }
        else
        {
            Console.WriteLine("Warning: Extracted data is too short to contain the expected extension.");
        }
        
        string extractedFilePath = Path.Combine(Constants.DefaultImageOutputPath, $"{extractedOutputFileName}.{extractedExtension}");
        File.WriteAllBytes(extractedFilePath, extractedData);
        Console.WriteLine($"Extracted file saved to: {extractedFilePath}");

        Console.WriteLine("--- Data Integrity Check ---");
        Console.WriteLine($"Original data (file content) size: {fileDataToHide.Length} bytes");
        Console.WriteLine($"Extracted data (file content) size: {extractedData.Length} bytes");

        if (fileDataToHide.Length != extractedData.Length)
        {
            Console.WriteLine($"Mismatch: Original and extracted file content sizes differ.");
        }
        else
        {
            bool match = true;
            for (int i = 0; i < fileDataToHide.Length; i++)
            {
                if (fileDataToHide[i] != extractedData[i])
                {
                    Console.WriteLine($"Mismatch at byte {i}: Original={fileDataToHide[i]}, Extracted={extractedData[i]}");
                    match = false;
                    break;
                }
            }

            if (match)
            {
                Console.WriteLine("Original and extracted file contents are identical.");
            }
        }
        
        GC.Collect();
        
        Console.WriteLine("--- Process Complete ---");
        Console.ReadKey();
    }
}
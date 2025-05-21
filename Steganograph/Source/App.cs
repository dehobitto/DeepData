using System.Text;
using DeepData.Core;
using DeepData.Core.Methods;

namespace DeepData;

public class App
{
    public void Run()
    {
        byte[] data = File.ReadAllBytes(Path.Combine(Constants.ProjectRoot, "3.pdf"));
        //stg.Embed(data);

        var steg = new DctJpeg
        {
            Options = new Options()
        };

        // ... (Constants, data, steg setup)

        using FileStream fs = File.OpenRead(Path.Combine(Constants.ProjectRoot, "Docs", "image.jpg"));
        var embeddedStream = steg.Embed(fs, data); // This stream contains the embedded data
        fs.Close(); // Close the stream for the original input image

// 1. Save the embeddedStream to a new file
        string outputPath = Path.Combine(Constants.DefaultImageOutputPath, "output.jpg");
        using (FileStream fileOut = File.Create(outputPath))
        {
            embeddedStream.CopyTo(fileOut);
        }
        embeddedStream.Close(); // Make sure to close the in-memory stream after copying

        Console.WriteLine($"Embedded image saved to: {outputPath}");

// 2. Now, open the saved file to extract the data
        using FileStream outFsForExtraction = File.OpenRead(outputPath);
        var extractedData = steg.Extract(outFsForExtraction);
        outFsForExtraction.Close(); // Close the stream used for extraction

        Console.WriteLine("Size1 " + data.Length);
        Console.WriteLine("Size2 " + extractedData.Length);

// ... rest of your code for verification

        File.WriteAllBytes(Path.Combine(Constants.DefaultImageOutputPath, "output.pdf"), extractedData);
// ... (rest of your verification code)

        var path = Path.Combine(Constants.DefaultImageOutputPath, "output.pdf");
        Console.WriteLine("Writing to " + path);
        Console.WriteLine("Data size: " + extractedData.Length); // должно быть 80_000+ байт

        Console.WriteLine(Encoding.UTF8.GetString(data));

// Проверка, что всё ок
        var info = new FileInfo(path);
        Console.WriteLine("Saved file size: " + info.Length);
        
        if (data.Length != extractedData.Length)
        {
            Console.WriteLine($"Size mismatch: {data.Length} vs {extractedData.Length}");
        }
        else
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != extractedData[i])
                {
                    Console.WriteLine($"Byte mismatch at {i}: original={data[i]}, extracted={extractedData[i]}");
                }
            }
            Console.WriteLine("Files are identical.");
        }


        Console.ReadKey();

    }
}
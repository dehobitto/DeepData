using System.Text;
using DeepData.Core;
using DeepData.Core.Methods;

namespace DeepData;

public class App
{
    public void Run()
    {
        string[] consts = [
            Constants.ProjectRoot,
        ];
        
        foreach (var con in consts)
        {
            Console.WriteLine(con);
        }
        
        byte[] data = File.ReadAllBytes(Path.Combine(Constants.ProjectRoot, "3.pdf"));
        //stg.Embed(data);

        var steg = new DctJpeg
        {
            Options = new Options()
        };

        using FileStream fs = File.OpenRead(Path.Combine(Constants.ProjectRoot, "Docs", "image.jpg"));

        var fss = steg.Embed(fs, data);
        fs.Close();

        using var fileOut = File.Create(Path.Combine(Constants.DefaultImageOutputPath, "output.jpg"));
        fss.CopyTo(fileOut);
        fss.Close();
        fileOut.Close();

        using FileStream outFs = File.OpenRead(Path.Combine(Constants.DefaultImageOutputPath, "output.jpg"));

        var outFss = steg.Extract(outFs);
        outFs.Close();
        
        Console.WriteLine("Size1 " + data.Length);
        Console.WriteLine("Size2 " + outFss.Length);

        File.WriteAllBytes(Path.Combine(Constants.DefaultImageOutputPath, "output.pdf"), outFss);

        var path = Path.Combine(Constants.DefaultImageOutputPath, "output.pdf");
        Console.WriteLine("Writing to " + path);
        Console.WriteLine("Data size: " + outFss.Length); // должно быть 80_000+ байт

        File.WriteAllBytes(path, outFss);

// Проверка, что всё ок
        var info = new FileInfo(path);
        Console.WriteLine("Saved file size: " + info.Length);
        
        if (data.Length != outFss.Length)
        {
            Console.WriteLine($"Size mismatch: {data.Length} vs {outFss.Length}");
        }
        else
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != outFss[i])
                {
                    Console.WriteLine($"Byte mismatch at {i}: original={data[i]}, extracted={outFss[i]}");
                }
            }
            Console.WriteLine("Files are identical.");
        }


        Console.ReadKey();

    }
}
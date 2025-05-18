using System.Text;
using DeepData.Stego;
using DeepData.Stego.Interfaces;
using DeepData.Stego.Methods;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData;

internal static class Runner
{
   public static void Main(string[] args)
   {
      IStegoMethod<Image<Rgba32>, byte[]> stg = new Qim();
      Image<Rgba32> source = Image.Load<Rgba32>(args[0]);
      Options opt = new Options();
      
      if (args.Length > 1)
      {
         byte[] data = Encoding.ASCII.GetBytes(args[1]);
         Image<Rgba32> output = stg.Embed(source, data, opt);
         output.Save("./Source/Data/Output/result.png", new PngEncoder());
      }else if (args.Length == 1)
      {
         byte[] data = stg.Extract(source, opt);
         string result = Encoding.ASCII.GetString(data);
         Console.WriteLine("Exctracted data: " + result);
      }
   }
}
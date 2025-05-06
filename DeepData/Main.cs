using System.Text;
using Converter;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Steganograph;

internal class Runner
{
   public static void Main(string[] args)
   {
      IStego stg = new LsbStego();
      (int h, int w) size;
      byte[] source = ImgToByte.ToBytes(args[0], out size);
      Options opt = new Options();
      
      if (args.Length > 1)
      {
         byte[] data = Encoding.ASCII.GetBytes(args[1]);
         
         byte[] output = stg.Embed(source, data, opt);
         Image<Rgba32> img = ImgToByte.ToImage(output, size);
         
         img.Save("result.png", new PngEncoder());
      }else if (args.Length == 1)
      {
         byte[] data = stg.Extract(source, opt);
         string result = Encoding.ASCII.GetString(data);
         Console.WriteLine("Exctracted data: " + result);
      }
   }
}


using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DeepData;

internal static class Program
{
   private static readonly App App = new ();
   public static void Main(string[] args)
   {
      /*Image<Rgba32> source = Image.Load<Rgba32>(args[0]);
      IStegoMethod<Image<Rgba32>, byte[]> stg = new Qim(new Options());
      
      if (args.Length > 1)
      {
         byte[] data = Encoding.ASCII.GetBytes(args[1]);
         Image<Rgba32> output = stg.Embed(source, data);
         output.Save("./Source/Data/Output/result.png", new PngEncoder());
      }else if (args.Length == 1)
      {
         byte[] data = stg.Extract(source);
         string result = Encoding.ASCII.GetString(data);
         Console.WriteLine("Exctracted data: " + result);
      }*/

      App.Run();
   }
}
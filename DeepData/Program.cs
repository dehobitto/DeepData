using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace DeepData
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            /*using (var bmp = new Bitmap(Selector.Select()))
            {
                
            }*/
            try
            {
                var bmp = new Bitmap(Selector.Select());
                Steganograph st = new Steganograph(bmp);
            
                Console.Write("Write data to file: ");
                string data = Console.ReadLine();
            
                st.Hide(data, "image");
            }
            catch (Exception e)
            {

            }
            
            Console.Write("Choose decode file: ");
            Console.ReadLine();
            
            var tbmp = new Bitmap(Selector.Select());
            var tst = new Steganograph(tbmp);
            Console.WriteLine("Decoded data: " + tst.Decode());
            Console.ReadLine();
        }
    }
}

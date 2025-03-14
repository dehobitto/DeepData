using System;
using System.IO;
using System.Windows.Forms;

namespace DeepData
{
    public class Selector
    {
        private static string filePath;
        [STAThread]
        public static Stream Select()
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                file.InitialDirectory = "c:\\";
                file.Filter = "Bmp images (*.bmp)|*.bmp|All files (*.*)|*.*";
                file.FilterIndex = 2;
                file.RestoreDirectory = true;
                
                if (file.ShowDialog() == DialogResult.OK)
                {
                    filePath = file.FileName;
                    return new MemoryStream(File.ReadAllBytes(filePath));
                }
                
                return null;
            }
        }
    }
}
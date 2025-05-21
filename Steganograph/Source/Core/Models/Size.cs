namespace DeepData.Stego.Models;

public class Size((int width, int height) size)
{
    public int w => size.width;
    public int h => size.height;
}
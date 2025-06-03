namespace DeepData.CLI;

public static class Constants
{
    public static readonly string[] SupportedImageFormats = [".jpg", ".jpeg", ".png", ".bmp" ];
    public static readonly string[] LossyFormats = [ ".jpg", ".jpeg" ];
    
    public const string Version = "1.1.2";
    public const string HelpText = """
                            usage: deepdata [--version | -v] [--help | -h]
                                            <command> [--method <method>] [<args>] [<options>]

                            Commands:
                              embed <input_image_path> <data_path> [--method <method>] [--output <output_image_path>] [--settings <key=value>...]
                                  Embeds data from <data_path> into <input_image_path>.
                                  <data_path> is required.
                                  If --method is not specified, Qim will be used for JPEG images and Lsb for others.

                              extract <input_image_path> --output <output_data_path> [--method <method>] [--settings <key=value>...]
                                  Extracts embedded data from <input_image_path>.
                                  --output is required.
                                  If --method is not specified, Qim will be used for JPEG images and Lsb for others.

                              capacity <input_image_path> [--method <method>] [--settings <key=value>...]
                                  Shows the estimated embedding capacity for the image.
                                  If --method is not specified, Qim will be used for JPEG images and Lsb for others.

                            Methods:
                              lsb       Least Significant Bit — for lossless formats (PNG, BMP).
                              qim       Quantization Index Modulation — supports multiple formats, including JPEG.
                                        **Note:** JPEG images can only be used with the Qim method.

                            Supported formats:
                              Lossless formats (PNG, BMP):
                                - Can use both Lsb and Qim methods
                                - Best for data that needs to be preserved exactly
                                - Larger file sizes
                              Lossy formats (JPEG):
                                - Can only use Qim method
                                - Smaller file sizes
                                - May lose some data quality

                            Options:
                              -h, --help                 Show help information.
                              -v, --version              Show version information.

                              --method <method>          Steganography method to use.
                                                         If not specified, method is auto-detected based on image format.

                              --output <path>            Output path for image or data.
                                                         - For 'embed': path to the output image (e.g., output.jpg).
                                                           If omitted, defaults to 'output_<input_image_name>.jpg'.
                                                         - For 'extract': path to save the extracted data (without extension).
                                                           The extension is recovered from the image itself.

                              --settings <key=value>... Method-specific settings.
                                                        Multiple settings can be provided.
                                                        Method must be specified when using settings.

                            Available settings per method:

                              qim:
                                channels=<R,G,B,Y,Cb,Cr>   Color channels to use.
                                                           For JPEG: use Y, Cb, Cr. For others: R, G, B.
                                                           Default: all supported channels.
                                delta=<int>                Quantization step (embedding strength). Default: 4.

                              lsb:
                                channels=<R,G,B>           Color channels to use. Default: R,G,B.
                                strength=<1–7>             Number of LSBs to modify per channel. Default: 1.

                            Examples:
                              # Using auto-detected method
                              deepdata embed photo.jpg secret.txt --output stego.jpg
                              deepdata extract stego.jpg --output recovered
                              deepdata capacity image.jpg

                              # Using specific method with settings
                              deepdata embed photo.jpg secret.txt --method qim --settings channels=Y,Cb,delta=4
                              deepdata extract stego.jpg --method lsb --settings channels=R,G,strength=2
                              deepdata capacity diagram.bmp --method lsb --settings channels=R,G,strength=2
                            """;
}
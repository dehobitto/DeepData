using BitMiracle.LibJpeg.Classic;
using DeepData.Extensions;

namespace DeepData.Utils.StegoSpecific;

public static class JpegHelper
{
    /// <summary>
    ///     Decompresses a JPEG image stream to get its Discrete Cosine Transform (DCT) coefficients.
    /// </summary>
    /// <param name="source">The JPEG input stream.</param>
    /// <returns>A <see cref="JpegDecompressResult" /> with all the decompression details and coefficients.</returns>
    public static JpegDecompressResult Decompress(Stream source)
    {
        var decompress = new jpeg_decompress_struct();
        decompress.jpeg_stdio_src(source);
        decompress.jpeg_read_header(true);
        var coefArrays = decompress.jpeg_read_coefficients();
        
        return new JpegDecompressResult(decompress, coefArrays);
    }

    /// <summary>
    ///     Gets the 2D array of DCT blocks for a specific component (like Y, Cb, or Cr).
    /// </summary>
    /// <param name="coefArray">The virtual array of coefficients for that component.</param>
    /// <param name="compInfo">Information about the component.</param>
    /// <returns>A 2D array of <see cref="JBLOCK" /> representing the DCT blocks.</returns>
    public static JBLOCK[][] GetBlocks(jvirt_array<JBLOCK> coefArray, jpeg_component_info compInfo)
    {
        return coefArray.Access(0, compInfo.Height_in_blocks());
    }

    /// <summary>
    ///     Compresses DCT coefficients back into a JPEG image stream.
    /// </summary>
    /// <param name="decompressResult">The result from a previous decompression, used to copy essential parameters.</param>
    /// <param name="coefArrays">The (possibly modified) DCT coefficient arrays.</param>
    /// <returns>A new <see cref="Stream" /> containing the compressed JPEG data.</returns>
    public static Stream Compress(JpegDecompressResult decompressResult, jvirt_array<JBLOCK>[] coefArrays)
    {
        var output = new MemoryStream();
        var compress = new jpeg_compress_struct();
        compress.jpeg_stdio_dest(output);

        decompressResult.DecompressStruct.jpeg_copy_critical_parameters(compress);
        compress.jpeg_write_coefficients(coefArrays);

        decompressResult.DecompressStruct.jpeg_finish_decompress();
        compress.jpeg_finish_compress();

        output.Seek(0, SeekOrigin.Begin);
        return output;
    }
}

/// <summary>
///     Stores the outcome of a JPEG decompression: the core decompression structure
///     and all the Discrete Cosine Transform (DCT) coefficient arrays.
/// </summary>
/// <param name="decompressStruct">The internal `jpeg_decompress_struct` from LibJpeg.Classic.</param>
/// <param name="coefArrays">The virtual arrays of DCT blocks for each color component.</param>
public sealed class JpegDecompressResult(jpeg_decompress_struct decompressStruct, jvirt_array<JBLOCK>[] coefArrays)
{
    /// <summary>
    ///     Gets the raw decompression structure.
    /// </summary>
    public jpeg_decompress_struct DecompressStruct { get; } = decompressStruct;

    /// <summary>
    ///     Gets the arrays of DCT coefficients for each component (like Y, Cb, Cr).
    /// </summary>
    public jvirt_array<JBLOCK>[] CoefArrays { get; } = coefArrays;

    /// <summary>
    ///     Gets the number of color components found in the JPEG image.
    /// </summary>
    public int NumComponents => DecompressStruct.Num_components;

    /// <summary>
    ///     Gets information for a specific JPEG color component by its index.
    /// </summary>
    /// <param name="index">The zero-based index of the component (e.g., 0 for Y, 1 for Cb).</param>
    /// <returns>The 'jpeg_component_info' for the chosen component.</returns>
    public jpeg_component_info GetComponentInfo(int index)
    {
        return DecompressStruct.Comp_info[index];
    }
}
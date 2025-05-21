using BitMiracle.LibJpeg.Classic;
using DeepData.Core.Extensions;

namespace DeepData.Core.Utils;

public static class JpegHelper
{
    public static JpegDecompressResult Decompress(Stream source)
    {
        var decompress = new jpeg_decompress_struct();
        decompress.jpeg_stdio_src(source);
        decompress.jpeg_read_header(true);
        var coefArrays = decompress.jpeg_read_coefficients();
        return new JpegDecompressResult(decompress, coefArrays);
    }

    public static JBLOCK[][] GetBlocks(jvirt_array<JBLOCK> coefArray, jpeg_component_info compInfo)
    {
        return coefArray.Access(0, compInfo.Height_in_blocks());
    }

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

public sealed class JpegDecompressResult(jpeg_decompress_struct decompressStruct, jvirt_array<JBLOCK>[] coefArrays)
{
    public jpeg_decompress_struct DecompressStruct { get; } = decompressStruct;
    public jvirt_array<JBLOCK>[] CoefArrays { get; } = coefArrays;
    public int NumComponents => DecompressStruct.Num_components;

    public jpeg_component_info GetComponentInfo(int index) => DecompressStruct.Comp_info[index];
}
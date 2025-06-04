namespace DeepData.Settings;

[Flags]
public enum ImageChannels
{
    None = 0,
    R = 1 << 0,
    G = 1 << 1,
    B = 1 << 2,
    All = R | G | B
}

[Flags]
public enum JpegChannels
{
    None = 0,
    Y = 1 << 0,
    Cb = 1 << 1,
    Cr = 1 << 2,
    All = Y | Cb | Cr
}

public enum Stego
{
    Qim,
    Lsb,
    Dct
}
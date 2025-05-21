namespace DeepData.Core.Interfaces;

public interface IStegoMethod<TSource, TPayload>
{
    /// <summary>
    /// Small generic interface so it's easier to handle methods
    /// </summary>
    /// <param name="source"></param>
    ///  Source image where data will be placed
    /// <param name="data"></param>
    ///  Data that we are hiding
    /// <param name="options"></param>
    ///  Some options that can be changed by user, in Steganograph/Options.cs
    
    TSource Embed(TSource source, byte[] data);
    TPayload Extract(TSource source);
    Options Options { get; set; }
}
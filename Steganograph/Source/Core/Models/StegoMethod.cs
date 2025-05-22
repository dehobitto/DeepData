using DeepData.Core.Interfaces;

namespace DeepData.Core.Models;

public abstract class StegoMethod<TSource, TPayload> : IStegoMethod<TSource, TPayload>
{
    public abstract TSource Embed(TSource source, byte[] data);
    public abstract TPayload Extract(TSource source);
    public required Options Options { get; set; }
    public abstract bool WillFit(TSource source, TPayload payload);
    public abstract int GetCapacityBytes(TSource source);
}
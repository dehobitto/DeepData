using DeepData.Interfaces;
using DeepData.Settings;

namespace DeepData.Models;

public abstract class StegoMethod<TSource, TPayload> : IStegoMethod<TSource, TPayload>
{
    protected readonly Options Options;

    protected StegoMethod(Options options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public abstract TSource Embed(TSource source, byte[] data);
    public abstract TPayload Extract(TSource source);
    public abstract bool WillFit(TSource source, TPayload payload);
    public abstract int GetCapacityBytes(TSource source);
}
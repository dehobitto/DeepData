using DeepData.Interfaces;
using DeepData.Settings;

namespace DeepData.Abstractions;

public abstract class StegoMethod<TSource, TPayload>(Options options) : IStegoMethod<TSource, TPayload>
{
    protected readonly Options Options = options ?? throw new ArgumentNullException(nameof(options));

    public abstract TSource Embed(TSource source, byte[] data);
    public abstract TPayload Extract(TSource source);
    public abstract bool WillFit(TSource source, TPayload payload);
    public abstract int GetCapacityBytes(TSource source);

    protected IProgress? Progress;

    public void SetProgress(IProgress progress)
    {
        Progress = progress;
    }
}
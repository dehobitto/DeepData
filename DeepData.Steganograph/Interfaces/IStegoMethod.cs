namespace DeepData.Interfaces;

public interface IStegoMethod<TSource, TPayload>
{
    TSource Embed(TSource source, byte[] data);
    TPayload Extract(TSource source);
    bool WillFit(TSource source, TPayload payload);
    int GetCapacityBytes(TSource source);
}
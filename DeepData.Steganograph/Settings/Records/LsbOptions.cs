namespace DeepData.Settings.Records;

public record LsbOptions
{
    public LsbOptions(){}

    public LsbOptions(byte strength)
    {
        if (strength < 1 || strength > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "LSB strength must be between 1 and 8.");
        }

        Strength = strength;
    }

    public byte Strength { get; init; } = StegoConstants.DefaultLsbStrength;
}
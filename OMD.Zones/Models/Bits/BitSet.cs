namespace OMD.Zones.Models.Bits;

public sealed class BitSet(int size)
{
    private static readonly int SizeOfUlong = sizeof(ulong);
    private static readonly int CeilingShift = SizeOfUlong - 1;

    private readonly ulong[] _bits = new ulong[(size + CeilingShift) / SizeOfUlong];

    public bool Get(int index)
    {
        var arrayIndex = index / SizeOfUlong;
        var bitIndex = index % SizeOfUlong;

        return (_bits[arrayIndex] & (1UL << bitIndex)) != 0;
    }

    public void Set(int index)
    {
        var arrayIndex = index / SizeOfUlong;
        var bitIndex = index % SizeOfUlong;

        _bits[arrayIndex] |= (1UL << bitIndex);
    }

    public void Clear(int index)
    {
        var arrayIndex = index / SizeOfUlong;
        var bitIndex = index % SizeOfUlong;

        _bits[arrayIndex] &= ~(1UL << bitIndex);
    }
}

namespace OMD.Zones.Models.Bits;

static class BitUtils
{
    public static int TrailingZeroCount(ulong x)
    {
        if (x == 0)
            return 32;

        var n = 0;

        while ((x & 1u) == 0)
        {
            n += 1;
            x >>= 1;
        }

        return n;
    }
}

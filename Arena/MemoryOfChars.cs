using System.Runtime.CompilerServices;

namespace apiTest.Arena;

public static class MemoryOfChars
{
    private static readonly char[] _singleDigitCharCache = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append(this List<Char> source, uint number)
    {
        var numberLength = CountDigits(number);

        source.Count += numberLength;

        if (numberLength == 1)
            source.AsSpan()[^1] = _singleDigitCharCache[number];
        else
            UInt32ToDecChars(source, number);        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UInt32ToDecChars(this List<Char> source, uint value)
    {
        var count = source.Count;
        var sp = source.AsSpan();
        do
        {
            var quotient = value / 10;
            (value, var remainder) = (quotient, value % 10);

            sp[--count] = (char)(remainder + '0');
        }
        while (value != 0);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountDigits(uint value)
    {
        var digits = 1;
        if (value >= 100_000)
        {
            value /= 100_000;
            digits += 5;
        }

        return value switch
        {
            < 10 => digits,
            < 100 => digits + 1,
            < 1000 => digits + 2,
            < 10_000 => digits + 3,
            _ => digits + 4
        };
    }

}

using apiTest.Arena;
using LinkDotNet.StringBuilder;
using mimalloc;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using ImeSense.Packages.Mimalloc.Runtime;
//using mimalloc;

namespace apiTest;

public class Strings
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static int CompareUnsafe(char* p1, char* p2)
    {
        while (*p1 != 0)
        {
            if (*p2 == 0) return 1;

            if (*p1 >= '0' && *p1 <= '9' && *p2 >= '0' && *p2 <= '9')
            {
                var (num1, num2) = (*p1 - '0', *p2 - '0');
                p1++; p2++;

                // Читаем остальные цифры первого числа
                while (*p1 >= '0' && *p1 <= '9')
                {
                    num1 = 10 * num1 + *p1 - '0';
                    p1++;
                }

                // Читаем остальные цифры второго числа
                while (*p2 >= '0' && *p2 <= '9')
                {
                    num2 = 10 * num2 + *p2 - '0';
                    p2++;
                }

                if (num1 != num2) return num1 > num2 ? 1 : -1;
            }
            else
            {
                // Сравниваем как символы
                if (*p1 != *p2) return (*p1 > *p2) ? 1 : -1;

                p1++; p2++;
            }
        }

        return *p2 == 0 ? 0 : -1;
    }

    public unsafe static int CompareUnsafe(string s1, string s2)
    {
        var (ne1, ne2) = (string.IsNullOrEmpty(s1), string.IsNullOrEmpty(s2));

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        fixed (char* pointer1 = s1, pointer2 = s2)
            return CompareUnsafe(pointer1, pointer2);
    }

    public unsafe static int CompareUnsafe(ref BufferString s1, ref BufferString s2)
    {
        var (ne1, ne2) = (s1.Count == 0, s2.Count == 0);

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;
        fixed (char* pointer1 = s1.AsSpan(), pointer2 = s2.AsSpan())
            return CompareUnsafe(pointer1, pointer2);
    }

    public unsafe static int CompareUnsafe(MimAllocString s1, MimAllocString s2)
    {
        var (ne1, ne2) = (s1.Length == 0, s2.Length == 0);

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        return CompareUnsafe(s1.AsCharPointer(), s2.AsCharPointer());
    }

    public static int CompareSafe(string s1, string s2)
    {
        var (ne1, ne2) = (string.IsNullOrEmpty(s1), string.IsNullOrEmpty(s2));

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        var (count1, count2) = (s1.Length, s2.Length);
        var (i1, i2) = (0, 0);

        while (i1 < count1)
        {
            if (i2 >= count2) return 1;

            var (char1, char2) = (s1[i1++], s2[i2++]);

            if (char1 >= '0' && char1 <= '9' && char2 >= '0' && char2 <= '9')
            {
                var (num1, num2) = (char1 - '0', char2 - '0');

                while (i1 < count1)
                {
                    char1 = s1[i1];
                    if (char1 >= '0' && char1 <= '9')
                    {
                        num1 = 10 * num1 + char1 - '0';
                        i1++;
                    }
                    else break;
                }

                // Читаем остальные цифры второго числа
                while (i2 < count2)
                {
                    char2 = s2[i2];
                    if (char2 >= '0' && char2 <= '9')
                    {
                        num2 = 10 * num2 + char2 - '0';
                        i2++;
                    }
                    else break;
                }

                if (num1 != num2) return num1 > num2 ? 1 : -1;
            }
            // Сравниваем как символы
            else if (char1 != char2)
                return (char1 > char2) ? 1 : -1;
        }

        return i2 == count2 ? 0 : -1;

    }

    public static int CompareIterator(string s1, string s2)
    {
        var (ne1, ne2) = (string.IsNullOrEmpty(s1), string.IsNullOrEmpty(s2));

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        var e1 = s1.GetEnumerator();
        var e2 = s2.GetEnumerator();

        var (b1, b2) = (e1.MoveNext(), e2.MoveNext());


        while (b1)
        {
            if (!b2) return 1;

            if (e1.Current >= '0' && e1.Current <= '9' && e2.Current >= '0' && e2.Current <= '9')
            {
                var (num1, num2) = (e1.Current - '0', e2.Current - '0');
                (b1, b2) = (e1.MoveNext(), e2.MoveNext());


                while (b1 && e1.Current >= '0' && e1.Current <= '9')
                {
                    num1 = 10 * num1 + e1.Current - '0';
                    b1 = e1.MoveNext();
                }

                // Читаем остальные цифры второго числа
                while (b2 && e2.Current >= '0' && e2.Current <= '9')
                {
                    num2 = 10 * num2 + e2.Current - '0';
                    b2 = e2.MoveNext();
                }
                if (num1 != num2) return num1 > num2 ? 1 : -1;
            }
            else
            {
                // Сравниваем как символы
                if (e1.Current != e2.Current) return (e1.Current > e2.Current) ? 1 : -1;

                (b1, b2) = (e1.MoveNext(), e2.MoveNext());
            }

        }

        return !b2 ? 0 : -1;

    }

    public static int CompareSBIterator(ValueStringBuilder s1, ValueStringBuilder s2)
    {
        var (ne1, ne2) = (s1.IsEmpty, s2.IsEmpty);

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        var e1 = s1.GetEnumerator();
        var e2 = s2.GetEnumerator();

        var (b1, b2) = (e1.MoveNext(), e2.MoveNext());


        while (b1)
        {
            if (!b2) return 1;

            if (e1.Current >= '0' && e1.Current <= '9' && e2.Current >= '0' && e2.Current <= '9')
            {
                var (num1, num2) = (e1.Current - '0', e2.Current - '0');
                (b1, b2) = (e1.MoveNext(), e2.MoveNext());


                while (b1 && e1.Current >= '0' && e1.Current <= '9')
                {
                    num1 = 10 * num1 + e1.Current - '0';
                    b1 = e1.MoveNext();
                }

                // Читаем остальные цифры второго числа
                while (b2 && e2.Current >= '0' && e2.Current <= '9')
                {
                    num2 = 10 * num2 + e2.Current - '0';
                    b2 = e2.MoveNext();
                }
                if (num1 != num2) return num1 > num2 ? 1 : -1;
            }
            else
            {
                // Сравниваем как символы
                if (e1.Current != e2.Current) return (e1.Current > e2.Current) ? 1 : -1;

                (b1, b2) = (e1.MoveNext(), e2.MoveNext());
            }

        }

        return !b2 ? 0 : -1;

    }


    public static unsafe int CompareSBUnSafe(ValueStringBuilder s1, ValueStringBuilder s2)
    {
        var (ne1, ne2) = (s1.IsEmpty, s2.IsEmpty);

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        fixed (char* pointer1 = s1.AsSpan(), pointer2 = s2.AsSpan())
            return CompareUnsafe(pointer1, pointer2);
    }

    public static int CompareSBSafe(ValueStringBuilder s11, ValueStringBuilder s21)
    {
        var (ne1, ne2) = (s11.IsEmpty, s21.IsEmpty);

        if (ne1 && ne2) return 0;
        if (ne1) return -1;
        if (ne2) return 1;

        var s1 = s11.AsSpan();
        var s2 = s21.AsSpan();        

        var (count1, count2) = (s1.Length, s2.Length);
        var (i1, i2) = (0, 0);

        while (i1 < count1)
        {
            if (i2 >= count2) return 1;

            var (char1, char2) = (s1[i1++], s2[i2++]);

            if (char1 >= '0' && char1 <= '9' && char2 >= '0' && char2 <= '9')
            {
                var (num1, num2) = (char1 - '0', char2 - '0');

                while (i1 < count1)
                {
                    char1 = s1[i1];
                    if (char1 >= '0' && char1 <= '9')
                    {
                        num1 = 10 * num1 + char1 - '0';
                        i1++;
                    }
                    else break;
                }


                // Читаем остальные цифры второго числа
                while (i2 < count2)
                {
                    char2 = s2[i2];
                    if (char2 >= '0' && char2 <= '9')
                    {
                        num2 = 10 * num2 + char2 - '0';
                        i2++;
                    }
                    else break;
                }

                if (num1 != num2) return num1 > num2 ? 1 : -1;
            }
            // Сравниваем как символы
            else if (char1 != char2)
                return (char1 > char2) ? 1 : -1;
        }

        return i2 == count2 ? 0 : -1;
    }

}


public struct MimAllocString
{
    private static readonly char[] _singleDigitCharCache = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

    public int Length { get; private set; } = 0;
    private IntPtr Buffer;
    private uint Copacity = 0;

    public unsafe MimAllocString(uint copacity)
    {
        Copacity = copacity;
        Buffer = (IntPtr)MiMalloc.mi_malloc(copacity * 2 + 2);
    }

    public unsafe MimAllocString(string s, uint copacity) : this(copacity < s.Length ? (uint)s.Length : copacity)
    {
        s.AsSpan().CopyTo(new Span<char>((char*)Buffer, Length));
        Length = s.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void Dispose()
    {
        MiMalloc.mi_free((void*)Buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ReAlloc(uint copacity)
    {
        if (copacity <= Copacity) return;

        var newBuffer = (char*)MiMalloc.mi_malloc(copacity * 2 + 2);

        new Span<char>((char*)Buffer, Length).CopyTo(new Span<char>(newBuffer, Length));

        MiMalloc.mi_free((void*)Buffer);
        Buffer = (IntPtr)newBuffer;
        Copacity = copacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string s) => Add(s.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Add(ReadOnlySpan<char> chars)
    {
        //var len = Length + chars.Length;
        //ReAlloc((uint)len);

        chars.CopyTo(new Span<char>((char*)Buffer + Length, chars.Length));

        //Length = len;
        Length += chars.Length;
    }

    public unsafe void Add(uint number)
    {
        var numberLength = CountDigits(number);
        //var len = Length + numberLength;
        //ReAlloc((uint)len);

        var pointer = (char*)Buffer + Length;

        Length += numberLength;

        if (numberLength == 1)
            *pointer = _singleDigitCharCache[number];
        else
            UInt32ToDecChars(pointer + numberLength, number);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void UInt32ToDecChars(char* bufferEnd, uint value)
    {
        do
        {
            var quotient = value / 10;
            (value, var remainder) = (quotient, value % 10);

            *--bufferEnd = (char)(remainder + '0');
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe char* AsCharPointer() => (char*)Buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe Span<char> AsSpan() => new((char*)Buffer, Length);
}
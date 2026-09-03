using apiTest.Arena;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        fixed (char* pointer1 = s1, pointer2 = s2)
            return CompareUnsafe(pointer1, pointer2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static int CompareUnsafe(in BufferString s1, in BufferString s2)
    {
        fixed (char* pointer1 = &s1.Items[s1.Start], pointer2 = &s2.Items[s2.Start])
            return CompareUnsafe(pointer1, pointer2);
    }

    public static int CompareSafe(string s1, string s2)
    {
        var sp1 = s1.AsSpan();
        var sp2 = s2.AsSpan();

        var (count1, count2) = (sp1.Length, sp2.Length);
        var (i1, i2) = (0, 0);

        
        while (i1 < count1)
        {
            if (i2 >= count2) return 1;

            var (char1, char2) = (sp1[i1++], sp2[i2++]);

            if (char1 >= '0' && char1 <= '9' && char2 >= '0' && char2 <= '9')
            {
                var (num1, num2) = (char1 - '0', char2 - '0');

                while (i1 < count1)
                {
                    char1 = sp1[i1];
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
                    char2 = sp2[i2];
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
}

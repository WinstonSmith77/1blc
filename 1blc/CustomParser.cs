using System.Numerics;

namespace _1blc;

public class CustomParser<T> where T : IFloatingPoint<T>
{
    private static T ten = T.CreateChecked(10);
    private static T[] digits;

    static CustomParser()
    {
        digits = Enumerable.Range(0, 10).Select(d => T.CreateChecked<int>(d)).ToArray();
    }

    public (int, string, T) SplitTextAndParseDoubleStateMachine(ReadOnlySpan<char> all, Range range)
    {
        T result = T.Zero;
        T fraction = T.Zero;
        T divisor = T.One;
        T exponent = T.One;
        var isNegative = false;
        var inFraction = true;

        var text = string.Empty;
        var hash = 0;

        var span = all[range];

        for (int index = span.Length - 1; index > 0; index--)
        {
            var c = span[index];
            switch (c)
            {
                case '\r':
                    break;
                case ';':
                    text = span.Slice(0, index).ToString();
                    hash = text.GetHashCode();
                    index = 0;
                    break;
                case '-':
                    isNegative = true;
                    break;
                case '.':
                    inFraction = false;
                    break;
                case >= '0' and <= '9':
                    var digit = c - '0';
                    if (inFraction)
                    {
                        fraction = fraction / ten + digits[digit];
                        divisor *= ten;
                    }
                    else
                    {
                        result += digits[digit] * exponent;
                        exponent *= ten;
                    }
                    break;
                default:
                    throw new Exception();
            }
        }

        result += fraction / ten;
        return (hash, text, isNegative ? -result : result);
    }
}
using System.Globalization;

namespace ChineseGold
{
    internal static class ChineseGoldFormatter
    {
        public static string Format(int value)
        {
            long number = value;

            if (number < 0)
            {
                return "-" + FormatPositive(-number);
            }

            return FormatPositive(number);
        }

        private static string FormatPositive(long value)
        {
            if (value < 10_000)
            {
                return value.ToString("N0", CultureInfo.InvariantCulture);
            }

            if (value < 100_000_000)
            {
                return FormatUnit(value, 10_000m, "万");
            }

            return FormatUnit(value, 100_000_000m, "亿");
        }

        private static string FormatUnit(long value, decimal unit, string suffix)
        {
            decimal amount = value / unit;
            return amount.ToString("0.##", CultureInfo.InvariantCulture) + suffix;
        }
    }
}

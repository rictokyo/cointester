using System.Collections.Generic;

namespace WpfLiteCoinTester
{
    public enum BalanceTypes
    {
        LTC = 1, JPY = 2, INLTC = 3
    }

    public static class BalanceUtils
    {
        private static Dictionary<int, string> balanceTypeDictionary = new Dictionary<int, string>
        {
            { 1, "LTC" },
            { 2, "JPY" },
            { 3, "IN-LTC" }
        };

        public static string GetBalanceType(int? type1)
        {
            if (type1 == null) return string.Empty;
            var type = type1.Value;

            string balanceType = string.Empty;
            balanceTypeDictionary.TryGetValue(type, out balanceType);

            return balanceType;
        }
    }
}

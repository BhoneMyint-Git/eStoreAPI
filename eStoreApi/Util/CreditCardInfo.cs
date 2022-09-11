using System.Text.RegularExpressions;

namespace eStoreApi.Util
{
    public class CreditCardInfo
    {
        public enum CardType
        {
            Unknown = 0,
            MasterCard = 1,
            VISA = 2
        }

        // Class to hold credit card type information
        private class CardTypeInfo
        {
            public CardTypeInfo(string regEx, int length, CardType type)
            {
                RegEx = regEx;
                Length = length;
                Type = type;
            }

            public string RegEx { get; set; }
            public int Length { get; set; }
            public CardType Type { get; set; }
        }

        // Array of CardTypeInfo objects.
        // Used by GetCardType() to identify credit card types.
        private static CardTypeInfo[] _cardTypeInfo =
        {
            new CardTypeInfo("^(51|52|53|54|55)", 16, CardType.MasterCard),
            new CardTypeInfo("^(4)", 16, CardType.VISA),
            new CardTypeInfo("^(4)", 13, CardType.VISA),
        };

        public static CardType GetCardType(string cardNumber)
        {
            foreach (CardTypeInfo info in _cardTypeInfo)
            {
                if (cardNumber.Length == info.Length &&
                    Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type;
            }

            return CardType.Unknown;
        }

    }
}

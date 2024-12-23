using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace touti_game_logic
{
    internal class Card : IComparable<Card>
    {
        public static readonly List<int> PossibleValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };
        public static readonly List<char> PossibleColors = new List<char> { 'o', 'c', 'e', 'b' };

        public int CardValue { get; private set; }
        public char CardColor { get; private set; }

        public string CardName
        {
            get
            {
                return $"{CardValue}{CardColor}";
            }
        }

        public static readonly Dictionary<int, int> CardPoints = new Dictionary<int, int>
                    {
                        { 1, 11 },
                        { 3, 10 },
                        { 12, 4 },
                        { 11, 3 },
                        { 10, 2 },
                        { 7, 0 },
                        { 6, 0 },
                        { 5, 0 },
                        { 4, 0 },
                        { 2, 0 }
                    };

        public Card(int cardValue, char cardColor)
        {
            if (!PossibleValues.Contains(cardValue))
                throw new ArgumentException("Invalid card value");
            if (!PossibleColors.Contains(cardColor))
                throw new ArgumentException("Invalid card color");

            CardValue = cardValue;
            CardColor = cardColor;
        }

        public int CompareTo(Card other)
        {
            if (other == null) return 1;
            return CardValue.CompareTo(other.CardValue);
        }

        public int CompareTo(Card other, char? fireColor, bool isGameplay = false)
        {
            if (other == null) return 1;

            char effectiveFireColor = fireColor ?? 'o';

            if (isGameplay)
            {
                // Define what makes a card a fire card
                bool isFireCard = CardColor == effectiveFireColor;
                bool isOtherFireCard = other.CardColor == effectiveFireColor;

                if (isFireCard && !isOtherFireCard)
                    return 1;
                if (!isFireCard && isOtherFireCard)
                    return -1;

                if (CardColor == other.CardColor)
                    return CardValue.CompareTo(other.CardValue);

                // If colors are different, the first card wins unless the second is a fire card
                return isOtherFireCard ? -1 : 1;
            }
            else
            {
                // Define color order
                List<char> colorOrder = new List<char> { 'o', 'c', 'e', 'b' };

                int thisColorIndex = colorOrder.IndexOf(CardColor);
                int otherColorIndex = colorOrder.IndexOf(other.CardColor);

                if (thisColorIndex != otherColorIndex)
                    return thisColorIndex.CompareTo(otherColorIndex);

                // If colors are the same, compare by card points
                int thisCardPoints = CardPoints[CardValue];
                int otherCardPoints = CardPoints[other.CardValue];

                if (thisCardPoints != otherCardPoints)
                    return thisCardPoints.CompareTo(otherCardPoints);

                // If points are the same, compare by card value
                return CardValue.CompareTo(other.CardValue);
            }
        }

        public static bool operator >(Card c1, (Card c2, char fireColor) t)
        {
            return c1.CompareTo(t.c2, t.fireColor) > 0;
        }

        public static bool operator <(Card c1, (Card c2, char fireColor) t)
        {
            return c1.CompareTo(t.c2, t.fireColor) < 0;
        }

        public static bool operator >=(Card c1, (Card c2, char fireColor) t)
        {
            return c1.CompareTo(t.c2, t.fireColor) >= 0;
        }

        public static bool operator <=(Card c1, (Card c2, char fireColor) t)
        {
            return c1.CompareTo(t.c2, t.fireColor) <= 0;
        }

        public string Serialize()
        {
            return CardName;
        }

        public static Card Deserialize(string cardName)
        {
            if (string.IsNullOrEmpty(cardName) || cardName.Length < 2)
                throw new ArgumentException("Invalid card name");

            int cardValue = int.Parse(cardName.Substring(0, cardName.Length - 1));
            char cardColor = cardName[cardName.Length - 1];

            return new Card(cardValue, cardColor);
        }
    }
}

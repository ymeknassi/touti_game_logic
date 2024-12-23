using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace touti_game_logic
{
    internal class Deck : IEnumerable<Card>
    {
        private List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        public void AddCards(IEnumerable<Card> newCards)
        {
            cards.AddRange(newCards);
        }

        public void Shuffle()
        {
            Random rng = new Random();
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public void Sort(char fireColor)
        {
            cards.Sort((card1, card2) => card1.CompareTo(card2, fireColor));
        }

        public List<Card> GetCards()
        {
            return new List<Card>(cards);
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Serialize()
        {
            if (cards == null || !cards.Any())
            {
                return "null";
            }
            return string.Join("-", cards.Select(card => card.Serialize()));
        }

        public static Deck Deserialize(string serializedDeck)
        {
            if (serializedDeck == "null")
            {
                return null;
            }

            var deck = new Deck();
            var cardStrings = serializedDeck.Split('-');
            foreach (var cardString in cardStrings)
            {
                deck.AddCard(Card.Deserialize(cardString));
            }
            return deck;
        }

        public static Deck CreateNewDeck()
        {
            var deck = new Deck();
            foreach (var color in Card.PossibleColors)
            {
                foreach (var value in Card.PossibleValues)
                {
                    deck.AddCard(new Card(value, color));
                }
            }
            return deck;
        }
    }
}

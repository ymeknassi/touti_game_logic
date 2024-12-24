using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace touti_game_logic
{
    internal static class GameData
    {
        public static int[] PartScore { get; set; } // Score for each team
        public static List<int[]> MatchScoreSheet { get; set; } // Scores at the end of each round
        public static char? PartFire { get; set; } // Fire color for the current round
        public static int? PartValue { get; set; } // Value of the round

        public static int? GamePhase { get; set; } = 0; // Phase of the game
        public static int[] PlayerEngagement { get; set; } // Engagement for each player
        public static int? EngagedPlayer { get; set; } // Player who got the engagement

        public static int? PartFirstPlayer { get; set; } // First player in the round

        public static int? CurrentPlayer { get; set; } // Current player
        public static Deck PlayedDeck { get; set; } = new Deck(); // Deck containing the cards played in the current play

        public static Deck[] PlayerDecks { get; set; } // Deck for each player
        public static Deck FullCardDeck { get; set; } // Full deck for the game

        public static Deck[] TeamsDecks { get; set; } // Decks for each team

        public static List<char> AlreadySung { get; set; } // Colors that were sung in the current round
        public static int? TurnSing { get; set; } // What was sung in the current turn
        public static int? Turn { get; set; } // Turn number
        public static int? TotalSing { get; set; } // Total sing made by the team of the engaged player in a round

        public static string Serialize()
        {
            var serializedData = new StringBuilder();

            if (PartScore != null)
                serializedData.Append($"PartScore:{string.Join(",", PartScore)}|");

            if (MatchScoreSheet != null)
                serializedData.Append($"MatchScoreSheet:{string.Join(";", MatchScoreSheet.Select(scores => string.Join(",", scores)))}|");

            if (PartFire.HasValue)
                serializedData.Append($"PartFire:{PartFire}|");

            if (PartValue.HasValue)
                serializedData.Append($"PartValue:{PartValue}|");

            if (GamePhase.HasValue)
                serializedData.Append($"GamePhase:{GamePhase}|");

            if (PlayerEngagement != null)
                serializedData.Append($"PlayerEngagement:{string.Join(",", PlayerEngagement)}|");

            if (EngagedPlayer.HasValue)
                serializedData.Append($"EngagedPlayer:{EngagedPlayer}|");

            if (PartFirstPlayer.HasValue)
                serializedData.Append($"PartFirstPlayer:{PartFirstPlayer}|");

            if (PlayedDeck != null)
                serializedData.Append($"PlayedDeck:{PlayedDeck.Serialize()}|");

            if (PlayerDecks != null)
                serializedData.Append($"PlayerDecks:{string.Join(";", PlayerDecks.Select(deck => deck.Serialize()))}|");

            if (FullCardDeck != null)
                serializedData.Append($"FullCardDeck:{FullCardDeck.Serialize()}|");

            if (TeamsDecks != null)
                serializedData.Append($"TeamsDecks:{string.Join(";", TeamsDecks.Select(deck => deck.Serialize()))}|");

            if (AlreadySung != null)
                serializedData.Append($"AlreadySung:{string.Join(",", AlreadySung)}|");

            if (TurnSing.HasValue)
                serializedData.Append($"TurnSing:{TurnSing}|");

            if (Turn.HasValue)
                serializedData.Append($"Turn:{Turn}|");

            if (TotalSing.HasValue)
                serializedData.Append($"TotalSing:{TotalSing}|");

            if (CurrentPlayer.HasValue)
                serializedData.Append($"CurrentPlayer:{CurrentPlayer}");

            // Remove the trailing '|'
            if (serializedData.Length > 0 && serializedData[serializedData.Length - 1] == '|')
            {
                serializedData.Length--;
            }

            return serializedData.ToString();
        }

        public static void Deserialize(string serializedData)
        {
            // Reset all properties to their initial values or null
            PartScore = null;
            MatchScoreSheet = null;
            PartFire = null;
            PartValue = null;
            GamePhase = 0;
            PlayerEngagement = null;
            EngagedPlayer = null;
            PartFirstPlayer = null;
            PlayedDeck = new Deck();
            PlayerDecks = null;
            FullCardDeck = null;
            TeamsDecks = null;
            AlreadySung = null;
            TurnSing = null;
            Turn = null;
            TotalSing = null;
            CurrentPlayer = null;

            var properties = serializedData.Split('|');

            foreach (var property in properties)
            {
                var keyValue = property.Split(':');
                var key = keyValue[0];
                var value = keyValue[1];

                switch (key)
                {
                    case "PartScore":
                        PartScore = value.Split(',').Select(int.Parse).ToArray();
                        break;
                    case "MatchScoreSheet":
                        MatchScoreSheet = value.Split(';').Select(scores => scores.Split(',').Select(int.Parse).ToArray()).ToList();
                        break;
                    case "PartFire":
                        PartFire = string.IsNullOrEmpty(value) ? (char?)null : value[0];
                        break;
                    case "PartValue":
                        PartValue = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "GamePhase":
                        GamePhase = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "PlayerEngagement":
                        PlayerEngagement = value.Split(',').Select(int.Parse).ToArray();
                        break;
                    case "EngagedPlayer":
                        EngagedPlayer = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "PartFirstPlayer":
                        PartFirstPlayer = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "PlayedDeck":
                        PlayedDeck = Deck.Deserialize(value);
                        break;
                    case "PlayerDecks":
                        PlayerDecks = value.Split(';').Select(Deck.Deserialize).ToArray();
                        break;
                    case "FullCardDeck":
                        FullCardDeck = Deck.Deserialize(value);
                        break;
                    case "TeamsDecks":
                        TeamsDecks = value.Split(';').Select(Deck.Deserialize).ToArray();
                        break;
                    case "AlreadySung":
                        AlreadySung = value.Split(',').Select(char.Parse).ToList();
                        break;
                    case "TurnSing":
                        TurnSing = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "Turn":
                        Turn = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "TotalSing":
                        TotalSing = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "CurrentPlayer":
                        CurrentPlayer = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace touti_game_logic
{
    internal class GameData
    {
        public int[] PartScore { get; set; } = new int[2]; // Score for each team
        public List<int[]> MatchScoreSheet { get; set; } = new List<int[]>(); // Scores at the end of each round
        public char? PartFire { get; set; } // Fire color for the current round
        public int? PartValue { get; set; } // Value of the round

        public int GamePhase { get; set; } // Phase of the game
        public int[] PlayerEngagement { get; set; } = new int[4]; // Engagement for each player
        public int? EngagedPlayer { get; set; } // Player who got the engagement

        public int PartFirstPlayer { get; set; } // First player in the round
        public Deck PlayedDeck { get; set; } = new Deck(); // Deck containing the cards played in the current play

        public Deck[] PlayerDecks { get; set; } = new Deck[4]; // Deck for each player
        public Deck FullCardDeck { get; set; } = new Deck(); // Full deck for the game

        public Deck[] TeamsDecks { get; set; } = new Deck[2]; // Decks for each team

        public List<char> AlreadySung { get; set; } = new List<char>(); // Colors that were sung in the current round
        public int TurnSing { get; set; } // What was sung in the current turn
        public int Turn { get; set; } // Turn number
        public int TotalSing { get; set; } // Total sing made by the team of the engaged player in a round

        public GameData()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayerDecks[i] = new Deck();
            }

            for (int i = 0; i < 2; i++)
            {
                TeamsDecks[i] = new Deck();
            }
        }

        public string Serialize()
        {
            var serializedData = new StringBuilder();
            serializedData.Append($"PartScore:{string.Join(",", PartScore)}|");
            serializedData.Append($"MatchScoreSheet:{string.Join(";", MatchScoreSheet.Select(scores => string.Join(",", scores)))}|");
            serializedData.Append($"PartFire:{PartFire}|");
            serializedData.Append($"PartValue:{PartValue}|");
            serializedData.Append($"GamePhase:{GamePhase}|");
            serializedData.Append($"PlayerEngagement:{string.Join(",", PlayerEngagement)}|");
            serializedData.Append($"EngagedPlayer:{EngagedPlayer}|");
            serializedData.Append($"PartFirstPlayer:{PartFirstPlayer}|");
            serializedData.Append($"PlayedDeck:{PlayedDeck.Serialize()}|");
            serializedData.Append($"PlayerDecks:{string.Join(";", PlayerDecks.Select(deck => deck.Serialize()))}|");
            serializedData.Append($"FullCardDeck:{FullCardDeck.Serialize()}|");
            serializedData.Append($"TeamsDecks:{string.Join(";", TeamsDecks.Select(deck => deck.Serialize()))}|");
            serializedData.Append($"AlreadySung:{string.Join(",", AlreadySung)}|");
            serializedData.Append($"TurnSing:{TurnSing}|");
            serializedData.Append($"Turn:{Turn}|");
            serializedData.Append($"TotalSing:{TotalSing}");
            return serializedData.ToString();
        }

        public static GameData Deserialize(string serializedData)
        {
            var gameData = new GameData();
            var properties = serializedData.Split('|');

            foreach (var property in properties)
            {
                var keyValue = property.Split(':');
                var key = keyValue[0];
                var value = keyValue[1];

                switch (key)
                {
                    case "PartScore":
                        gameData.PartScore = value.Split(',').Select(int.Parse).ToArray();
                        break;
                    case "MatchScoreSheet":
                        gameData.MatchScoreSheet = value.Split(';').Select(scores => scores.Split(',').Select(int.Parse).ToArray()).ToList();
                        break;
                    case "PartFire":
                        gameData.PartFire = string.IsNullOrEmpty(value) ? (char?)null : value[0];
                        break;
                    case "PartValue":
                        gameData.PartValue = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "GamePhase":
                        gameData.GamePhase = int.Parse(value);
                        break;
                    case "PlayerEngagement":
                        gameData.PlayerEngagement = value.Split(',').Select(int.Parse).ToArray();
                        break;
                    case "EngagedPlayer":
                        gameData.EngagedPlayer = string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value);
                        break;
                    case "PartFirstPlayer":
                        gameData.PartFirstPlayer = int.Parse(value);
                        break;
                    case "PlayedDeck":
                        gameData.PlayedDeck = Deck.Deserialize(value);
                        break;
                    case "PlayerDecks":
                        gameData.PlayerDecks = value.Split(';').Select(Deck.Deserialize).ToArray();
                        break;
                    case "FullCardDeck":
                        gameData.FullCardDeck = Deck.Deserialize(value);
                        break;
                    case "TeamsDecks":
                        gameData.TeamsDecks = value.Split(';').Select(Deck.Deserialize).ToArray();
                        break;
                    case "AlreadySung":
                        gameData.AlreadySung = value.Split(',').Select(char.Parse).ToList();
                        break;
                    case "TurnSing":
                        gameData.TurnSing = int.Parse(value);
                        break;
                    case "Turn":
                        gameData.Turn = int.Parse(value);
                        break;
                    case "TotalSing":
                        gameData.TotalSing = int.Parse(value);
                        break;
                }
            }

            return gameData;
        }
    }
}

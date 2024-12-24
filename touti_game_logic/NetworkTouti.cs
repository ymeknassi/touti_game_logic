using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Photon.Realtime;
using Photon.Client;

namespace touti_game_logic
{

    internal class NetworkTouti
    {
        private RealtimeClient client;
        private Dictionary<int, bool> acknowledgments = new Dictionary<int, bool>();

        public NetworkTouti(RealtimeClient client)
        {
            this.client = client;
            this.client.EventReceived += OnEvent; // Register the callback for receiving events
            Console.WriteLine("EventReceived callback registered.");
        }

        public void StartGame()
        {
            Console.WriteLine("StartGame()");

            if (IsMasterPlayer())
            {
                //AdvanceGamePhase();
                InitializeNewGame();
            }
        }

        private bool IsMasterPlayer()
        {
            return client.LocalPlayer.IsMasterClient;
        }

        private void AdvanceGamePhase()
        {
            switch (GameData.GamePhase)
            {
                case null:
                case 0:
                    InitializeNewGame();
                    break;
                case 1:
                    StartNewRound();
                    break;
                case 2:
                    EngagementPhase();
                    break;
                case 3:
                    PlayCardsPhase();
                    break;
                // Add cases for phases 4, 5, 6 as needed
                default:
                    Console.WriteLine("Unknown game phase.");
                    break;
            }

            // Wait for acknowledgments from all clients before proceeding
            if (IsMasterPlayer())
            {
                WaitForAcknowledgments();
            }
        }

        private void InitializeNewGame()
        {
            Console.WriteLine("Initializing new game...");
            GameData.FullCardDeck = Deck.CreateNewDeck();
            GameData.FullCardDeck.Shuffle();
            Random random = new Random();
            GameData.PartFirstPlayer = random.Next(0, 4);
            GameData.GamePhase = 1;
            SendGameData();
            AdvanceGamePhase();
        }

        private void StartNewRound()
        {
            Console.WriteLine("Starting new round...");
            if (GameData.PartScore == null)
            {
                GameData.PartScore = new int[2];
            }
            GameData.PlayerDecks = new Deck[4];
            for (int i = 0; i < 4; i++)
            {
                GameData.PlayerDecks[i] = new Deck();
            }
            DistributeCards();
            GameData.GamePhase = 2;
            SendGameData();
            AdvanceGamePhase();
        }

        private void EngagementPhase()
        {
            Console.WriteLine("Engagement phase...");
            Random random = new Random();
            GameData.PartValue = GetRandomPartScore(random);
            GameData.PartFire = GetRandomCardColor(random);
            GameData.EngagedPlayer = random.Next(0, 4);
            GameData.GamePhase = 3;
            GameData.CurrentPlayer = GameData.PartFirstPlayer;
            SendGameData();
            AdvanceGamePhase();
        }

        private void PlayCardsPhase()
        {
            Console.WriteLine("Playing cards phase...");
            for (int i = 0; i < 4; i++)
            {
                //PrintGameData(); // Print game data before each turn
                PlayTurn();
                GameData.CurrentPlayer = (GameData.CurrentPlayer + 1) % 4;
                SendGameData();
            }

            DetermineTurnWinner();
            GameData.GamePhase = 3; // Continue to the next turn
            SendGameData();
            AdvanceGamePhase();
        }

        private void PlayTurn()
        {
            Console.WriteLine($"Player {GameData.CurrentPlayer}'s turn. Enter the card name:");
            Console.Out.Flush(); // Ensure previous output is displayed

            string cardName = Console.ReadLine();
            Card playedCard = GameData.PlayerDecks[GameData.CurrentPlayer.Value].GetCards().FirstOrDefault(c => c.CardName == cardName);

            if (playedCard == null)
            {
                Console.WriteLine("Invalid card. Try again.");
                PlayTurn();
                return;
            }

            // Validate the card according to the rules
            if (!IsValidCard(playedCard))
            {
                Console.WriteLine("Invalid card according to the rules. Try again.");
                PlayTurn();
                return;
            }

            GameData.PlayerDecks[GameData.CurrentPlayer.Value].RemoveCard(playedCard);
            GameData.PlayedDeck.AddCard(playedCard);
        }

        private bool IsValidCard(Card card)
        {
            // Implement the rules for validating the card
            // This method should return true if the card is valid according to the rules, otherwise false
            return true; // Placeholder, implement the actual validation logic
        }

        private void DetermineTurnWinner()
        {
            // Implement the logic to determine the winner of the turn
            // The winner is the player with the strongest card according to the rules
            // Update GameData.PartFirstPlayer to the winner of the turn
        }

        private void DistributeCards()
        {
            Console.WriteLine("DistributeCards()");
            int currentPlayer = GameData.PartFirstPlayer.Value;
            while (GameData.FullCardDeck.GetCards().Count > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (GameData.FullCardDeck.GetCards().Count > 0)
                        {
                            Card card = GameData.FullCardDeck.GetCards().First();
                            GameData.FullCardDeck.RemoveCard(card);
                            GameData.PlayerDecks[currentPlayer].AddCard(card);
                        }
                    }
                    currentPlayer = (currentPlayer + 1) % 4;
                }
            }

            // Sort each player's deck
            for (int i = 0; i < 4; i++)
            {
                GameData.PlayerDecks[i].Sort(GameData.PartFire ?? 'o');
            }

            // Set FullCardDeck to null after distributing the cards
            GameData.FullCardDeck = null;
        }

        private int GetRandomPartScore(Random random)
        {
            int[] possibleScores = { 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210 };
            int index = random.Next(0, possibleScores.Length);
            return possibleScores[index];
        }

        private char GetRandomCardColor(Random random)
        {
            char[] possibleColors = { 'o', 'c', 'e', 'b' };
            int index = random.Next(0, possibleColors.Length);
            return possibleColors[index];
        }

        private void SendGameData()
        {
            Console.WriteLine("SendGameData()");

            // Serialize the game data
            string serializedGameData = GameData.Serialize();
            Console.WriteLine($"Serialized Game Data: {serializedGameData}");

            // Create the event arguments
            RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.All };

            // Send the event
            bool success = client.OpRaiseEvent(1, serializedGameData, raiseEventArgs, SendOptions.SendReliable);
            Console.WriteLine($"Game data sent: {success}");
        }

        public void OnEvent(EventData photonEvent)
        {
            Console.WriteLine($"OnEvent called with event code: {photonEvent.Code}");
            if (photonEvent.Code == 1)
            {
                Console.WriteLine("Received game data from master.");
                string serializedGameData = (string)photonEvent.CustomData;
                GameData.Deserialize(serializedGameData);
                PrintGameData();

                // Send acknowledgment back to the master
                SendAcknowledgment();
            }
            else if (photonEvent.Code == 2)
            {
                int playerId = (int)photonEvent.CustomData;
                acknowledgments[playerId] = true;

                // Print acknowledgment received
                Console.WriteLine($"Acknowledgment received from player {playerId}");
            }
        }

        private void SendAcknowledgment()
        {
            RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient };
            bool success = client.OpRaiseEvent(2, client.LocalPlayer.ActorNumber, raiseEventArgs, SendOptions.SendReliable);
            Console.WriteLine($"Acknowledgment sent: {success}");
        }

        private void WaitForAcknowledgments()
        {
            acknowledgments.Clear();
            foreach (var player in client.CurrentRoom.Players.Values)
            {
                if (player.ActorNumber != client.LocalPlayer.ActorNumber)
                {
                    acknowledgments[player.ActorNumber] = false;
                }
            }

            while (acknowledgments.Values.Contains(false))
            {
                client.Service();
                Thread.Sleep(10); // Reduce sleep duration to ensure frequent servicing
            }
        }

        private void PrintGameData()
        {
            Console.WriteLine("Game Data: " + GameData.Serialize());
            Console.WriteLine();
            Console.Out.Flush(); // Ensure previous output is displayed
                                 // Print other game data properties as needed
        }
    }

  
}

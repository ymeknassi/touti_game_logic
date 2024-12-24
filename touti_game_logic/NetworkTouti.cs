using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Realtime;
using Photon.Client;
using System.Threading;

namespace touti_game_logic
{

    internal class NetworkTouti : IDisposable
    {
        private RealtimeClient client;
        private Dictionary<int, bool> acknowledgments = new Dictionary<int, bool>();
        private Thread sendOutgoingCommandsThread;
        private bool isRunning = true;

        public NetworkTouti(RealtimeClient client)
        {
            this.client = client;
            this.client.EventReceived += OnEvent; // Register the callback for receiving events

            // Start the background thread to send outgoing commands
            //sendOutgoingCommandsThread = new Thread(SendOutgoingCommandsLoop);
            //sendOutgoingCommandsThread.Start();
        }

        private void SendOutgoingCommandsLoop()
        {
            while (isRunning)
            {
                //Console.WriteLine("SendOutoing()");
                client.SendOutgoingCommands();
                Thread.Sleep(10000); // Sleep for 10 seconds
            }
        }

        public void Dispose()
        {
            isRunning = false;
            sendOutgoingCommandsThread.Join();
        }

        public void StartGame()
        {
            Console.WriteLine("StartGame()");
            if (IsMasterPlayer())
            {
                AdvanceGamePhase();
            }
        }

        private void AdvanceGamePhase()
        {
            Console.WriteLine("AdvanceGamePhase()");
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
        }

        private void StartNewRound()
        {
            DistributeCards();
            GameData.GamePhase = 2;
            SendGameData();
            AdvanceGamePhase();
        }

        private void InitializeNewGame()
        {
            Console.WriteLine("InitializeNewGame()");
            if (IsMasterPlayer())
            {
                if (GameData.FullCardDeck == null)
                {
                    InitializeGameDeck();
                }

                if (GameData.PartFirstPlayer == null)
                {
                    Random random = new Random();
                    GameData.PartFirstPlayer = 4;// random.Next(1, 4);
                }
                else
                {
                    GameData.PartFirstPlayer = (GameData.PartFirstPlayer + 1) % 4;
                }
                GameData.PlayedDeck = new Deck();

                GameData.GamePhase = 1;

                SendGameData();
                AdvanceGamePhase();
            }
        }

        private void PlayCardsPhase()
        {
            Console.WriteLine("Play cards phase...");
            if (IsMasterPlayer())
            {
                if (GameData.GamePhase == 3)
                {
                    if (GameData.CurrentPlayer == client.LocalPlayer.ActorNumber)
                    {
                        PlayerInputCard();
                    }
                    else
                    {
                        CallPlayerInputCard(GameData.CurrentPlayer.Value);
                        WaitForPlayerMove();
                    }
                }
            }
        }

        private void CallPlayerInputCard(int playerId)
        {
            Console.WriteLine("CallPlayerInputCard()");
            RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.All };
            client.OpRaiseEvent(4, playerId, raiseEventArgs, SendOptions.SendReliable);
            client.SendOutgoingCommands();
        }

        private void WaitForPlayerMove()
        {
            Console.WriteLine("WaitForPlayerMove()");
            while (GameData.GamePhase == 3 && GameData.CurrentPlayer != client.LocalPlayer.ActorNumber)
            {
                client.Service();
                Thread.Sleep(10); // Reduce sleep duration to ensure frequent servicing
            }
        }

        private void PlayerInputCard()
        {
            if (GameData.GamePhase == 3 && GameData.CurrentPlayer == client.LocalPlayer.ActorNumber)
            {
                while (true)
                {
                    Console.WriteLine($"Player {GameData.CurrentPlayer}'s turn. Enter the card name:");
                    Console.Out.Flush(); // Ensure previous output is displayed

                    string cardName = Console.ReadLine();
                    if (!string.IsNullOrEmpty(cardName))
                    {
                        SendCardNameToMaster(cardName);
                        break; // Exit the loop after sending the card name
                    }
                }
            }
        }

        private void SendCardNameToMaster(string cardName)
        {

            if (GameData.GamePhase == 3 && GameData.CurrentPlayer == client.LocalPlayer.ActorNumber)
            {
                RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient };
                client.OpRaiseEvent(3, cardName, raiseEventArgs, SendOptions.SendReliable);
                client.SendOutgoingCommands();

                Console.WriteLine($"Card name '{cardName}' sent to master.");
            }
        }

        private void ProcessPlayerMove(string cardName, int playerId)
        {
            if (GameData.GamePhase == 3 && GameData.CurrentPlayer == playerId)
            {
                Card playedCard = GameData.PlayerDecks[playerId - 1].GetCards().FirstOrDefault(c => c.CardName == cardName);

                if (playedCard != null && IsValidCard(playedCard))
                {

                    GameData.PlayerDecks[playerId - 1].RemoveCard(playedCard);
                    GameData.PlayedDeck.AddCard(playedCard);
                    GameData.CurrentPlayer = (GameData.CurrentPlayer % 4) + 1; // Ensure CurrentPlayer is between 1 and 4

                    if (GameData.PlayerDecks[GameData.CurrentPlayer.Value - 1].GetCards().Count != 0)
                    {
                        
                        SendGameData();
                        AdvanceGamePhase();
                    }
                    else
                    { 
                        GameData.GamePhase = 4;
                        SendGameData();
                        AdvanceGamePhase();

                    }
                }
                else
                {
                    Console.WriteLine($"Invalid move by player {playerId} with card '{cardName}'.");
                }
            }
        }

        private bool IsValidCard(Card card)
        {
            if (GameData.CurrentPlayer.HasValue && GameData.PlayerDecks[GameData.CurrentPlayer.Value - 1].Contains(card))
                return true;
            else
                return false;
        }

        private void EngagementPhase()
        {
            Console.WriteLine("Engagement phase...");
            Random random = new Random();

            int[] possibleScores = { 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210 };
            int index = random.Next(0, possibleScores.Length);

            GameData.PartValue = possibleScores[index];

            char[] possibleColors = { 'o', 'c', 'e', 'b' };
            int indexf = random.Next(0, possibleColors.Length);

            GameData.PartFire = possibleColors[indexf];
            GameData.EngagedPlayer = random.Next(1, 4);

            GameData.CurrentPlayer = GameData.PartFirstPlayer;

            GameData.GamePhase = 3;
            SendGameData();
            AdvanceGamePhase();
            // Implement the engagement phase
            // Update the game data
            // Send the updated game data
            // Advance the game phase
        }

        private bool IsMasterPlayer()
        {
            return client.LocalPlayer.IsMasterClient;
        }

        private void InitializeGameDeck()
        {
            Console.WriteLine("InitializeGameDeck()");
            GameData.FullCardDeck = Deck.CreateNewDeck();
            GameData.FullCardDeck.Shuffle();
        }

        private void DistributeCards()
        {
            Console.WriteLine("DistributeCards()");
            GameData.PlayerDecks = new Deck[4];
            for (int i = 0; i < 4; i++)
            {
                GameData.PlayerDecks[i] = new Deck();
            }

            int currentPlayer = (GameData.PartFirstPlayer.Value % 4) + 1;
            while (GameData.FullCardDeck.GetCards().Count > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (GameData.FullCardDeck.GetCards().Count > 0)
                        {
                            Card card = GameData.FullCardDeck.GetCards().First();
                            GameData.FullCardDeck.RemoveCard(card); // Fixed line
                            GameData.PlayerDecks[currentPlayer - 1].AddCard(card);
                        }
                    }
                    currentPlayer = (currentPlayer % 4) + 1; // Ensure currentPlayer is between 1 and 4
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

        private void SendGameData()
        {
            Console.WriteLine("SendGameData()");
            string serializedGameData = GameData.Serialize();
            RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.All };
            client.OpRaiseEvent(1, serializedGameData, raiseEventArgs, SendOptions.SendReliable);
            client.SendOutgoingCommands();
            WaitForAcknowledgments();
        }

        private void PrintGameData()
        {
            Console.WriteLine("Game Data: " + GameData.Serialize());
        }

        public void OnEvent(EventData photonEvent)
        {
            Console.WriteLine($"OnEvent called with event code: {photonEvent.Code}");
            if (photonEvent.Code == 1)
            {
                string serializedGameData = (string)photonEvent.CustomData;
                GameData.Deserialize(serializedGameData);
                PrintGameData();
                SendAcknowledgment();
            }
            else if (photonEvent.Code == 2)
            {
                int playerId = (int)photonEvent.CustomData;
                acknowledgments[playerId] = true;

                // Print acknowledgment received
                Console.WriteLine($"Acknowledgment received from player {playerId}");
            }
            else if (photonEvent.Code == 3)
            {
                string cardName = (string)photonEvent.CustomData;
                int playerId = photonEvent.Sender;
                ProcessPlayerMove(cardName, playerId);
            }
            else if (photonEvent.Code == 4)
            {
                int playerId = (int)photonEvent.CustomData;
                if (playerId == client.LocalPlayer.ActorNumber)
                {
                    PlayerInputCard();
                }
            }
        }

        private void SendAcknowledgment()
        {
            RaiseEventArgs raiseEventArgs = new RaiseEventArgs { Receivers = ReceiverGroup.MasterClient };
            bool success = client.OpRaiseEvent(2, client.LocalPlayer.ActorNumber, raiseEventArgs, SendOptions.SendReliable);
            client.SendOutgoingCommands();
            Console.WriteLine($"Acknowledgment sent: {success}");
        }

        private void WaitForAcknowledgments()
        {
            Console.WriteLine($"waiting for acknowledgments...");

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

            Console.WriteLine($"acknowledgments received");
        }
    }

  
}

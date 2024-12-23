using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Realtime;
using Photon.Client;

namespace touti_game_logic
{

    internal class NetworkTouti
    {
        private RealtimeClient client;

        public NetworkTouti(RealtimeClient client)
        {
            this.client = client;
            this.client.EventReceived += OnEvent; // Register the callback for receiving events
        }

        public void StartGame()
        {
            Console.WriteLine("StartGame()");

            if (IsMasterPlayer())
            {
                if (GameData.FullCardDeck == null)
                {
                    InitializeGameDeck();
                }

                if (GameData.PartFirstPlayer == null)
                {
                    Random random = new Random();
                    GameData.PartFirstPlayer = random.Next(0, 4);
                }
                else
                {
                    GameData.PartFirstPlayer = (GameData.PartFirstPlayer + 1) % 4;
                }

                if (GameData.PartFire == null)
                {
                    Random random = new Random();
                    GameData.PartFire = Card.PossibleColors[random.Next(Card.PossibleColors.Count)];
                }

                if (GameData.PlayerDecks == null)
                {
                    DistributeCards();
                }

                SendGameData();
            }
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
            // Initialize other game data properties as needed
        }

        private void DistributeCards()
        {
            Console.WriteLine("DistributeCards()");
            GameData.PlayerDecks = new Deck[4];
            for (int i = 0; i < 4; i++)
            {
                GameData.PlayerDecks[i] = new Deck();
            }

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
                            GameData.FullCardDeck.RemoveCard(card); // Fixed line
                            GameData.PlayerDecks[currentPlayer].AddCard(card);
                        }
                    }
                    currentPlayer = (currentPlayer + 1) % 4;
                }
            }

            // Sort each player's deck
            for (int i = 0; i < 4; i++)
            {
                GameData.PlayerDecks[i].Sort(GameData.PartFire ?? ' ');
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
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == 1)
            {
                string serializedGameData = (string)photonEvent.CustomData;
                GameData.Deserialize(serializedGameData);
                UpdateGameData();
            }
        }

        private void UpdateGameData()
        {
            // Update the client's game data
            PrintGameData();
        }

        private void PrintGameData()
        {
            Console.WriteLine("Game Data: " + GameData.Serialize());
            // Print other game data properties as needed
        }
    }

  
}

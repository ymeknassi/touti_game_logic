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
                InitializeGameData();
            }
        }

        private bool IsMasterPlayer()
        {
            return client.LocalPlayer.IsMasterClient;
        }

        private void InitializeGameData()
        {
            Console.WriteLine("InitializeGameData()");
            GameData.FullCardDeck = Deck.CreateNewDeck();
            GameData.FullCardDeck.Shuffle();
            SendGameData();

            // Initialize other game data properties as needed
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
            Console.WriteLine("Game Data: " + GameData.FullCardDeck.Serialize());
            // Print other game data properties as needed
        }
    }

  
}

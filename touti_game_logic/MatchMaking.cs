using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;
using Photon.Client;
namespace touti_game_logic
{
    class MatchMaking : IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks
    {
        private RealtimeClient client = new RealtimeClient();
        private bool quit;
        private NetworkTouti networkTouti;

        ~MatchMaking()
        {
            this.client.Disconnect();
            this.client.RemoveCallbackTarget(this);
        }

        public void StartClient()
        {
            this.client.AddCallbackTarget(this);
            this.client.StateChanged += this.OnStateChange;

            this.client.ConnectUsingSettings(new AppSettings() { AppIdRealtime = "ff9b7d06-6d94-4a3b-948f-336bbc30e7c9", FixedRegion = "usw" });
            //this.client.RealtimePeer.DisconnectTimeout = 120000;
            Thread t = new Thread(this.Loop);
            t.Start();

            //.WriteLine("Running until key pressed.");
            //Console.ReadKey();
            //this.quit = true;
        }

        private void QuickMatch()
        {
            client.OpJoinRandomOrCreateRoom(null, null);
        }

        private void Loop(object state)
        {
            while (!this.quit)
            {
                this.client.Service();
                Thread.Sleep(10);
                this.client.SendOutgoingCommands();
            }
        }

        private void OnStateChange(ClientState arg1, ClientState arg2)
        {
            Console.WriteLine(arg1 + " -> " + arg2);
        }

        // from IConnectionCallbacks:

        public void OnConnectedToMaster()
        {
            Console.WriteLine("OnConnectedToMaster Server: " + this.client.CurrentServerAddress);
            this.QuickMatch();
        }

        public void OnConnected()
        {
            Console.WriteLine("OnConnected");
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Console.WriteLine("OnDisconnected: " + cause);
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            Console.WriteLine("OnRegionListReceived");
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            Console.WriteLine("OnCustomAuthenticationResponse");
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Console.WriteLine("OnCustomAuthenticationFailed: " + debugMessage);
        }

        #region IMatchmakingCallbacks

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            Console.WriteLine("OnFriendListUpdate");
        }

        public void OnCreatedRoom()
        {
            Console.WriteLine("OnCreatedRoom");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Console.WriteLine("OnCreateRoomFailed: " + message);
        }

        public void OnJoinedRoom()
        {
            Console.WriteLine("OnJoinedRoom");
            if (client.LocalPlayer.IsMasterClient)
            {
                Console.WriteLine("Master Client");
                this.client.CurrentRoom.MaxPlayers = 4;
                
                if (client.CurrentRoom.PlayerCount < 4)   ///TEMP ONLY FOR DEV PURPOSES AND TOSTART OTHER GAMES INSTANCES AUTOMATICALLY
                {
                    ExecuteOtherGameInstances.StartInstances(3);
                }


            }
            else
            {
                Console.WriteLine("Not Master Client");
            }

            Console.WriteLine("Room Name: " + client.CurrentRoom.Name);
            ListPlayers();

            // Initialize NetworkTouti
            networkTouti = new NetworkTouti(client);

        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Console.WriteLine("OnPlayerEnteredRoom: " + newPlayer.ActorNumber);
            ListPlayers();

            // Start the game if the room has 4 players and the local player is the master client
            if (client.CurrentRoom.PlayerCount == 4 && client.LocalPlayer.IsMasterClient)
            {
                networkTouti.StartGame();
            }
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Console.WriteLine("OnJoinRoomFailed: " + message);
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Console.WriteLine("OnJoinRandomFailed: " + message);
        }

        public void OnLeftRoom()
        {
            Console.WriteLine("OnLeftRoom");
        }
        #endregion

        #region IInRoomCallbacks

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Console.WriteLine("OnPlayerLeftRoom: " + otherPlayer.NickName);
        }

        public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
        {
            Console.WriteLine("OnRoomPropertiesUpdate");            

        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
        {
            Console.WriteLine("OnPlayerPropertiesUpdate: " + targetPlayer.NickName);
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            Console.WriteLine("OnMasterClientSwitched: " + newMasterClient.NickName);
        }

        #endregion
        private void ListPlayers()
        {
            foreach (var player in client.CurrentRoom.Players.Values)
            {
                string playerInfo = $"Player: {player.ActorNumber}";
                if (player.IsMasterClient)
                {
                    playerInfo += " (Master Client)";
                }
                if (player.ActorNumber == client.LocalPlayer.ActorNumber)
                {
                    playerInfo += " (Current Client)";
                }
                Console.WriteLine(playerInfo);
            }
        }


    }
}

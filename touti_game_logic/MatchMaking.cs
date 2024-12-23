using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;
using Photon.Client;

class MatchMaking : IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks
{
    private RealtimeClient client = new RealtimeClient();
    private bool quit;

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

        Thread t = new Thread(this.Loop);
        t.Start();

        Console.WriteLine("Running until key pressed.");
        Console.ReadKey();
        this.quit = true;
    }

    private void QuickMatch()
    {
        client.OpJoinRandomOrCreateRoom(null, null); ;
    }

    private void Loop(object state)
    {
        while (!this.quit)
        {
            this.client.Service();
            Thread.Sleep(33);
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
        if(client.LocalPlayer.IsMasterClient)
        {
            Console.WriteLine("Master Client");
            this.client.CurrentRoom.MaxPlayers = 4;
        }
        else
        {
            Console.WriteLine("Not Master Client");
        }

        Console.WriteLine("Room Name: " + client.CurrentRoom.Name);
        ListPlayers();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    { // This is a method from the IMatchmakingCallbacks interface
        Console.WriteLine("OnPlayerEnteredRoom: " + newPlayer.ActorNumber);
        ListPlayers();
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
            Console.WriteLine("Player: " + player.NickName);
        }
    }
}

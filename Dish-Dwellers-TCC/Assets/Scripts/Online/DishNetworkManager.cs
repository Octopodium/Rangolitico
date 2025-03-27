using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class DishNetworkManager : NetworkManager {
    [Header("Dish Network Manager")]
    public GameObject heaterPrefab;
    public GameObject anglerPrefab;

    public bool isPlayerOneHeater = true;
    public LobbyPlayer[] lobbyPlayers = new LobbyPlayer[2];
    public System.Action<bool> OnTrocarPersonagens;

    public enum Personagem { Heater, Angler }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        GameObject player = Instantiate(playerPrefab);

        LobbyPlayer lobbyPlayer = player.GetComponent<LobbyPlayer>();
        lobbyPlayer.isPlayerOne = lobbyPlayers[0] == null;
        lobbyPlayer.nome = isPlayerOneHeater ? "Juanzinho" : "GameMaster 3000";
        
        if (lobbyPlayer.isPlayerOne) lobbyPlayers[0] = lobbyPlayer;
        else lobbyPlayers[1] = lobbyPlayer;

        player.name = $"[connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        base.OnServerDisconnect(conn);
        
        if (lobbyPlayers[0].connectionToClient == conn) lobbyPlayers[0] = null;
        else lobbyPlayers[1] = null;
    }

    [Server]
    public void TrocarPersonagens() {
        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers) {
            lobbyPlayer.pronto = false;
        }

        isPlayerOneHeater = !isPlayerOneHeater;
        OnTrocarPersonagens?.Invoke(isPlayerOneHeater);
    }
}

using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class DishNetworkManager : NetworkManager {
    [Header("Dish Network Manager")]
    public GameObject heaterPrefab;
    public GameObject anglerPrefab;

    public LobbyPlayer[] lobbyPlayers;

    public enum Personagem { Heater, Angler }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        if (lobbyPlayers.Length == 0) {
            lobbyPlayers = new LobbyPlayer[2];
        } else if (lobbyPlayers[0] != null && lobbyPlayers[1] != null) {
            return;
        }

        GameObject player = Instantiate(playerPrefab);

        LobbyPlayer lobbyPlayer = player.GetComponent<LobbyPlayer>();
        lobbyPlayer.isPlayerOne = lobbyPlayers[0] == null;

        if (lobbyPlayer.isPlayerOne) lobbyPlayers[0] = lobbyPlayer;
        else lobbyPlayers[1] = lobbyPlayer;

        lobbyPlayer.personagem = GetPersonagemNaoUsado();
        lobbyPlayer.nome = (lobbyPlayer.isPlayerOne) ? "Juanzinho" : "GameMaster 3000";

        player.name = $"[connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    Personagem GetPersonagemNaoUsado() {
        if (lobbyPlayers[0] == null && lobbyPlayers[1] == null) return Personagem.Heater;
        if (lobbyPlayers[0] == null) return (lobbyPlayers[1].personagem == Personagem.Heater) ? Personagem.Angler : Personagem.Heater;
        if (lobbyPlayers[1] == null) return (lobbyPlayers[0].personagem == Personagem.Heater) ? Personagem.Angler : Personagem.Heater;
        return Personagem.Heater;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        if (lobbyPlayers[0] != null && lobbyPlayers[0].connectionToClient == conn) {
            lobbyPlayers[0].FoiDesconectado();
            lobbyPlayers[0] = null;
        }
        else if (lobbyPlayers[1] != null && lobbyPlayers[1].connectionToClient == conn) {
            lobbyPlayers[1].FoiDesconectado();
            lobbyPlayers[1] = null;
        }

        base.OnServerDisconnect(conn);
    }

    [Server]
    public void TrocarPersonagens() {
        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers) {
            if (lobbyPlayer == null) continue;
            lobbyPlayer.pronto = false;

            if (lobbyPlayer.personagem == Personagem.Heater) lobbyPlayer.personagem = Personagem.Angler;
            else  lobbyPlayer.personagem = Personagem.Heater;
        }
    }

    [Server]
    LobbyPlayer GetLobbyPlayer(NetworkConnectionToClient conn) {
        if (lobbyPlayers == null) return null;

        foreach (LobbyPlayer lobbyPlayer in lobbyPlayers) {
            if (lobbyPlayer == null) continue;
            if (lobbyPlayer.connectionToClient == conn) return lobbyPlayer;
        }

        return null;
    }

    [Server]
    public void SetPronto(NetworkConnectionToClient conn, bool pronto) {
        LobbyPlayer lobbyPlayer = GetLobbyPlayer(conn);
        if (lobbyPlayer == null) {
            Debug.LogError("Jogador " + conn + " não encontrado!");
            return;
        }

        lobbyPlayer.pronto = pronto;
    }
}

using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class DishNetworkManager : NetworkManager {
    [Header("Dish Network Manager")]

    public GameObject heaterPrefab;
    public GameObject anglerPrefab;

    public LobbyPlayer[] lobbyPlayers; // Players do lobby (para escolher personagem)
    public Player[] players; // Players instanciados na cena (são criados a partir de um LobbyPlayer)

    public enum Personagem { Heater, Angler }


    public override void Awake() {
        base.Awake();

        NetworkServer.RegisterHandler<RequestPassaDeSalaMessage>(OnRequestedPassaDeSala);
    }


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
        lobbyPlayer.nome = (lobbyPlayer.isPlayerOne) ? "Player 1" : "Player 2";

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

        if (players != null && players.Length > 0) {
            if (players[0] != null) players[0].conectado = false;
            if (players.Length > 1 && players[1] != null) players[1].conectado = false;
        }

        base.OnServerDisconnect(conn);
    }

    #region No Lobby

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

    public void IniciarJogo() {
        if (lobbyPlayers[0] == null || lobbyPlayers[1] == null) return;
        if (!lobbyPlayers[0].pronto || !lobbyPlayers[1].pronto) return;

        players = new Player[lobbyPlayers.Length];

        for (int i = 0; i < lobbyPlayers.Length; i++) {
            LobbyPlayer lobbyPlayer = lobbyPlayers[i];
            if (lobbyPlayer == null) continue;

            GameObject playerPrefab = (lobbyPlayer.personagem == Personagem.Heater) ? heaterPrefab : anglerPrefab;

            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.name = $"{lobbyPlayer.nome} [connId={lobbyPlayer.connectionToClient.connectionId}]";
            NetworkServer.ReplacePlayerForConnection(lobbyPlayer.connectionToClient, player, true);

            players[i] = player.GetComponent<Player>();
            Destroy(lobbyPlayer.gameObject);
        }

        foreach (Player player in players) {
            player.conectado = true;
        }
    }

    #endregion No Lobby

    #region In Game

    public struct RequestPassaDeSalaMessage : NetworkMessage {
        public bool passarDeSala;

        public RequestPassaDeSalaMessage(bool passarDeSala = true) {
            this.passarDeSala = passarDeSala;
        }
    }

    public struct AcaoPassaDeSalaMessage : NetworkMessage {
        public bool passarDeSala;

        public AcaoPassaDeSalaMessage(bool passarDeSala = true) {
            this.passarDeSala = passarDeSala;
        }
    }

    private void OnRequestedPassaDeSala(NetworkConnectionToClient conn, RequestPassaDeSalaMessage msg) {
        if (players == null || players.Length == 0) return;
        if (players[0] == null || players[1] == null) return;

        NetworkServer.SendToAll(new AcaoPassaDeSalaMessage(msg.passarDeSala));
    }

    #endregion In Game
}

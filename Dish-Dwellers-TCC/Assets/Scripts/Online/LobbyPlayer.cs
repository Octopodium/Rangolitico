using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Classe que representa um jogador no lobby (antes de entrar no jogo)
/// </summary>
public class LobbyPlayer : NetworkBehaviour {

    public float atualizaPingACada = 0.5f; // Tempo em segundos para atualizar o ping

    [SyncVar] public bool isPlayerOne = false;
    [SyncVar(hook = nameof(UpdateNomeUI))] public string nome;
    [SyncVar(hook = nameof(UpdatePingUI))] public int ping;
    [SyncVar(hook = nameof(UpdatePersonagemUI))] public DishNetworkManager.Personagem personagem;
    [SyncVar(hook = nameof(UpdateSetProntoUI))] public bool pronto = false;

    void Start(){
        if (isPlayerOne) ConnectionUI.instance.p1 = this;
        else ConnectionUI.instance.p2 = this;

        ConnectionUI.instance.UpdateNominhos();
        ConnectionUI.instance.UpdateAguardandoJogadorUI();

        if (ConnectionUI.instance != null) {
            ConnectionUI.instance.EntrouNoLobby();
        }

        if (isLocalPlayer) {
            StartCoroutine(PingCoroutine());
        }
    }

    IEnumerator PingCoroutine() {
        DishNetworkManager manager = (DishNetworkManager)NetworkManager.singleton;
        
        while (gameObject.activeSelf) {
            int pingV = manager.GetCurrentPingInMs();
            SetPing(pingV);
            yield return new WaitForSeconds(atualizaPingACada); // Espera N segundo antes de atualizar o ping novamente
        }
    }

    // Definir que o jogador está pronto
    public void SetPronto(bool pronto) {
        if (isLocalPlayer) {
            CmdSetPronto(pronto);
        }
    }

    [Command]
    void CmdSetPronto(bool pronto) {
        DishNetworkManager manager = (DishNetworkManager)NetworkManager.singleton;
        manager.SetPronto(connectionToClient, pronto);
    }

    void UpdateSetProntoUI(bool oldValue, bool newValue) {
        if (isPlayerOne) ConnectionUI.instance.UpdateP1ProntoUI(newValue);
        else ConnectionUI.instance.UpdateP2ProntoUI(newValue);
    }



    // Trocar de personagem (Heater <-> Angler)
    public void TrocarPersonagens() {
        if (isLocalPlayer) {
            CmdTrocarPersonagens();
        }
    }

    [Command]
    void CmdTrocarPersonagens() {
        DishNetworkManager manager = (DishNetworkManager)NetworkManager.singleton;
        manager.TrocarPersonagens();
    }



    // Tentar começar o jogo (qualquer um pode fazer desde que os dois estejam prontos)
    public void TentarComecar() {
        if (isLocalPlayer) {
            CmdTentarComecar();
        }
    }

    [Command]
    void CmdTentarComecar() {
        DishNetworkManager manager = (DishNetworkManager)NetworkManager.singleton;
        manager.IniciarJogo();
    }



    // Trocar o nome do personagem
    public void TrocarNome(string nome) {
        if (isLocalPlayer) {
            CmdTrocarNome(nome);
        }
    }

    [Command]
    void CmdTrocarNome(string nome) {
        this.nome = nome;

        if (nome.Trim() == "") {
            pronto = false;
        }
    }

    public void SetPing(int ping) {
        if (isLocalPlayer && ping != this.ping) {
            CmdSetPing(ping);
        }
    }

    [Command]
    void CmdSetPing(int ping) {
        this.ping = ping;
    }



    void UpdatePersonagemUI(DishNetworkManager.Personagem oldValue, DishNetworkManager.Personagem newValue) {
        ConnectionUI.instance.UpdateNominhos();
    }

    void UpdateNomeUI(string oldValue, string newValue) {
        ConnectionUI.instance.UpdateNominhos();
    }

    void UpdatePingUI(int oldValue, int newValue) {
        ConnectionUI.instance.UpdatePingUI(newValue, personagem == DishNetworkManager.Personagem.Angler);
    }

    public void FoiDesconectado() {
        if (isPlayerOne) {
            ConnectionUI.instance.p1 = null;
            ConnectionUI.instance.UpdateP1ProntoUI(false);
        } else {
            ConnectionUI.instance.p2 = null;
            ConnectionUI.instance.UpdateP2ProntoUI(false);
        }

        ConnectionUI.instance.UpdateNominhos();
        ConnectionUI.instance.UpdateAguardandoJogadorUI();
    }
}

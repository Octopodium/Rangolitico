using UnityEngine;
using Mirror;

/// <summary>
/// Classe que representa um jogador no lobby (antes de entrar no jogo)
/// </summary>
public class LobbyPlayer : NetworkBehaviour {

    [SyncVar] public bool isPlayerOne = false;
    [SyncVar(hook = nameof(UpdateNomeUI))] public string nome;
    [SyncVar(hook = nameof(UpdatePersonagemUI))] public DishNetworkManager.Personagem personagem;
    [SyncVar(hook = nameof(UpdateSetProntoUI))] public bool pronto = false;

    void Start(){
        if (isPlayerOne) ConnectionUI.instance.p1 = this;
        else ConnectionUI.instance.p2 = this;

        ConnectionUI.instance.UpdateNominhos();
        ConnectionUI.instance.UpdateAguardandoJogadorUI();
    }

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

    void UpdatePersonagemUI(DishNetworkManager.Personagem oldValue, DishNetworkManager.Personagem newValue) {
        ConnectionUI.instance.UpdateNominhos();
    }

    void UpdateNomeUI(string oldValue, string newValue) {
        ConnectionUI.instance.UpdateNominhos();
    }

    public void FoiDesconectado() {
        if (isPlayerOne) ConnectionUI.instance.p1 = null;
        else ConnectionUI.instance.p2 = null;

        ConnectionUI.instance.UpdateNominhos();
        ConnectionUI.instance.UpdateAguardandoJogadorUI();
    }
}

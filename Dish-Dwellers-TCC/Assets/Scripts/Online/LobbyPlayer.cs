using UnityEngine;
using Mirror;

/// <summary>
/// Classe que representa um jogador no lobby (antes de entrar no jogo)
/// </summary>
public class LobbyPlayer : NetworkBehaviour {
    [SyncVar]
    public bool isPlayerOne = false;

    [SyncVar]
    public string nome;

    [SyncVar(hook = nameof(UpdateSetProntoUI))]
    public bool pronto = false;

    void Start(){
        Debug.Log("Eu nasci!");
    }

    public void SetPronto(bool pronto) {
        if (isLocalPlayer) {
            this.pronto = pronto;
        }
    }

    void UpdateSetProntoUI(bool oldValue, bool newValue) {
        if (isPlayerOne) ConnectionUI.instance.UpdateP1ProntoUI(newValue);
        else ConnectionUI.instance.UpdateP2ProntoUI(newValue);
    }
}

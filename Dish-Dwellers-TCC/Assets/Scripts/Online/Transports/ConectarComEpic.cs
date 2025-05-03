using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConectarComEpic : ConectorDeTransport {
    public Text mostrarID;
    public InputField idInput;

    public BetterEOSLobby beOSLobby;


    void Awake() {
        if (beOSLobby == null) beOSLobby = GetComponent<BetterEOSLobby>();
    }


    void Start() {
        beOSLobby.OnLobbyEncontrado += idLobby => mostrarID.text = "ID: " + idLobby;
        beOSLobby.OnLobbyCriado += idLobby => mostrarID.text = "ID: " + idLobby;
    }

    public override void Setup() {
        idInput.text = "";
        mostrarID.text = "";
    }

    public override void LogarUsuario(System.Action<bool> callback = null) {
        beOSLobby.OnLogou += () => {
            if (callback != null) callback.Invoke(true);
        };
    }

    public override void Hostear() {
        beOSLobby.CriarHost();
    }

    public override void ConectarCliente() {
        string id = idInput.text;
        beOSLobby.ConectarCliente(id);
    }

    public override void EncerrarHost() {
        beOSLobby.DesconectarHost();
    }

    public override void EncerrarCliente() {
        beOSLobby.DesconectarCliente();
    }


}

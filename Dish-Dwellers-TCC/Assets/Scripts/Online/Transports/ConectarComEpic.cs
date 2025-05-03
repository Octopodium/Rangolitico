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
        beOSLobby.OnLobbyCriado += idLobby => { 
            mostrarID.text = "ID: " + idLobby;
            callbackHostear?.Invoke(true);
            callbackHostear = null;
        };
        beOSLobby.OnCriarLobbyFalhou += () => { callbackHostear?.Invoke(false); callbackHostear = null; };

        beOSLobby.OnEntrouLobby += () => { callbackConectarCliente?.Invoke(true); callbackConectarCliente = null; };
        beOSLobby.OnEntrarLobbyFalhou += () => { callbackConectarCliente?.Invoke(false); callbackConectarCliente = null; };
    }

    public override void Setup() {
        idInput.text = "";
        mostrarID.text = "";
    }

    System.Action<bool> callbackHostear, callbackConectarCliente;
    public override void Hostear(System.Action<bool> callback = null) {
        callbackHostear = callback;

        beOSLobby.CriarHost();
    }

    public override void ConectarCliente(System.Action<bool> callback = null) {
        callbackConectarCliente = callback;

        string id = idInput.text.Trim().ToUpper();
        beOSLobby.ConectarCliente(id);
    }

    public override void EncerrarHost() {
        beOSLobby.DesconectarHost();
    }

    public override void EncerrarCliente() {
        beOSLobby.DesconectarCliente();
    }


}

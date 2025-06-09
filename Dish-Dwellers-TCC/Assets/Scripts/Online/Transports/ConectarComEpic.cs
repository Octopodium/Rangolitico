using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConectarComEpic : ConectorDeTransport {
    public Text mostrarID;
    public InputField idInput;

    private BetterEOSLobby _beOSLobby;
    public BetterEOSLobby beOSLobby {
        get {
            if (_beOSLobby == null) {
                _beOSLobby = FindFirstObjectByType<BetterEOSLobby>();
            }
            return _beOSLobby;
        }
    }


    public override void Setup() {
        idInput.text = "";
        mostrarID.text = "";
        mostrarID.gameObject.SetActive(true);

        beOSLobby.OnLobbyEncontrado += idLobby => mostrarID.text = "ID: " + idInput.text.Trim().ToUpper();
        beOSLobby.OnLobbyCriado += idLobby => { 
            mostrarID.text = "ID: " + idLobby;
            callbackHostear?.Invoke(true);
            callbackHostear = null;
        };
        beOSLobby.OnCriarLobbyFalhou += () => { callbackHostear?.Invoke(false); callbackHostear = null; };

        beOSLobby.OnEntrouLobby += () => { callbackConectarCliente?.Invoke(true); callbackConectarCliente = null; };
        beOSLobby.OnEntrarLobbyFalhou += () => { callbackConectarCliente?.Invoke(false); callbackConectarCliente = null; };
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

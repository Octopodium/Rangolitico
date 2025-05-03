using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ConectorOnlineIP : ConectorDeTransport {
    NetworkManager networkManager;
    TelepathyTransport telepathyTransport;


    public InputField ipInputField, portInputField;

    
    void Start() {
        networkManager = NetworkManager.singleton;
        telepathyTransport = (TelepathyTransport)networkManager.transport;
    }

    public override void LogarUsuario(System.Action<bool> callback = null) {
        if (callback != null) callback.Invoke(true);
    }

    public override void Setup() {
        ipInputField.text = networkManager.networkAddress;
        portInputField.text = telepathyTransport.port.ToString();
    }

    public override void Hostear() {
        networkManager.StartHost();
    }

    public override void ConectarCliente() {
        networkManager.networkAddress = ipInputField.text;
        telepathyTransport.port = ushort.Parse(portInputField.text);
        networkManager.StartClient();
    }

    public override void EncerrarHost() {
        networkManager.StopHost();
        networkManager.StopClient();
        networkManager.StopServer();
    }

    public override void EncerrarCliente() {
        networkManager.StopClient();
    }
}
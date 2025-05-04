using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ConectorOnlineIP : ConectorDeTransport {
    NetworkManager networkManager;
    TelepathyTransport telepathyTransport;


    public InputField ipInputField, portInputField;

    public override void Setup() {
        networkManager = NetworkManager.singleton;
        telepathyTransport = (TelepathyTransport)networkManager.transport;

        ipInputField.text = networkManager.networkAddress;
        portInputField.text = telepathyTransport.port.ToString();
    }

    public override void Hostear(System.Action<bool> callback = null) {
        networkManager.StartHost();
        callback?.Invoke(true);
    }

    public override void ConectarCliente(System.Action<bool> callback = null) {
        networkManager.networkAddress = ipInputField.text;
        telepathyTransport.port = ushort.Parse(portInputField.text);
        networkManager.StartClient();
        callback?.Invoke(true);
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
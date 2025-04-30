using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Auth;

[RequireComponent(typeof(EOSLobby))]
public class ConectarComEpic : ConectorDeTransport {
    NetworkManager networkManager;
    public EOSLobby eOSLobby;

    public Text mostrarID;
    public InputField idInput;

    void Start() {
        networkManager = NetworkManager.singleton;
        eOSLobby = GetComponent<EOSLobby>();

        // eOSLobby.CreateLobbySucceeded += OnHostearSucess;
        eOSLobby.CreateLobbyFailed += (error) => Debug.Log("Falha ao criar lobby: " + error);
    }


    public override void Setup() {
        idInput.text = eOSLobby.GetCurrentLobbyId();
        mostrarID.text = eOSLobby.GetCurrentLobbyId();
    }

    public override void Hostear() {
        // networkManager.StartHost();
        // eOSLobby.CreateLobby(2, LobbyPermissionLevel.Publicadvertised, true);
        // Debug.Log("Tentando criar lobby...");
    }
/*
    void OnHostearSucess(List<Attribute> attributes) {
        Debug.Log("Lobby criado com sucesso: " + eOSLobby.GetCurrentLobbyId() + " e" + attributes.Count + " atributos. São eles: " + attributes);
        mostrarID.text = eOSLobby.GetCurrentLobbyId();
    }
*/
    public override void ConectarCliente() {
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

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Auth;
using EpicTransport;

[RequireComponent(typeof(EOSLobby))]
public class ConectarComEpic : ConectorDeTransport {
    NetworkManager networkManager;
    public EOSLobby eOSLobby;
    public EOSSDKComponent eossdkComponent;

    public Text mostrarID;
    public InputField idInput;

    bool vaiHostear = false, logado = false;

    void Awake() {
        networkManager = NetworkManager.singleton;
        eOSLobby = GetComponent<EOSLobby>();
        eossdkComponent.OnCustomEventLoggedIn += AoEntrar;
    }

    void Start() {
        // eOSLobby.CreateLobbySucceeded += OnHostearSucess;
        eOSLobby.CreateLobbyFailed += (error) => Debug.Log("Falha ao criar lobby: " + error);
        Debug.Log("?????");
    }

    void AoEntrar(string id) {
        Debug.Log("eeeee: " + id);
        logado = true;
        if (vaiHostear) Hostear();
        vaiHostear = false;
    }


    public override void Setup() {
        idInput.text = eOSLobby.GetCurrentLobbyId();
        mostrarID.text = eOSLobby.GetCurrentLobbyId();
    }

    public override void Hostear() {
        if (!logado) {
            vaiHostear = true;
            return;
        }


        if (networkManager == null) networkManager = NetworkManager.singleton;
        networkManager.StartHost();
        eOSLobby.CreateLobby(2, 0, true);
        Debug.Log("Tentando criar lobby...");
    }
/*
    void OnHostearSucess(List<Attribute> attributes) {
        Debug.Log("Lobby criado com sucesso: " + eOSLobby.GetCurrentLobbyId() + " e" + attributes.Count + " atributos. São eles: " + attributes);
        mostrarID.text = eOSLobby.GetCurrentLobbyId();
    }
*/
    public override void ConectarCliente() {
        string id = idInput.text;
        eOSLobby.JoinLobbyByID(id);
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

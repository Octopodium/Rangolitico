using EpicTransport;
using Epic.OnlineServices.Lobby;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

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

        eOSLobby.FindLobbiesSucceeded += (foundLobbies) => {
            Debug.Log(foundLobbies.Count + " lobbies encontrados.");
            string idCorreto = "";

            foreach (var lobby in foundLobbies) {
                string id = eOSLobby.GetIdFromInfo(lobby);
                if (id != null && id != "") {
                    idCorreto = id;
                    break;
                }
            }

            Debug.Log("ID encontrado: " + idCorreto);
            eOSLobby.JoinLobbyByID(idCorreto);
        };

        eOSLobby.CreateLobbySucceeded += (lobbyAttrs) => {
            Debug.Log("Lobby criado com sucesso [" + eOSLobby.GetCurrentLobbyId() + "]");
        };

        eOSLobby.JoinLobbySucceeded += (lobbyAttrs) => {
            Debug.Log("Entrou no lobby [" + eOSLobby.GetCurrentLobbyId() + "]");
            networkManager.StartClient();
        };
    }

    string GerarID() {
        int tamanho = 5;
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[tamanho];
        System.Random random = new System.Random();
        for (int i = 0; i < tamanho; i++) {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

    void AoEntrar(string id) {
        logado = true;
        if (vaiHostear) Hostear();

        vaiHostear = false;
    }


    public override void Setup() {
        idInput.text = "";
        mostrarID.text = "";
    }
    string globalID = "";
    public override void Hostear() {
        if (!logado) {
            vaiHostear = true;
            return;
        }


        if (networkManager == null) networkManager = NetworkManager.singleton;
        networkManager.StartHost();

        string id = GerarID();
        globalID = id;
        mostrarID.text = "ID: " + id;
        Debug.Log("ID gerado: " + id);
        eOSLobby.CreateLobby(2, 0, true, eOSLobby.CriarAtributos(id));
    }

    public override void ConectarCliente() {
        string id = idInput.text;
        eOSLobby.FindLobbies(1, eOSLobby.CriarPesquisa(id));
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

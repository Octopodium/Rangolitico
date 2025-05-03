using EpicTransport;
using Epic.OnlineServices.Lobby;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

// OBS: Por enquanto para rodar coisas do multiplayer é necessário ter um arquivo na pasta root do projeto (Dish-Dwellers-TCC)
// Esse arquivo pode ser baixado no link: https://github.com/EOS-Contrib/eos_plugin_for_unity/releases/download/v4.0.0/com.playeveryware.eos-4.0.0.tgz 
// Favor não renomear, e não mover o arquivo, pois o plugin não vai funcionar corretamente.

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

    string idHost = null;

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
                    idHost = eOSLobby.GetHostIDFromInfo(lobby);
                    break;
                }
            }

            Debug.Log("ID encontrado: " + idCorreto);
            Debug.Log("Host ID: " + idHost);
            eOSLobby.JoinLobbyByID(idCorreto);
        };

        eOSLobby.CreateLobbySucceeded += (lobbyAttrs) => {
            Debug.Log(lobbyAttrs + " " + lobbyAttrs.GetType());
            Debug.Log("Lobby criado com sucesso [" + eOSLobby.GetCurrentLobbyId() + "]");

            // networkManager.networkAddress = eOSLobby.GetCurrentLobbyId();
            networkManager.StartHost();
        };

        eOSLobby.JoinLobbySucceeded += (lobbyAttrs) => {
            Debug.Log("Entrou no lobby [" + eOSLobby.GetCurrentLobbyId() + "]");
            networkManager.networkAddress = idHost;
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

    public override void Hostear() {
        if (!logado) {
            vaiHostear = true;
            return;
        }


        if (networkManager == null) networkManager = NetworkManager.singleton;
        

        string id = GerarID();
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

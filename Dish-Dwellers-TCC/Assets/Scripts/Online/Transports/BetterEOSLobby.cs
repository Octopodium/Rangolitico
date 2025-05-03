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
public class BetterEOSLobby : MonoBehaviour {
    // OBS: Além deste script, eu também fiz alterações no EOSLobby.cs, para que o mesmo funcione corretamente.
    // Se você copiar este código, sugiro fortemente que copie também o EOSLobby.cs, para evitar bugs.
    // EOSLobby.cs está na pasta: Assets/Mirror/Transports/EOSTransport/Lobby/EOSLobby.cs
    // Nota: você vai precisar de acesso ao SDK da Epic de alguma forma, seja importanto diretamente ou com o plugin do EOS.

    // Referências
    NetworkManager networkManager;
    public EOSLobby eOSLobby;
    public EOSSDKComponent eossdkComponent;


    // Variáveis internas
    bool estaLogado = false, pediuPraHostear = false, pediuPraEntrar = false;
    string tentarEntrarNoLobby_Cache = ""; // Quando o jogador tentou entrar em um lobby, mas não estava logado, esse cache vai armazenar o ID do lobby que ele queria entrar.
    string idHost = "", idLobby = "";


    // Eventos
    public System.Action OnLogou;
    public System.Action OnEntrouLobby, OnEntrarLobbyFalhou;
    public System.Action<string> OnLobbyCriado, OnLobbyEncontrado;
    public System.Action OnCriarLobbyFalhou;


    void Awake() {
        networkManager = NetworkManager.singleton;

        if (eOSLobby == null) eOSLobby = GetComponent<EOSLobby>();
        if (eossdkComponent == null) eossdkComponent = GetComponent<EOSSDKComponent>();

        eossdkComponent.OnCustomEventLoggedIn += AoLogar;
    }

    void Start() {
        // Quando um lobby é criado com sucesso, iniciar o host
        eOSLobby.CreateLobbySucceeded += (lobbyAttrs) => {
            Debug.Log("Lobby criado com sucesso: " + idLobby);
            OnLobbyCriado?.Invoke(idLobby);
            networkManager.StartHost();
        };

        // Na falha ao criar o lobby, exibir mensagem de erro
        eOSLobby.CreateLobbyFailed += (error) => {
            OnCriarLobbyFalhou?.Invoke();
            Debug.LogError("Falha ao criar lobby: " + error);
        };

        // Ao encontrar um lobby baseado no ID
        eOSLobby.FindLobbiesSucceeded += (foundLobbies) => {
            string idCorreto = "";

            foreach (var lobby in foundLobbies) {
                string id = eOSLobby.GetIdFromInfo(lobby);
                if (id != null && id != "") {
                    idCorreto = id;
                    idHost = eOSLobby.GetHostIDFromInfo(lobby);
                    break;
                }
            }

            if (idCorreto == "") {
                Debug.LogError("Nenhum lobby encontrado com o ID fornecido.");
                OnEntrarLobbyFalhou?.Invoke();
                return;
            }

            OnLobbyEncontrado?.Invoke(idHost);

            Debug.Log("Host ID: " + idHost);
            eOSLobby.JoinLobbyByID(idCorreto);
        };

        // Ao entra em um lobby com sucesso, iniciar o cliente
        eOSLobby.JoinLobbySucceeded += (lobbyAttrs) => {
            OnEntrouLobby?.Invoke();
            networkManager.networkAddress = idHost;
            networkManager.StartClient();
        };

        eOSLobby.JoinLobbyFailed += (error) => {
            OnEntrarLobbyFalhou?.Invoke();
            Debug.LogError("Falha ao entrar no lobby: " + error);
        };
    }

    void AoLogar(string id)  {
        estaLogado = true;

        if (pediuPraHostear) CriarHost();
        if (pediuPraEntrar) ConectarCliente(tentarEntrarNoLobby_Cache);

        pediuPraHostear = false;
        pediuPraEntrar = false;
        tentarEntrarNoLobby_Cache = "";
    }

    public void CriarHost() {
        if (!estaLogado) {
            pediuPraHostear = true;
            return;
        }

        if (networkManager == null) networkManager = NetworkManager.singleton;

        idLobby = GerarID();
        eOSLobby.CreateLobby(2, 0, true, eOSLobby.CriarAtributos(idLobby));
    }

    public void ConectarCliente(string id) {
        if (!estaLogado) {
            pediuPraEntrar = true;
            return;
        }

        eOSLobby.FindLobbies(1, eOSLobby.CriarPesquisa(id));
    }

    public void DesconectarHost() {
        networkManager.StopHost();
        networkManager.StopClient();
        networkManager.StopServer();
    }

    public void DesconectarCliente() {
        networkManager.StopClient();
    }

    // Gera um ID aleatório de 5 caracteres alfanuméricos
    // E sim, eu não estou checando se esse ID já existe, mas eu devia...
    protected string GerarID() {
        int tamanho = 5;
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[tamanho];
        System.Random random = new System.Random();
        for (int i = 0; i < tamanho; i++) {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }
}

using EpicTransport;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Connect;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Mirror;


[RequireComponent(typeof(EOSLobby))]
public class BetterEOSLobby : MonoBehaviour {
    // OBS: Além deste script, eu também fiz uma pequena adição no EOSSKDComponent.cs, para que o mesmo funcione corretamente.
    // Se você copiar este código, para quaisquer que sejam seus motivos, você deve fazer uma das duas opções:
    // 1- Substituir o EOSSKDComponent pelo alterado, que está na pasta: Assets/Mirror/Transports/EOSTransport/EOSSKDComponent.cs
    // 2- Manualmente adicionar a função OnCustomEventLoggedIn, que é chamada quando o jogador loga no EOS, e que chama a função AoLogar() deste script.
    // 2.1 - Para isso, adicione a seguinte linha no EOSSDKComponent.cs, logo em cima da declaração da função OnConnectInterfaceLogin: (neste projeto está na linha 415)
    //          public System.Action<string> OnCustomEventLoggedIn;
    // 2.2 - E adicione a seguinte linha dentro do if que indica que o jogador logou com sucesso, na função OnConnectInterfaceLogin: (neste projeto está na linha 428)
    //          OnCustomEventLoggedIn?.Invoke(productIdString);

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

        /*
        AddNotifyLoginStatusChangedOptions options = new AddNotifyLoginStatusChangedOptions();
        object dado = new object();
        EOSSDKComponent.GetConnectInterface().AddNotifyLoginStatusChanged(ref options, dado, (ref LoginStatusChangedCallbackInfo info) => {
            if (info.CurrentStatus == Epic.OnlineServices.LoginStatus.LoggedIn) {
                AoLogar(info.LocalUserId.ToString());
            }
        });
        */
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
                string id = GetIdFromInfo(lobby);
                if (id != null && id != "") {
                    idCorreto = id;
                    idHost = GetHostIDFromInfo(lobby);
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

        OnLogou?.Invoke();

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
        eOSLobby.CreateLobby(2, 0, true, CriarAtributos(idLobby));
    }

    public void ConectarCliente(string id) {
        if (!estaLogado) {
            pediuPraEntrar = true;
            return;
        }

        eOSLobby.FindLobbies(1, CriarPesquisa(id));
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


    #region Utilidades

    // Função customizada para criar um atributo para o lobby
    AttributeData[] CriarAtributos(string id) {
        AttributeData[] attributes = new AttributeData[1];
        attributes[0] = new AttributeData { Key = "id_jogo", Value = id };
        return attributes;
    }

    LobbySearchSetParameterOptions[] CriarPesquisa(string id) {
        LobbySearchSetParameterOptions[] options = new LobbySearchSetParameterOptions[1];
        options[0] = new LobbySearchSetParameterOptions {
            ComparisonOp = Epic.OnlineServices.ComparisonOp.Equal,
            Parameter = new AttributeData { Key = "id_jogo", Value = id }
        };
        return options;
    }

    string GetIdFromInfo(LobbyDetails details) {
        LobbyDetailsCopyInfoOptions options;
        LobbyDetailsInfo? outLobbyDetailsInfo = null;

        details.CopyInfo(ref options, out outLobbyDetailsInfo);

        Debug.Log("ID do Lobby: " + outLobbyDetailsInfo?.LobbyId);
        return outLobbyDetailsInfo?.LobbyId;
    }

    string GetHostIDFromInfo(LobbyDetails details) {
        LobbyDetailsCopyInfoOptions options;
        LobbyDetailsInfo? outLobbyDetailsInfo = null;

        details.CopyInfo(ref options, out outLobbyDetailsInfo);

        Debug.Log("ID do Host: " + outLobbyDetailsInfo?.LobbyOwnerUserId);
        return outLobbyDetailsInfo?.LobbyOwnerUserId.ToString();
    }

    #endregion
}

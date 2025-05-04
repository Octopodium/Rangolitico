using EpicTransport;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Connect;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Mirror;

// Este código precisa que o projeto tenha o EOSTransport, disponibilizado no GitHub: https://github.com/WeLoveJesusChrist/EOSTransport
// Assim, para que o código funcione, você irá precisar:
// 1. Ter instalado o EOSTransport no projeto (obs: ele já vem com o EOSSDK, não precisa instalar separado)
// 2. Ter o EOSTransport conectado ao NetworkManager, assim como o EOSSDKComponent incluso na cena (recomendo até que seja junto do NetworkManager)
// 3. Esse script foi feito com a seguinte configuração do EOSSDKComponent em mente: (há um prefab do Manager que utilizamos: Assets/Prefabs/Online/NetworkManagerEpic.prefab)
//    (Esta configuração permite utilizar a Epic sem precisar de um login, apenas com o DeviceID, que é único para cada dispositivo.)
//      - AuthInterfaceLogin: false
//      - AuthInterfaceCredentialType: Account Portal
//      - ConnectInterfaceCredentialType: Deviceid Access Token
// 3. Ter o EOSLobby incluso na cena, junto com este componente (para não ficar muito poluído, recomendo que estes estejam num GameObject filho do NetworkManager)
// 4. Alterar o código de iniciar Host/Cliente para os definidos neste componente.

[RequireComponent(typeof(EOSLobby))]
public class BetterEOSLobby : MonoBehaviour {
    // Referências
    NetworkManager networkManager;
    public EOSLobby eOSLobby;
    public EOSSDKComponent eossdkComponent;

    public enum Estado {  Nenhum, CriandoLobby, EntrandoLobby }
    public Estado estado = Estado.Nenhum;


    // Variáveis internas
    bool estaLogado = false, pediuPraHostear = false, pediuPraEntrar = false;
    string tentarEntrarNoLobby_Cache = ""; // Quando o jogador tentou entrar em um lobby, mas não estava logado, esse cache vai armazenar o ID do lobby que ele queria entrar.
    string idHost = "", idLobby = "";

    public interface ILobbyIDCreator {
        string CreateID();
        string FormatID(string id);
    }
    public class AutoIDConfig: ILobbyIDCreator {
        public int quantCaracteres = 5; // Quantidade de caracteres do ID (indicado não ser menor que 5)
        public bool usarMaiusculas = true; // Se o ID pode ter letras maiúsculas
        public bool usarMinusculas = false; // Se o ID pode ter letras minúsculas
        public bool usarNumeros = true; // Se o ID pode ter números

        public AutoIDConfig(int quantCaracteres = 5, bool usarMaiusculas = true, bool usarMinusculas = false, bool usarNumeros = true) {
            this.quantCaracteres = quantCaracteres;
            this.usarMaiusculas = usarMaiusculas;
            this.usarMinusculas = usarMinusculas;
            this.usarNumeros = usarNumeros;
        }

        public string CreateID() {
            string chars = "";
            if (usarMaiusculas) chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (usarMinusculas) chars += "abcdefghijklmnopqrstuvwxyz";
            if (usarNumeros) chars += "0123456789";

            char[] stringChars = new char[quantCaracteres];
            System.Random random = new System.Random();
            for (int i = 0; i < quantCaracteres; i++) {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        public string FormatID(string id) {
            id = id.Trim(); // Remove espaços em branco no início e no final do ID
            if (usarMaiusculas && !usarMinusculas) return id.ToUpper();
            if (usarMinusculas && !usarMaiusculas) return id.ToLower();
            return id;
        }
    }


    public struct Configuracao {
        public uint maxPlayers;
        public LobbyPermissionLevel permissionLevel;  // Publicadvertised [0]: publico p/ qualquer um, Joinviapresence[1]: entrar via presença, Inviteonly[2]: só entra quem foi convidado (ver mais na documentação do EOS SDK)
        public bool presenceEnabled;
        public AttributeData[] attributes;
        public ILobbyIDCreator definirLobbyID; // Classe que monta o ID automaticamente. Se nulo, o ID é deixado por conta do programador.

        public Configuracao(uint maxPlayers, LobbyPermissionLevel permissionLevel, bool presenceEnabled, AttributeData[] attributes = null, ILobbyIDCreator definirLobbyID = null) {
            this.maxPlayers = maxPlayers;
            this.permissionLevel = permissionLevel;
            this.presenceEnabled = presenceEnabled;
            this.attributes = attributes;
            this.definirLobbyID = definirLobbyID;
        }
    }
    Configuracao configuracaoPadrao = new Configuracao {
        maxPlayers = 2,
        permissionLevel = LobbyPermissionLevel.Publicadvertised,
        presenceEnabled = true,
        attributes = null,
        definirLobbyID = new AutoIDConfig()
    };


    // Eventos
    public System.Action OnLogou;
    public System.Action OnEntrouLobby, OnEntrarLobbyFalhou;
    public System.Action<string> OnLobbyCriado, OnLobbyEncontrado;
    public System.Action OnCriarLobbyFalhou;


    void Awake() {
        networkManager = NetworkManager.singleton;

        if (eOSLobby == null) eOSLobby = GetComponent<EOSLobby>();
        if (eossdkComponent == null) eossdkComponent = GetComponent<EOSSDKComponent>();

        StartCoroutine(EsperandoDeviceID());
    }

    void Start() {
        // Quando um lobby é criado com sucesso, iniciar o host
        eOSLobby.CreateLobbySucceeded += (lobbyAttrs) => {
            Debug.Log("Lobby criado com sucesso: " + idLobby);
            estado = Estado.Nenhum;
            OnLobbyCriado?.Invoke(idLobby);
            networkManager.StartHost();
        };

        // Na falha ao criar o lobby, exibir mensagem de erro
        eOSLobby.CreateLobbyFailed += (error) => {
            estado = Estado.Nenhum;
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

            bool lobbyEncontrado = idCorreto != "";

            if (lobbyEncontrado) {
                OnLobbyEncontrado?.Invoke(idHost);
                Debug.Log("Host ID: " + idHost);
                eOSLobby.JoinLobbyByID(idCorreto);
            } else {
                Debug.LogError("Nenhum lobby encontrado com o ID fornecido.");
                OnEntrarLobbyFalhou?.Invoke();
            }
        };

        // Ao entra em um lobby com sucesso, iniciar o cliente
        eOSLobby.JoinLobbySucceeded += (lobbyAttrs) => {
            estado = Estado.Nenhum;
            OnEntrouLobby?.Invoke();
            networkManager.networkAddress = idHost;
            networkManager.StartClient();
        };

        eOSLobby.JoinLobbyFailed += (error) => {
            estado = Estado.Nenhum;
            OnEntrarLobbyFalhou?.Invoke();
            Debug.LogError("Falha ao entrar no lobby: " + error);
        };
     
    }

    IEnumerator EsperandoDeviceID() {
        while (!EOSSDKComponent.Initialized) {
            yield return null;
        }

        AoLogar(EOSSDKComponent.LocalUserProductId.ToString());
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

    Configuracao? configuracaoLobbyCache = null;
    public void CriarHost(Configuracao? configuracao = null) {
        configuracaoLobbyCache = configuracao;
    
        if (!estaLogado) {
            pediuPraHostear = true;
            return;
        }

        if (networkManager == null) networkManager = NetworkManager.singleton;


        estado = Estado.CriandoLobby;

        Configuracao configuracaoAtual = configuracaoLobbyCache ?? configuracaoPadrao;
        configuracaoLobbyCache = null;
        AttributeData[] atributos = SetarIdDoLobby(configuracaoAtual.definirLobbyID, configuracaoAtual.attributes);
        
        eOSLobby.CreateLobby(configuracaoAtual.maxPlayers, configuracaoAtual.permissionLevel, configuracaoAtual.presenceEnabled, atributos);
    }

    public void ConectarCliente(string id, Configuracao? configuracao = null) {
        configuracaoLobbyCache = configuracao;

        if (!estaLogado) {
            pediuPraEntrar = true;
            tentarEntrarNoLobby_Cache = id;
            return;
        }

        Configuracao configuracaoAtual = configuracaoLobbyCache ?? configuracaoPadrao;
        configuracaoLobbyCache = null;

        string idFormatado = configuracaoAtual.definirLobbyID?.FormatID(id) ?? id;

        estado = Estado.EntrandoLobby;
        eOSLobby.FindLobbies(1, CriarPesquisa(idFormatado));
    }

    public void DesconectarHost() {
        networkManager.StopHost();
        networkManager.StopClient();
        networkManager.StopServer();
    }

    public void DesconectarCliente() {
        networkManager.StopClient();
    }


    #region Utilidades

    // Função customizada para criar um atributo para o lobby
    AttributeData[] CriarAtributos(string id) {
        AttributeData[] attributes = new AttributeData[1];
        attributes[0] = new AttributeData { Key = "id_jogo", Value = id };
        return attributes;
    }

    AttributeData[] CriarAtributos(string id, AttributeData[] atributos) {
        List<AttributeData> atributosList = new List<AttributeData>(atributos);

        foreach (var atributo in atributos) {
            if (atributo.Key == "id_jogo") {
                atributosList.Remove(atributo);
                break;
            }
        }

        AttributeData novoAtributo = new AttributeData { Key = "id_jogo", Value = id };
        atributosList.Add(novoAtributo);

        return atributosList.ToArray();
    }

    AttributeData[] SetarIdDoLobby(ILobbyIDCreator definirLobby, AttributeData[] atributos) {
        bool temDefinidor = definirLobby != null;

        if (atributos == null || atributos.Length == 0) {
            if (temDefinidor) {
                idLobby = definirLobby.CreateID();
                return CriarAtributos(idLobby);
            }
            return null;
        }

        // Vê se já tem o atributo "id_jogo"
        for (int i = 0; i < atributos.Length; i++) {
            var atributo = atributos[i];

            if (atributo.Key == "id_jogo") {
                if (!temDefinidor) {
                    idLobby = atributo.Value.ToString();
                    return atributos;
                } else {
                    idLobby = definirLobby.CreateID();
                    atributos[i].Value = idLobby;
                    return atributos;
                } 
            }
        }
        
        if (temDefinidor) {
            idLobby = definirLobby.CreateID();
            return CriarAtributos(idLobby, atributos);
        }
        return null;
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

        return outLobbyDetailsInfo?.LobbyOwnerUserId.ToString();
    }

    #endregion
}

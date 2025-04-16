using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class ConnectionUI : MonoBehaviour {
    public static ConnectionUI instance;

    public string menuInicialScene = "MenuInicial";

    public DishNetworkManager networkManager;
    [HideInInspector] public LobbyPlayer p1, p2;


    [Header("UI")]
    public GameObject esperandoJogador;
    public GameObject tentandoConectar;
    public InputField anglerInput, heaterInput;
    public GameObject anglerReady, heaterReady;
    public Transform logsHolder;

    


    void Awake() {
        instance = this;
    }

    void Start() {
        networkManager = (DishNetworkManager)NetworkManager.singleton;
        UpdateNominhos();

        if (PartidaInfo.instance != null && PartidaInfo.instance.modo == PartidaInfo.Modo.Entrar) {
            PrepararPraEntrarLobby();
        } else {
            ComecarHostear();
        }
    }



    #region Lobby 

    // Pede pro servidor trocar os personagens (ao apertar o botão de trocar personagens)
    public void TrocarPersonagens() {
        UpdateP1ProntoUI(false);
        UpdateP2ProntoUI(false);

        if (p1.isLocalPlayer) p1.TrocarPersonagens();
        else p2.TrocarPersonagens();
    }


    // Atualiza a informação de nome do jogador (quando o jogador muda o nome no input)
    public void UpdateNominhos(LobbyPlayer lobbyPlayer) {
        UpdateNominhos();
    }

    // Quando o servidor trocou os personagens
    public void UpdateNominhos() {
        if (networkManager.lobbyPlayers == null) return;

        InputField inputP1 = null;
        InputField inputP2 = null;

        if ((p1 != null && p1.personagem == DishNetworkManager.Personagem.Angler) || (p2 != null && p2.personagem == DishNetworkManager.Personagem.Heater)) {
            inputP1 = anglerInput;
            inputP2 = heaterInput;
        } else {
            inputP1 = heaterInput;
            inputP2 = anglerInput;
        }

        inputP1.text = (p1 != null) ? p1.nome : "???";
        inputP2.text = (p2 != null) ? p2.nome : "???";

        if (p1 != null && p1.isLocalPlayer) {
            inputP1.interactable = true;
            inputP2.interactable = false;
        } else if (p2 != null && p2.isLocalPlayer) {
            inputP1.interactable = false;
            inputP2.interactable = true;
        } else {
            inputP1.interactable = false;
            inputP2.interactable = false;
        }
    }


    // Chamado quando o nome no input do jogador atual é alterado
    public void OnNomeChange() {
        // A função não recebe o input como parâmetro para garantir que só altere o nome do jogador atual
        LobbyPlayer jogadorAtual = (p1 != null && p1.isLocalPlayer) ? p1 : ((p2 != null && p2.isLocalPlayer) ? p2 : null);
        if (jogadorAtual == null) return;


        InputField input = jogadorAtual.personagem == DishNetworkManager.Personagem.Angler ? anglerInput : heaterInput;
        string nome = input.text;
        jogadorAtual.TrocarNome(nome);
    }


    // Chamado quando o cliente clica no botão de começar
    public void SetPronto() {
        SetComecar();

        LobbyPlayer lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        lobbyPlayer.SetPronto(!lobbyPlayer.pronto);
    }


    // Atualiza a UI a partir do valor do LobbyPlayer 1
    public void UpdateP1ProntoUI(bool val) {
        if (p1 == null) return;

        if (p1.personagem == DishNetworkManager.Personagem.Heater) heaterReady.SetActive(val);
        else anglerReady.SetActive(val);

        if (p1.isLocalPlayer) {
            //prontoButton.GetComponentInChildren<Text>().text = (val) ? "Cancelar" : "Pronto";
        }
    }


    // Atualiza a UI a partir do valor do LobbyPlayer 2
    public void UpdateP2ProntoUI(bool val) {
        if (p2 == null) return;

        if (p2.personagem == DishNetworkManager.Personagem.Angler) anglerReady.SetActive(val);
        else heaterReady.SetActive(val);

        if (p2.isLocalPlayer) {
            //prontoButton.GetComponentInChildren<Text>().text = (val) ? "Cancelar" : "Pronto";
        }
    }


    // Atualiza informações de espera de jogador
    public void UpdateAguardandoJogadorUI() {
        esperandoJogador.SetActive(p1 == null || p2 == null);
    }


    // Chamado quando um jogador tenta começar o jogo
    public void SetComecar() {
        if (p1 == null || p2 == null) return;

        if (p1.isLocalPlayer) p1.TentarComecar();
        else p2.TentarComecar();
    }

    #endregion


    #region Entrar em Lobby

    [Header("Entrar em Lobby")]
    public GameObject entrarLobbyPanel;
    public InputField ipInputField, portInputField;

    
    // Chamado quando o cliente tenta criar um lobby
    public void ComecarHostear() {
        entrarLobbyPanel.SetActive(false);
        networkManager.StartHost();
    }

    // Mostra modal para entrar em um lobby já criado
    public void PrepararPraEntrarLobby() {
        tentandoConectar.SetActive(false);
        entrarLobbyPanel.SetActive(true);
        ipInputField.text = networkManager.networkAddress;

        TelepathyTransport telepathyTransport = networkManager.gameObject.GetComponent<TelepathyTransport>();
        if (telepathyTransport != null) {
            portInputField.text = telepathyTransport.port.ToString();
        }
    }

    // Chamado quando um cliente tenta entrar em um lobby já criado
    public void EntrarNoLobby() {
        entrarLobbyPanel.SetActive(false);
        tentandoConectar.SetActive(true);

        networkManager.networkAddress = ipInputField.text;
        TelepathyTransport telepathyTransport = networkManager.gameObject.GetComponent<TelepathyTransport>();
        if (telepathyTransport != null) {
            telepathyTransport.port = ushort.Parse(portInputField.text);
        }

        networkManager.StartClient();
    }

    // Chamado quando um cliente entra no lobby com sucesso (pelo LobbyPlayer)
    public void EntrouNoLobby() {
        tentandoConectar.SetActive(false);
    }

    public void CancelarEntrada() {
        tentandoConectar.SetActive(false);
        entrarLobbyPanel.SetActive(true);
        networkManager.StopClient();
    }

    #endregion


    public void SairDoLobby() {
        networkManager.StopHost();
        networkManager.StopClient();
        networkManager.StopServer();

        Destroy(networkManager.gameObject);
        if (GameManager.instance != null)  Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene(menuInicialScene, LoadSceneMode.Single);
    }
}

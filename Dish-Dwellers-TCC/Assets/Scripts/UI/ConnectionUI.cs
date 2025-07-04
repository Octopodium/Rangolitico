using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public enum TipoDeTransport { IP, Epic }

public class ConnectionUI : MonoBehaviour {
    public static ConnectionUI instance;

    public string menuInicialScene = "MenuInicial";
    public DishNetworkManager networkManager;

    public Transform canvasDaConexao;
    public EventSystem eventSystem;


    [Header("Configuração de Conexão")]
    public TipoDeTransport tipoDeTransport = TipoDeTransport.IP;
    public GameObject ipPrefab, epicPrefab;
    public GameObject ipPanel, epicPanel;
    public Button ipButton, epicButton;
    public Button cancelarConexaoButton;
    public InputField ipField, epicField;

    [HideInInspector] public LobbyPlayer p1, p2;


    [Header("UI")]
    public GameObject esperandoJogador;
    public GameObject telaCarregamento;
    public Text telaCarregamentoTexto;
    public InputField anglerInput, heaterInput;
    public Text pingAngler, pingHeater;
    public GameObject anglerReady, heaterReady;
    public Transform logsHolder;
    public Button trocarPersonagensButton, voltarButton;
    
    ConectorDeTransport conectorDeTransport;

    void Awake() {
        instance = this;

        GameObject prefab = null;
        switch (tipoDeTransport) {
            case TipoDeTransport.IP:
                ipPanel.SetActive(true);
                epicPanel.SetActive(false);
                prefab = ipPrefab;
                conectorDeTransport = GetComponent<ConectorOnlineIP>();
                SelecionarBotao(ipButton);
                break;
            case TipoDeTransport.Epic:
                ipPanel.SetActive(false);
                epicPanel.SetActive(true);
                prefab = epicPrefab;
                conectorDeTransport = GetComponent<ConectarComEpic>();
                SelecionarBotao(epicButton);
                break;
        }

        GameObject managerInstancia = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        networkManager = managerInstancia.GetComponent<DishNetworkManager>();
    }

    void Start() {
        conectorDeTransport.Setup();

        UpdateNominhos();


        if (PartidaInfo.instance != null && PartidaInfo.instance.modo == PartidaInfo.Modo.Entrar) {
            PrepararPraEntrarLobby();
        } else {
            ComecarHostear();
        }
    }

    public void SelecionarBotao(Button botao) {
        eventSystem.SetSelectedGameObject(botao.gameObject);
        SetarNavigation();
    }

    public void SetarNavigation () {
        Navigation navigation = voltarButton.navigation;
        navigation.mode = Navigation.Mode.Explicit;

        bool taCarregando = telaCarregamento.activeSelf;
        if (taCarregando) {
            navigation.selectOnRight = cancelarConexaoButton;
            navigation.selectOnDown = cancelarConexaoButton;
            voltarButton.navigation = navigation;
            return;
        }

        if (tipoDeTransport == TipoDeTransport.IP && ipPanel.activeSelf) {
            navigation.selectOnRight = ipField;
            navigation.selectOnDown = ipField;
        } else if (tipoDeTransport == TipoDeTransport.IP && epicPanel.activeSelf) {
            navigation.selectOnRight = epicField;
            navigation.selectOnDown = epicField;
        } else {
            navigation.selectOnRight = trocarPersonagensButton;
            navigation.selectOnDown = trocarPersonagensButton;
        }

        voltarButton.navigation = navigation;
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

        if (p1 != null) UpdatePingUI(p1.ping, p1.personagem == DishNetworkManager.Personagem.Angler);
        if (p2 != null) UpdatePingUI(p2.ping, p2.personagem == DishNetworkManager.Personagem.Angler);

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

    public void UpdateProntoUI() {
        if (p1 != null) UpdateP1ProntoUI(p1.pronto);
        if (p2 != null) UpdateP2ProntoUI(p2.pronto);
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

    public void UpdatePingUI(int ping, bool isAngler) {
        if (isAngler) pingAngler.text = ping.ToString() + "ms";
        else pingHeater.text = ping.ToString() + "ms";
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

    void OnDestroy() {
        if (canvasDaConexao != null && canvasDaConexao.gameObject != null)
            Destroy(canvasDaConexao.gameObject);
    }

    #endregion


    #region Entrar em Lobby

    [Header("Entrar em Lobby")]
    public GameObject entrarLobbyPanel;
    
    
    // Chamado quando o cliente tenta criar um lobby
    public void ComecarHostear() {
        entrarLobbyPanel.SetActive(false);

        MostrarCarregamento("Criando lobby...", SairDoLobby);
        conectorDeTransport.Hostear(status => EsconderCarregamento());
    }

    // Mostra modal para entrar em um lobby já criado
    public void PrepararPraEntrarLobby() {
        entrarLobbyPanel.SetActive(true);
        
        conectorDeTransport.Setup();
    }

    // Chamado quando um cliente tenta entrar em um lobby já criado
    public void EntrarNoLobby() {
        entrarLobbyPanel.SetActive(false);

        MostrarCarregamento("Tentando conectar...", CancelarEntrada);
        conectorDeTransport.ConectarCliente();
    }

    // Chamado quando um cliente entra no lobby com sucesso (pelo LobbyPlayer)
    public void EntrouNoLobby() {
        EsconderCarregamento();
        UpdateProntoUI();
        SelecionarBotao(trocarPersonagensButton);
    }

    public void CancelarEntrada() {
        EsconderCarregamento();
        entrarLobbyPanel.SetActive(true);
        conectorDeTransport.EncerrarCliente();
    }

    System.Action OnCancelarCarregamento = null;
    public void HandleCancelarCarregamento() {
        if (OnCancelarCarregamento != null) OnCancelarCarregamento.Invoke();
        OnCancelarCarregamento = null;

        switch (tipoDeTransport) {
            case TipoDeTransport.IP:
                SelecionarBotao(ipButton);
                break;
            case TipoDeTransport.Epic:
                SelecionarBotao(epicButton);
                break;
        }
    }

    public void MostrarCarregamento(string texto, System.Action onCancelar = null) {
        telaCarregamento.SetActive(true);
        SelecionarBotao(cancelarConexaoButton);
        telaCarregamentoTexto.text = texto;
        OnCancelarCarregamento = onCancelar;
    }

    public void EsconderCarregamento() {
        telaCarregamento.SetActive(false);
        SelecionarBotao(trocarPersonagensButton);
    }

    #endregion


    public void SairDoLobby() {
        conectorDeTransport.EncerrarHost();

        Destroy(networkManager.gameObject);
        if (GameManager.instance != null)  Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene(menuInicialScene, LoadSceneMode.Single);
    }
}

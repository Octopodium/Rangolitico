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
    public Text anglerText, heaterText;
    public GameObject anglerReady, heaterReady;
    public GameObject esperandoJogador;

    


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

    void OnDisable() { }

    // Pede pro servidor trocar os personagens
    public void TrocarPersonagens() {
        UpdateP1ProntoUI(false);
        UpdateP2ProntoUI(false);

        if (p1.isLocalPlayer) p1.TrocarPersonagens();
        else p2.TrocarPersonagens();
    }

    public void UpdateNominhos(LobbyPlayer lobbyPlayer) {
        UpdateNominhos();
    }

    // Quando o servidor trocou os personagens
    public void UpdateNominhos() {
        if (networkManager.lobbyPlayers == null) return;

        string p1Nome = "???";
        string p2Nome = "???";


        if (p1 != null)
            p1Nome = p1.nome;
        
        if (p2 != null)
            p2Nome = p2.nome;

        if ((p1 != null && p1.personagem == DishNetworkManager.Personagem.Angler) || (p2 != null && p2.personagem == DishNetworkManager.Personagem.Heater)) {
            anglerText.text = p1Nome;
            heaterText.text = p2Nome;
        } else {
            anglerText.text = p2Nome;
            heaterText.text = p1Nome;
        }
    }

    public void SetPronto() {
        SetComecar();

        LobbyPlayer lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        lobbyPlayer.SetPronto(!lobbyPlayer.pronto);
    }

    public void UpdateP1ProntoUI(bool val) {
        if (p1 == null) return;

        if (p1.personagem == DishNetworkManager.Personagem.Heater) heaterReady.SetActive(val);
        else anglerReady.SetActive(val);

        if (p1.isLocalPlayer) {
            //prontoButton.GetComponentInChildren<Text>().text = (val) ? "Cancelar" : "Pronto";
        }
    }

    public void UpdateP2ProntoUI(bool val) {
        if (p2 == null) return;

        if (p2.personagem == DishNetworkManager.Personagem.Angler) anglerReady.SetActive(val);
        else heaterReady.SetActive(val);

        if (p2.isLocalPlayer) {
            //prontoButton.GetComponentInChildren<Text>().text = (val) ? "Cancelar" : "Pronto";
        }
    }

    public void UpdateAguardandoJogadorUI() {
        esperandoJogador.SetActive(p1 == null || p2 == null);
    }

    public void SetComecar() {
        if (p1 == null || p2 == null) return;

        if (p1.isLocalPlayer) p1.TentarComecar();
        else p2.TentarComecar();
    }




    [Header("Entrar em Lobby")]
    public GameObject entrarLobbyPanel;
    public InputField ipInputField, portInputField;

    public void PrepararPraEntrarLobby() {
        entrarLobbyPanel.SetActive(true);
        ipInputField.text = networkManager.networkAddress;

        TelepathyTransport telepathyTransport = networkManager.gameObject.GetComponent<TelepathyTransport>();
        if (telepathyTransport != null) {
            portInputField.text = telepathyTransport.port.ToString();
        }
    }

    public void ComecarHostear() {
        entrarLobbyPanel.SetActive(false);
        networkManager.StartHost();
    }

    public void EntrarNoLobby() {
        entrarLobbyPanel.SetActive(false);

        networkManager.networkAddress = ipInputField.text;
        TelepathyTransport telepathyTransport = networkManager.gameObject.GetComponent<TelepathyTransport>();
        if (telepathyTransport != null) {
            telepathyTransport.port = ushort.Parse(portInputField.text);
        }

        networkManager.StartClient();
    }

    public void SairDoLobby() {
        networkManager.StopHost();
        networkManager.StopClient();
        networkManager.StopServer();

        Destroy(networkManager.gameObject);
        if (GameManager.instance != null)  Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene(menuInicialScene, LoadSceneMode.Single);
    }
}

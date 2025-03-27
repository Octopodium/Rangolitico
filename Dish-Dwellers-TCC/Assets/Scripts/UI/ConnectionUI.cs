using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ConnectionUI : MonoBehaviour {
    public static ConnectionUI instance;

    public DishNetworkManager networkManager;

    [Header("UI")]
    public Button prontoButton;
    public Text anglerText, heaterText;
    public GameObject anglerReady, heaterReady;
    public GameObject esperandoJogador;

    void Awake() {
        instance = this;
    }

    void Start() {
        networkManager = (DishNetworkManager)NetworkManager.singleton;
        networkManager.OnTrocarPersonagens += UpdateNominhos;

        /*
        networkManager.OnClientConnect += OnClientConnect;
        networkManager.OnClientDisconnect += OnClientDisconnect;
        networkManager.OnClientReady += OnClientReady;
        */

        UpdateNominhos(true);
    }

    void OnDisable() {
        networkManager.OnTrocarPersonagens -= UpdateNominhos;
    }

    // Pede pro servidor trocar os personagens
    public void TrocarPersonagens() {
        networkManager.TrocarPersonagens();
        UpdateP1ProntoUI(false);
        UpdateP2ProntoUI(false);
    }

    // Quando o servidor trocou os personagens
    void UpdateNominhos(bool isPlayerOneHeater) {
        if (networkManager.lobbyPlayers == null) return;

        string p1 = networkManager.lobbyPlayers[0] == null ? "???" : networkManager.lobbyPlayers[0].nome;
        string p2 = networkManager.lobbyPlayers[1] == null ? "???" : networkManager.lobbyPlayers[1].nome;

        anglerText.text = isPlayerOneHeater ? p2 : p1;
        heaterText.text = isPlayerOneHeater ? p1 : p2;
    }

    public void SetPronto() {
        LobbyPlayer lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        lobbyPlayer.SetPronto(true);
    }

    public void UpdateP1ProntoUI(bool val) {
        if (networkManager.isPlayerOneHeater) heaterReady.SetActive(val);
        else anglerReady.SetActive(val);
    }

    public void UpdateP2ProntoUI(bool val) {
        if (networkManager.isPlayerOneHeater) anglerReady.SetActive(val);
        else heaterReady.SetActive(val);
    }
}

using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ConnectionUI : MonoBehaviour {
    public static ConnectionUI instance;

    public DishNetworkManager networkManager;
    [HideInInspector] public LobbyPlayer p1, p2;


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

        /*
        networkManager.OnClientConnect += OnClientConnect;
        networkManager.OnClientDisconnect += OnClientDisconnect;
        networkManager.OnClientReady += OnClientReady;
        */

        UpdateNominhos();
    }

    void OnDisable() {
        /*
        networkManager.OnClientConnect -= OnClientConnect;
        networkManager.OnClientDisconnect -= OnClientDisconnect;
        networkManager.OnClientReady -= OnClientReady;
        */
    }

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
        LobbyPlayer lobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        lobbyPlayer.SetPronto(true);
    }

    public void UpdateP1ProntoUI(bool val) {
        if (p1 == null) return;

        if (p1.personagem == DishNetworkManager.Personagem.Heater) heaterReady.SetActive(val);
        else anglerReady.SetActive(val);
    }

    public void UpdateP2ProntoUI(bool val) {
        if (p2 == null) return;

        if (p2.personagem == DishNetworkManager.Personagem.Angler) anglerReady.SetActive(val);
        else heaterReady.SetActive(val);
    }

    public void UpdateAguardandoJogadorUI() {
        esperandoJogador.SetActive(p1 == null || p2 == null);
    }
}

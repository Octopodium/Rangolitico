using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PauseUI : MonoBehaviour {
    public GameObject[] telasInternasPause;
    public GameObject telaPrincipalPause;

    [Header("Event System")]
    public EventSystem eventSystem;
    public GameObject primeiroSelecionadoPause;

    bool inicializado = false;

    [Header("Opcoes - Controles Locais")]
    public Selectable campoEmCimaDoPainelDeControles;
    public GameObject controlesLocaisPanel;
    public Button singleplayerButton, multiplayerButton, ajustarMultiplayerButton;
    public TextMeshProUGUI habilitarTimerText, desativarTimerText, mostrarTimerText;

    public System.Action<bool> OnTimerHabilitadoChange;

    void Start() {
        RefreshHabilitarTimer();
    }


    // Chamado no UIManager
    public void Inicializar() {
        if (GameManager.instance != null)
            GameManager.OnPause += HandlePausa;

        if (eventSystem == null) {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }

        inicializado = true;
    }


    private void OnDestroy() {
        if (GameManager.instance != null)
            GameManager.OnPause -= HandlePausa;
        
        inicializado = false;
    }

    public void HandlePausa(bool estado){
        if (gameObject == null || !inicializado) {
            return;
        }

        if (estado) {
            eventSystem.SetSelectedGameObject(primeiroSelecionadoPause);

            telaPrincipalPause?.SetActive(true);

            if (telasInternasPause.Length > 0) {
                foreach (GameObject tela in telasInternasPause) {
                    tela.SetActive(false);
                }
            }


            // Tela de Opções possui campos que só aparecem em determinados modos de jogo
            if (GameManager.instance != null) {
                bool multLocal = GameManager.instance.modoDeJogo == ModoDeJogo.MULTIPLAYER_LOCAL;
                bool single = GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER;

                Navigation navigation = campoEmCimaDoPainelDeControles.navigation;

                if (multLocal || single) {
                    singleplayerButton.gameObject.SetActive(multLocal);
                    ajustarMultiplayerButton.gameObject.SetActive(multLocal);
                    multiplayerButton.gameObject.SetActive(single);
                    navigation.selectOnDown = multLocal ? singleplayerButton : multiplayerButton;

                    Navigation multNav = ajustarMultiplayerButton.navigation;
                    multNav.selectOnLeft = singleplayerButton;
                    ajustarMultiplayerButton.navigation = multNav;
                } else {
                    singleplayerButton.gameObject.SetActive(false);
                    multiplayerButton.gameObject.SetActive(false);
                    ajustarMultiplayerButton.gameObject.SetActive(false);
                    navigation.selectOnDown = null;

                    Navigation multNav = ajustarMultiplayerButton.navigation;
                    multNav.selectOnLeft = null;
                    ajustarMultiplayerButton.navigation = multNav;
                }

                campoEmCimaDoPainelDeControles.navigation = navigation;
            }
        }

        gameObject.SetActive(estado);
    }

    public void DespauseNoResume(){ 
        if(GameManager.instance != null){
            GameManager.instance.Despausar();
        }
    }

    public void VoltarParaMenu() {
        if(GameManager.instance != null){
            GameManager.instance.VoltarParaMenu();
        }
    }


    public void SetModoSingleplayer() {
        if (GameManager.instance == null) return;

        GameManager.instance.Despausar();
        GameManager.instance.SetModoSingleplayer();
    }

    public void SetModoMultiplayerLocal() {
        if (GameManager.instance == null) return;

        GameManager.instance.Despausar();
        eventSystem.SetSelectedGameObject(null);
        GameManager.instance.SetModoMultiplayerLocal();
    }

    public void RedefinirControlesMultiplayer() {
        if (GameManager.instance == null) return;

        GameManager.instance.Despausar();
        eventSystem.SetSelectedGameObject(null);

        if (GameManager.instance.modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE) GameManager.instance.RedefinirControlesMultiplayerOnline();
        else GameManager.instance.RedefinirControlesMultiplayerLocal();
    }

    public void ToggleHabilitarTimer() {
        bool habilitado = GetTimerHabilitado();
        SetTimerMode(!habilitado);
    }

    public void RefreshHabilitarTimer() {
        bool habilitado = GetTimerHabilitado();
        SetTimerMode(habilitado);
    }

    public void SetTimerMode(bool habilitado = true) {
        habilitarTimerText.gameObject.SetActive(!habilitado);
        desativarTimerText.gameObject.SetActive(habilitado);
        
        PlayerPrefs.SetInt("mostra_timer", habilitado ? 1 : 0);
        OnTimerHabilitadoChange?.Invoke(habilitado);
        PlayerPrefs.Save();
    }

    public bool GetTimerHabilitado() {
        return PlayerPrefs.HasKey("mostra_timer") && (PlayerPrefs.GetInt("mostra_timer") == 1);
    }
}

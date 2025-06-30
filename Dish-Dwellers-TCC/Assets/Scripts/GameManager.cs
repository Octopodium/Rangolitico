using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine.Rendering;

public enum ModoDeJogo {SINGLEPLAYER, MULTIPLAYER_LOCAL, MULTIPLAYER_ONLINE, INDEFINIDO}; // Indefinido: substituto para NULL (de quando não foi definido ainda)

public class GameManager : MonoBehaviour {
    public ModoDeJogo modoDeJogo = ModoDeJogo.SINGLEPLAYER;

    public static GameManager instance;
    public InputController inputController; // Controlador de inputs do jogo, que gerencia os inputs dos jogadores
    public Actions input => inputController.actions; // Acesso ao InputActions do jogo


    public LeitorDeControle controle;

    // Eventos
    public Action<QualPlayer> OnTrocarControle; // Chamado no singleplayer, quando o jogador troca de controle, e no online para definir o jogador que está jogando
    public Action<Player,Player> OnPlayersInstanciados; // Chamado quando os jogadores são instanciados na cena
    public Action OnMudaDeSala;
    public static event UnityAction<bool> OnPause;



    public bool isOnline {
        get { return modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE; }
        set {
            if (value) {
                modoDeJogo = ModoDeJogo.MULTIPLAYER_ONLINE;
            } else {
                modoDeJogo = ModoDeJogo.SINGLEPLAYER;
            }
        }
    }
    public bool isServer {
        get { return NetworkServer.active; }
    }
    
    public string primeiraFaseSceneName = "1-1";
    public string menuPrincipalSceneName = "MainMenu"; // Cena do menu do jogo
    bool voltandoParaMenu = false; // Evitar fadiga

    


    [Header("Opção Offline")]
    public GameObject offlineAnglerPrefab;
    public GameObject offlineHeaterPrefab;

    // Usar > APENAS < na situação que o GameManager é destruido no Awake
    bool marcadoParaDestruir = false;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            marcadoParaDestruir = true;
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Lê o modo de jogo escolhido pelo jogador (é definido na escolha do menu)
        if (PartidaInfo.instance != null) {
            modoDeJogo = PartidaInfo.instance.modoDeJogo;
        }


        // Inicializar o input
        inputController.Inicializar();

        input.UI.Pause.started += Pause;
        input.Geral.TrocarPersonagens.performed += ctx => TrocarControleSingleplayer();

        // Referencia interna
        controle = GetComponent<LeitorDeControle>();


        if (!isOnline) {
            // Apenas no modo offline que o GameManager deve instanciar os jogadores, no modo online o NetworkManager faz isso
            GerarPlayersOfline();
        } else {
            // No modo online, não se muda a sala diretamente, mas sim através de uma mensagem
            NetworkClient.RegisterHandler<DishNetworkManager.AcaoPassaDeSalaMessage>(OnRequestedPassaDeSalaOnline);
        }
    }

    void Start() {
        inputController.ConfigurarInputs();

        if (!isOnline && !marcadoParaDestruir) AnalyticsManager.instance?.ComecarPartida();
    }

    bool partidaConcluida = false;
    public void SetPartidaConcluida() {
        partidaConcluida = true;
    }

    void OnDestroy() {
        if (marcadoParaDestruir) return;

        if (input != null) {
            input.UI.Pause.started -= Pause;
            input.Geral.TrocarPersonagens.performed -= TrocarControleSingleplayer;
        }

        if (isOnline) {
            if (isServer) AnalyticsManager.instance?.FinalizarPartida(partidaConcluida);
            DesligarOOnline();
        } else {
            AnalyticsManager.instance?.FinalizarPartida(partidaConcluida);
        }

        if (DialogueSystem.instance != null) {
            Destroy(DialogueSystem.instance.gameObject);
        }
    }

    public void Pause(InputAction.CallbackContext ctx) {
        Pause();
    }

    public void Pause() {
        if (Time.timeScale == 1) {
            OnPause?.Invoke(true);
            Time.timeScale = 0;
        } else {
            OnPause?.Invoke(false);
            Time.timeScale = 1;
        }
    }

    public void Pause(bool pausar) {
        if (pausar) {
            OnPause?.Invoke(true);
            Time.timeScale = 0;
        } else {
            OnPause?.Invoke(false);
            Time.timeScale = 1;
        }
    }

    #region Input

    public QualPlayer playerAtual { get; private set; } = QualPlayer.Player1;
    
    public void AtualizarControleSingleplayer() {
        TrocarControleSingleplayer(playerAtual);
    }

    public void TrocarControleSingleplayer(InputAction.CallbackContext ctx) {
        TrocarControleSingleplayer();
    }

    public void TrocarControleSingleplayer() {
        if (modoDeJogo != ModoDeJogo.SINGLEPLAYER) return;

        this.playerAtual = (this.playerAtual == QualPlayer.Player1) ? QualPlayer.Player2 : QualPlayer.Player1;

        OnTrocarControle?.Invoke(this.playerAtual);
    }

    public void TrocarControleSingleplayer(QualPlayer player){
        if (modoDeJogo != ModoDeJogo.SINGLEPLAYER) return;
        if (this.playerAtual == player) return;

        TrocarControleSingleplayer();
    }

    public QualPersonagem GetQualPersonagem(QualPlayer player) {
        foreach (Player jogador in jogadores) {
            if (jogador.qualPlayer == player) {
                return jogador.personagem;
            }
        }

        return QualPersonagem.Heater;
    }

    public QualPlayer GetQualPlayer(QualPersonagem personagem) {
        foreach (Player jogador in jogadores) {
            if (jogador.personagem == personagem) {
                return jogador.qualPlayer;
            }
        }

        return QualPlayer.Player1;
    }

    public Player GetPlayer(QualPlayer player) {
        foreach (Player jogador in jogadores) {
            if (jogador.qualPlayer == player) {
                return jogador;
            }
        }

        return null; // Retorna null se não encontrar o jogador
    }

    public Player GetPlayer(QualPersonagem personagem) {
        foreach (Player jogador in jogadores) {
            if (jogador.personagem == personagem) {
                return jogador;
            }
        }

        return null; // Retorna null se não encontrar o jogador
    }

    #endregion


    #region Sistema de salas
    public List<Player> jogadores = new List<Player>();
    private AsyncOperation cenaProx;
    private AsyncOperation unloading;
    private sala sala = null;
    public sala salaAtual{ get{return sala;} }
    public TransicaoDeTela telaDeLoading;
    public string cenaAtualNome;

    
    /// <summary>
    /// Descarrega a sala atual, finaliza o carregamento da proxima e posiciona o jogador no porximo ponto de spawn.
    /// </summary>
    public void PassaDeSala() {
        if (isOnline) RequestPassaDeSalaOnline();
        else PassaDeSalaOffline();
    }

    private void PassaDeSalaOffline() {
        // Inicio da transição

        if (!isOnline || isServer) AnalyticsManager.instance?.FinalizarSala();

        this.cenaAtualNome = sala.NomeProximaSala();

        sala.enabled = false;
        cenaProx.allowSceneActivation = true;

        OnMudaDeSala?.Invoke();
    }

    /// <summary>
    /// Reinicia a sala para as condições iniciais.
    /// </summary>
    public void ResetSala(){
        sala.ResetSala();
    }

    // Metodo lento para encontrar os jogadores
    private void GetPlayers(){
        foreach( var data in GameObject.FindGameObjectsWithTag("Player")){
            jogadores.Add(data.GetComponent<Player>());
        }
    }

    // Gera players caso o jogo seja rodado direto da cena, ao invés de um servidor
    private void GerarPlayersOfline() {
        if (isOnline) return;

        foreach (GameObject data in GameObject.FindGameObjectsWithTag("Player")) {
            Destroy(data);
        }

        jogadores.Clear();

        GameObject angler = Instantiate(offlineAnglerPrefab, Vector3.zero, Quaternion.identity);
        GameObject heater = Instantiate(offlineHeaterPrefab, Vector3.zero, Quaternion.identity);

        angler.transform.SetParent(transform, false);
        heater.transform.SetParent(transform, false);

        jogadores.Add(angler.GetComponent<Player>());
        jogadores.Add(heater.GetComponent<Player>());

        angler.name = "Angler";
        heater.name = "Heater";

        OnPlayersInstanciados?.Invoke(jogadores[0], jogadores[1]);
    }

    /// <summary>
    /// Caso exista uma sala prévia, inicia o descarregamento da mesma.
    /// Determina a sala informada como a sala atual do jogo.
    /// Inicia o pré-carregamento da cena seguinte.
    /// </summary>
    /// <param name="sala"></param>
    public void SetSala(sala sala){

        // Descarrega a sala anterior :
        if (this.sala != null) {
            StartCoroutine(UnloadSala(this.sala.gameObject.scene));
        }

        // Determina a sala informada como a sala atual :
        this.sala = sala;
        this.cenaAtualNome = SceneManager.GetActiveScene().name;

        if (!isOnline || isServer) AnalyticsManager.instance?.ComecarSala(cenaAtualNome);


        // Evita de tentar carregar uma sala quando está voltando para o menu principal:
        if (voltandoParaMenu) return;

        // Carrega informação para lightProbes:

        // Inicia o precarregamento da próxima sala :
        string proximaSala = sala.NomeProximaSala();
        if (proximaSala == string.Empty) {
            return;
        }
        StartCoroutine(PreloadProximaSala(proximaSala));

    }

    

    #region Corotinas de carregamento

    IEnumerator PreloadProximaSala(string salaPCarregar) {

        if (SceneUtility.GetBuildIndexByScenePath($"Scenes/{salaPCarregar}") == 0) {
            Debug.Log("Proxima cena não está contida na build ou, não está com o nome correto.");
            yield break;
        }

        cenaProx = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        cenaProx.allowSceneActivation = false;

        yield return new WaitUntil(() => cenaProx.isDone);

    }

    IEnumerator UnloadSala(Scene scene){
        unloading = SceneManager.UnloadSceneAsync(scene);
        
        yield return new WaitUntil(() => unloading.isDone);

        ProbeReferenceVolume.instance.SetActiveScene(SceneManager.GetActiveScene());
    }

    #endregion

    #endregion


    #region Online
    // Referente ao Online

    [HideInInspector] public QualPlayer playerOnlineAtual = QualPlayer.Player1; // O jogador que está jogando atualmente, no online

    /// <summary>
    /// Chamado quando o player local é instanciado.
    /// </summary>
    public void SetarPlayerAtualOnline(QualPlayer player) {
        if (isOnline) {
            playerOnlineAtual = player;
            OnTrocarControle?.Invoke(player);
        }
    }

    public void ComecarOnline() {
        if (!isOnline) return;

        // Se o jogo estiver online, inicia a cena online
        StartCoroutine(ComecarOnlineAsync());
    }

    public IEnumerator ComecarOnlineAsync() {
        if (!isOnline) yield break;

        foreach (Transform child in transform) {
            if (child.GetComponent<Player>() == null) continue;
            Destroy(child.gameObject);
        }
        jogadores.Clear();

        GetPlayers();

        foreach (Player player in jogadores) {
            if (player == null) continue;
            if (player.transform.parent != transform)
                player.transform.SetParent(transform, false);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(primeiraFaseSceneName, LoadSceneMode.Single);
        op.allowSceneActivation = true;

        yield return new WaitUntil(() => op.isDone);

        if (isServer) AnalyticsManager.instance?.ComecarPartida();

        sala sala = GameObject.FindFirstObjectByType<sala>();
        sala.PosicionarJogador();

        OnPlayersInstanciados?.Invoke(jogadores[0], jogadores[1]);

        UIManager uiManager = GetComponentInChildren<UIManager>(true);
        if (uiManager != null) {
            uiManager.gameObject.SetActive(true);
        }
    }


    

    /// <summary>
    /// Envia uma mensagem para servidor pedindo para passar de sala.
    /// </summary>
    private void RequestPassaDeSalaOnline() {
        NetworkClient.Send(new DishNetworkManager.RequestPassaDeSalaMessage(true, salaAtual?.GetNome()));
    }

    /// <summary>
    /// Recebe a mensagem de passar de sala do servidor e chama o método para passar de sala offline.
    /// (Roda em todos os clientes)
    /// </summary>
    private void OnRequestedPassaDeSalaOnline(DishNetworkManager.AcaoPassaDeSalaMessage msg) {
        if (isOnline && msg.passarDeSala) {
            DestruirNetworkIdentityPassaCena();
            PassaDeSalaOffline();
        }
    }

    public void DestruirNetworkIdentityPassaCena() {
        if (!isOnline) return;

        foreach (NetworkIdentity identity in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (identity.gameObject.scene.name == "DontDestroyOnLoad") continue; // Não destrói objetos que estão na cena DontDestroyOnLoad
            DestroyImmediate(identity.gameObject);
        }
    }

    public void DesligarOOnline() {
        if (!isOnline) return;

        NetworkManager networkManager = NetworkManager.singleton;
        if (networkManager != null) {
            try {
                networkManager.StopClient();
                networkManager.StopServer();
            } catch (Exception e) {
                Debug.LogError($"Erro ao parar o NetworkManager: {e.Message}");
            }
            

            Destroy(networkManager.gameObject);
        }
    }

    #endregion


    public void VoltarParaMenu() {
        if (voltandoParaMenu) return;
        voltandoParaMenu = true;

        if (!isOnline || isServer)  {
            AnalyticsManager.instance?.FinalizarSala(false);
            AnalyticsManager.instance?.FinalizarPartida(false);
        }

        DesligarOOnline();

        gameObject.SetActive(true);

        ForcarCenaAguardando();

        StartCoroutine(VoltarParaMenuAsync());
    }

    IEnumerator VoltarParaMenuAsync() {
        Time.timeScale = 1;

        AsyncOperation op = SceneManager.LoadSceneAsync(menuPrincipalSceneName, LoadSceneMode.Single);
        if (op != null) {
            op.allowSceneActivation = true;
            yield return new WaitUntil(() => op.isDone);
        }

        voltandoParaMenu = false;

        input.Disable();

        instance = null;
        Destroy(gameObject);
    }
    
    public void ForcarCenaAguardando() {
        if (cenaProx != null) {
            cenaProx.allowSceneActivation = true;
            cenaProx = null;
        }
    }

}

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
    public SelacaoDePersonagem selecaoDePersonagem;
    public UIManager uiManager;
    public Actions input => inputController.actions; // Acesso ao InputActions do jogo

    public GameTime gameTime;
    public float gameTimer {get {return gameTime.timer;}}
    public static float ultimoTimer;


    // Eventos
    public Action OnAtualizarModoCamera;
    public Action<QualPlayer> OnTrocarControle; // Chamado no singleplayer, quando o jogador troca de controle, e no online para definir o jogador que está jogando
    public Action<Player,Player> OnPlayersInstanciados; // Chamado quando os jogadores são instanciados na cena
    public Action OnMudaDeSala;
    public static event UnityAction<bool> OnPause;

    public Indicador indicadorAtual;

    public LayerMask layerChao;
    public bool jogadorMorto;


    public bool isOnline {
        get { return modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE; }
        set {
            if (value) {
                modoDeJogo = ModoDeJogo.MULTIPLAYER_ONLINE;
            }
            else {
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


        if (!isOnline) {
            // Apenas no modo offline que o GameManager deve instanciar os jogadores, no modo online o NetworkManager faz isso
            gameTime.Play();
            GerarPlayersOfline();
        } else {
            // No modo online, não se muda a sala diretamente, mas sim através de uma mensagem
            NetworkClient.RegisterHandler<DishNetworkManager.AcaoPassaDeSalaMessage>(OnRequestedPassaDeSalaOnline);
            NetworkClient.RegisterHandler<DishNetworkManager.HadPreReadyMessage>(OnHadPreReady);
        }

        gameTime.OnPause += RegistrarTimer;
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
        
        OnPause = null;

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

        if (gameTime != null) 
            gameTime.OnPause -= RegistrarTimer;
    }

    public void Pause(InputAction.CallbackContext ctx) {
        Pause();
    }

    public void Pause() {
        bool estaPausado = (isOnline && pausado) || Time.timeScale == 0;
        Pause(!estaPausado);
    }

    public void Despausar() {
        Pause(false);
    }

    bool pausado = false;
    public void Pause(bool pausar) {
        if (pausar) {
            OnPause?.Invoke(true);
            pausado = true;
        } else {
            OnPause?.Invoke(false);
            pausado = false;
        }

        if (!isOnline) Time.timeScale = pausar ? 0 : 1;
    }

    public bool isPaused { get { return  Time.timeScale == 0 || (isOnline && pausado); } }


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
            if (jogador?.qualPlayer == player) {
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



    public void SetModoDeJogo(ModoDeJogo modoDeJogo) {
        if (modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE || modoDeJogo == ModoDeJogo.INDEFINIDO) return;
        
        this.modoDeJogo = modoDeJogo;
        inputController.RedefinirInputs();
    }

    public void SetModoSingleplayer() {
        SetModoDeJogo(ModoDeJogo.SINGLEPLAYER);
        OnAtualizarModoCamera.Invoke();
    }

    public void SetModoMultiplayerLocal() {
        SetModoDeJogo(ModoDeJogo.MULTIPLAYER_LOCAL);
        OnAtualizarModoCamera.Invoke();
    }

    public void RedefinirControlesMultiplayerLocal() {
        SetModoMultiplayerLocal();
    }

    public void RedefinirControlesMultiplayerOnline() {
        if (jogadores.Count > 1)
            selecaoDePersonagem.ComecarSelecaoOnline();
    }

    #endregion


    #region Sistema de salas
    public List<Player> jogadores = new List<Player>();
    private AsyncOperation cenaProx;
    private AsyncOperation unloading;
    private sala sala = null;
    public sala salaAtual{ get{return sala;} }
    public string cenaAtualNome;
    [SerializeField] private TelaDeLoading telaDeLoading;
    public Action OnTerminaDeCarregarASala;
    public bool carregando;
    [SerializeField] private ITransicao telaDeTransicaoFogo, telaDeTransicaoAgua, telaDeTransicaoPorrada;
    bool passarDeCenaSemPre = false;
    string cenaSemPreload = "";


    
    public void PassaDeSala() {
        passarDeCenaSemPre = false;
        if (isOnline) RequestPassaDeSalaOnline();
        else StartCoroutine(PassaDeSalaOffline());
    }


    IEnumerator PassaDeSalaOffline() {
        // Inicio da transição
        carregando = true;
        gameTime.Pause();
        telaDeLoading.AtivarTelaDeCarregamento(true);
        yield return new WaitForSeconds(telaDeLoading.GetTempoDeTransicao());
        Debug.Log("Acabou a transição");
        gameTime.Play();

        if (!isOnline || isServer) AnalyticsManager.instance?.FinalizarSala();

        if (passarDeCenaSemPre){
            PassaDeSalaImediato(cenaSemPreload);
            yield break;
        }

        this.cenaAtualNome = sala.NomeProximaSala();

        sala.enabled = false;
        cenaProx.allowSceneActivation = true;

        OnMudaDeSala?.Invoke();
    }

    public void PassaDeSalaImediato(string nomeDaSala) {
        StartCoroutine(PassaDeSalaDireto(nomeDaSala));
    }

    IEnumerator PassaDeSalaDireto(string nomeDaSala) {
        carregando = true;
        telaDeLoading.AtivarTelaDeCarregamento(true);
        yield return new WaitForSeconds(telaDeLoading.GetTempoDeTransicao());
        cenaAtualNome = nomeDaSala;
        gameTime.Play();

        SceneManager.LoadScene(nomeDaSala, LoadSceneMode.Additive);
        OnMudaDeSala?.Invoke();
    }

    /// <summary>
    /// Reinicia a sala para as condições iniciais.
    /// </summary>
    public void ResetSala(AnimadorPlayer.fonteDeDano motivo = AnimadorPlayer.fonteDeDano.FOGO){
        Debug.Log("Toca transição");
        ITransicao transicao = null;
        switch(motivo) {
            case AnimadorPlayer.fonteDeDano.FOGO:
                transicao = telaDeTransicaoFogo;
            break;
            case AnimadorPlayer.fonteDeDano.AFOGADO:
                transicao = telaDeTransicaoAgua;
            break;
            case AnimadorPlayer.fonteDeDano.PORRADA:
                transicao = telaDeTransicaoPorrada;
            break;
        }
        StartCoroutine(TocarTransicao(transicao));
        sala.ResetSala();
    }

    IEnumerator TocarTransicao(ITransicao transicao) {
        transicao.gameObject.SetActive(true);
        gameTime.Pause();
        yield return new WaitForSecondsRealtime(transicao.GetDuracao());
        gameTime.Play();
        Debug.Log("Desmorreu");
        jogadorMorto = false;
    }

    // Utilizado no Online
    public void SoftResetSala(){
        sala.SoftReset();
    }

    // Metodo lento para encontrar os jogadores
    public void GetPlayers(){
        for (int i = jogadores.Count-1; i >= 0; i--) {
            if (jogadores[i] == null) jogadores.RemoveAt(i);
        }

        // Não procura por tag pois não iria incluir jogadores inativos
        foreach( var data in FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None)){
            Player jogador = data.GetComponent<Player>();
            if (jogador != null && !jogadores.Contains(jogador))
                jogadores.Add(jogador);
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

        // Evita de tentar carregar uma sala quando está voltando para o menu principal:
        if (voltandoParaMenu) return;

        Debug.Log("Setou a sala: " + sala.nSala);

        // Determina a sala informada como a sala atual :
        this.sala = sala;

        // Determina se deve salvar o progresso :
        SalvarProgresso();
        
        cenaAtualNome = SceneManager.GetActiveScene().name;

        if (!isOnline || isServer) AnalyticsManager.instance?.ComecarSala(cenaAtualNome);
        
        // Inicia o precarregamento da próxima sala :
        string proximaSala = sala.NomeProximaSala();
        if (proximaSala == string.Empty) {
            return;
        }
        StartCoroutine(PreloadProximaSala(proximaSala));

    }

    public void IrParaSalaSemPreload(string nomeSala){
        passarDeCenaSemPre = true;
        cenaSemPreload = nomeSala;

        if (isOnline) RequestPassaDeSalaOnline();
        else StartCoroutine(PassaDeSalaOffline());
    }

    private void SalvarProgresso(){
        if(ProgressManager.Instance == null){
            Debug.Log($"<color=yellow> ProgressManager não pôde ser encontrado, portanto, progresso pode não ser salvo.</color>");
            return;
        }

        if(sala == null || sala.nFase <= 0 || sala.nSala <= 0){
            Debug.Log($"<color=yellow> Valor da sala invalido, portanto, progresso pode não ser salvo.</color>");
            return;
        }

        Progress progresso = ProgressManager.Instance.CarregarProgresso();
        if(progresso != null){
            if(progresso.ultimoNivelAlcancado > sala.nFase) return;
            if(progresso.ultimoNivelAlcancado == sala.nFase && progresso.ultimaSalaAlcancada > sala.nSala) return;
        
        
            progresso.ultimoNivelAlcancado = sala.nFase;
            progresso.ultimaSalaAlcancada = sala.nSala;
        }
        else{
            progresso = new Progress{
                ultimoNivelAlcancado = sala.nFase,
                ultimaSalaAlcancada = sala.nSala,
            };
        }
        ProgressManager.Instance.SalvarProgresso(progresso);
    }

    

    #region Corotinas de carregamento

    IEnumerator PreloadProximaSala(string salaPCarregar) {

        if (SceneUtility.GetBuildIndexByScenePath($"Scenes/Salas/{salaPCarregar}") == 0) {
            Debug.Log("Proxima cena não está contida na build ou, não está com o nome correto.");
            yield break;
        }

        cenaProx = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        cenaProx.allowSceneActivation = false;

        yield return new WaitUntil(() => cenaProx == null|| cenaProx.isDone);

    }


    IEnumerator UnloadSala(Scene scene){
        unloading = SceneManager.UnloadSceneAsync(scene);
        
        yield return new WaitUntil(() => unloading.isDone);

        ProbeReferenceVolume.instance.SetActiveScene(SceneManager.GetActiveScene());
        
        telaDeLoading.AtivarTelaDeCarregamento(false);
        yield return new WaitForSeconds(telaDeLoading.GetTempoDeTransicao());
        OnTerminaDeCarregarASala?.Invoke();
        carregando = false;
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

        if (uiManager != null) {
            uiManager.gameObject.SetActive(true);
        }
    }

    public void SetarOnline(Player quemChamou = null) {
        if (!isOnline) return;

        // Se o jogo estiver online, inicia a cena online
        StartCoroutine(SetarOnlineAsync(quemChamou));
    }

    public IEnumerator SetarOnlineAsync(Player quemChamou = null) {
        if (!isOnline) yield break;

        foreach (Transform child in transform) {
            if (child.GetComponent<Player>() == null) continue;
            if (child.GetComponent<NetworkIdentity>() != null && child.GetComponent<NetworkIdentity>().enabled) continue;
            Destroy(child.gameObject);
        }

        GetPlayers();

        foreach (Player player in jogadores) {
            if (player == null) continue;
            if (player.transform.parent != transform)
                player.transform.SetParent(transform, false);
        }


        sala sala = GameObject.FindFirstObjectByType<sala>();
        if (quemChamou != null) sala.PosicionarApenasUmJogador(quemChamou);
        else sala.PosicionarJogador(quemChamou);

        sala.SoftReset();

        // OnPlayersInstanciados?.Invoke(jogadores[0], jogadores[1]);
        if (uiManager != null) {
            uiManager.gameObject.SetActive(true);
        }
    }


    
    

    /// <summary>
    /// Envia uma mensagem para servidor pedindo para passar de sala.
    /// </summary>
    private void RequestPassaDeSalaOnline() {
        NetworkClient.Send(new DishNetworkManager.RequestPassaDeSalaMessage(true, salaAtual?.GetNome(), salaAtual?.NomeProximaSala()));
    }

    /// <summary>
    /// Recebe a mensagem de passar de sala do servidor e chama o método para passar de sala offline.
    /// (Roda em todos os clientes)
    /// </summary>
    private void OnRequestedPassaDeSalaOnline(DishNetworkManager.AcaoPassaDeSalaMessage msg) {
        if (isOnline && msg.passarDeSala) {
            DestruirNetworkIdentityPassaCena();
            StartCoroutine(PassaDeSalaOffline());
        }
    }

    /// <summary>
    /// Recebe mensagem que um cliente está quase dando Ready
    /// </summary>
    /// <param name="msg"></param>
    private void OnHadPreReady(DishNetworkManager.HadPreReadyMessage msg) {
        Debug.Log("Um pre-ready!");
        sala.SoftReset();
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

        DishNetworkManager networkManager = NetworkManager.singleton as DishNetworkManager;
        if (networkManager != null) {
            try {
                networkManager.SairDoLobby();
                networkManager.StopClient();
                if (isServer) networkManager.StopServer();
            } catch (Exception e) {
                Debug.LogError($"Erro ao parar o NetworkManager: {e.Message}");
            }
            

            Destroy(networkManager.gameObject);
        }
    }

    public GameObject Instanciar(GameObject online, GameObject offline, Vector3 pos = default, Quaternion rot = default) {
        GameObject prefab = isOnline? online : offline;
        return Instantiate(prefab, pos, rot);
    }

    #endregion

    // Chamado pelo Portal.cs
    public void HandleChegouNoFim() {
        gameTime.Pause();

        if (isOnline) DesligarOOnline();
            
        ForcarCenaAguardando();
    }


    public void VoltarParaMenu() {
        if (voltandoParaMenu) return;
        voltandoParaMenu = true;

        Time.timeScale = 1;

        if (!isOnline || isServer)  {
            AnalyticsManager.instance?.FinalizarSala(false);
            AnalyticsManager.instance?.FinalizarPartida(false);
        }

        DesligarOOnline();

        gameObject.SetActive(true);

        ForcarCenaAguardando();

        gameTime.Pause();

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

        AnalyticsManager.instance?.LimparPartida();

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

    public void RegistrarTimer() {
        ultimoTimer = gameTime.timer;
    }

}

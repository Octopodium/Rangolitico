using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;

public enum ModoDeJogo {SINGLEPLAYER, MULTIPLAYER_LOCAL, MULTIPLAYER_ONLINE, INDEFINIDO}; // Indefinido: substituto para NULL (de quando não foi definido ainda)

public class GameManager : MonoBehaviour {
    public ModoDeJogo modoDeJogo = ModoDeJogo.SINGLEPLAYER;

    public static GameManager instance;
    public Actions input;
    public PlayerInputManager playerInputManager; // Gerencia os inputs dos jogadores locais


    public LeitorDeControle controle;
    

    // Eventos
    public System.Action<QualPlayer> OnTrocarControle; // Chamado no singleplayer, quando o jogador troca de controle, e no online para definir o jogador que está jogando
    public System.Action<Player,Player> OnPlayersInstanciados; // Chamado quando os jogadores são instanciados na cena
    public System.Action OnMudaDeSala;
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
    
    public string primeiraFaseSceneName = "1-1";
    public string menuPrincipalSceneName = "MainMenu"; // Cena do menu do jogo
    bool voltandoParaMenu = false; // Evitar fadiga

    


    [Header("Opção Offline")]
    public GameObject offlineAnglerPrefab;
    public GameObject offlineHeaterPrefab;


    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Lê o modo de jogo escolhido pelo jogador (é definido na escolha do menu)
        if (PartidaInfo.instance != null) {
            modoDeJogo = PartidaInfo.instance.modoDeJogo;
        }


        // Inicializar o input
        input = new Actions();
        input.Enable();

        input.UI.Pause.started += ctx => Pause();
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
        ConfigurarInputs();
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

    #region Input

    public PlayerInput player1Input, player2Input;
    public InputDevice player1Device, player2Device;

    public QualPlayer playerAtual { get; private set; } = QualPlayer.Player1;
    public Action<InputAction.CallbackContext, QualPlayer> OnInputTriggered;

    protected void ConfigurarInputs() {
        player1Input.gameObject.SetActive(true);
        player1Input.onActionTriggered += ctx => HandleOnInputTriggered(ctx, QualPlayer.Player1);
        player1Input.onDeviceLost += ctx => OnDeviceLost(player1Input, QualPlayer.Player1);

        if (modoDeJogo == ModoDeJogo.MULTIPLAYER_LOCAL) {
            player2Input.gameObject.SetActive(true);
            player2Input.onActionTriggered += ctx => HandleOnInputTriggered(ctx, QualPlayer.Player2);
            player2Input.onDeviceLost += ctx => OnDeviceLost(player2Input, QualPlayer.Player2);

            CadastrarDevices();
        } else {
            Destroy(player2Input.gameObject);
            player2Input = null;
        }
    }

    protected void OnDeviceLost(PlayerInput playerInput, QualPlayer qualPlayer) {
        Debug.Log($"Dispositivo perdido para {qualPlayer}: {playerInput.currentControlScheme}");

        if (qualPlayer == QualPlayer.Player1) {
            player1Device = null;
            player1Input.user.UnpairDevices();
            player1Input.DeactivateInput();
        } else if (qualPlayer == QualPlayer.Player2) {
            player2Device = null;
            player2Input.user.UnpairDevices();
            player2Input.DeactivateInput();
        }

        UIConexaoInGame.instancia.SetConectando(qualPlayer);
        input.Player.Get().actionTriggered += OuveAcoesParaCadastrarDevices;
    }

    protected void HandleOnInputTriggered(InputAction.CallbackContext ctx, QualPlayer qualPlayer) {
        if (modoDeJogo == ModoDeJogo.SINGLEPLAYER) {
            qualPlayer = playerAtual;
        } else if (modoDeJogo == ModoDeJogo.MULTIPLAYER_ONLINE) {
            qualPlayer = QualPlayer.Player1;
        }

        if (OnInputTriggered != null) {
            OnInputTriggered(ctx, qualPlayer);
        }
    }

    public PlayerInput GetPlayerInput(Player player) {
        if (modoDeJogo == ModoDeJogo.MULTIPLAYER_LOCAL) {
            return (player.qualPlayer == QualPlayer.Player1) ? player1Input : player2Input;
        } else {
            return (player.qualPlayer == playerAtual) ? player1Input : null;
        }
    }

    protected void CadastrarDevices() {
        if (modoDeJogo != ModoDeJogo.MULTIPLAYER_LOCAL) return;

        player1Device = null;
        player2Device = null;

        if (player1Input.user.valid) player1Input.user.UnpairDevices();
        if (player2Input.user.valid) player2Input.user.UnpairDevices();

        player1Input.DeactivateInput();
        player2Input.DeactivateInput();

        UIConexaoInGame.instancia.SetConectando(QualPlayer.Player1);
        UIConexaoInGame.instancia.SetConectando(QualPlayer.Player2);

        input.Player.Get().actionTriggered += OuveAcoesParaCadastrarDevices;
    }

    protected void OuveAcoesParaCadastrarDevices(InputAction.CallbackContext ctx) {
        InputDevice device = ctx.control.device;
        if (device == null) return;

        if (player1Device != null && player2Device != null) {
            input.Player.Get().actionTriggered -= OuveAcoesParaCadastrarDevices;
            return;
        }

        if (player1Device == null) {
            CadastrarDevice(QualPlayer.Player1, device);
        } else if (player2Device == null && player1Device != device) {
            CadastrarDevice(QualPlayer.Player2, device);
        }
    }

    protected void CadastrarDevice(QualPlayer player, InputDevice device) {
        if (device == null) return;

        bool isPlayer1 = player == QualPlayer.Player1;

        if (isPlayer1) {
            if (player2Device != null && player2Device == device) {
                // Se o dispositivo já estiver registrado para o Player2, não faz nada
                return;
            }

            player1Device = device;
        } else {
            if (player1Device != null && player1Device == device) {
                // Se o dispositivo já estiver registrado para o Player1, não faz nada
                return;
            }

            player2Device = device;
        }

        PlayerInput playerInput = isPlayer1 ? player1Input : player2Input;
        playerInput.user.UnpairDevices();
        InputUser.PerformPairingWithDevice(device, playerInput.user);

        // O Input System não reconhece automaticamente o esquema de controle, então é necessário definir manualmente... (??????)
        string controlScheme = GetControlSchemeName(device);
        playerInput.SwitchCurrentControlScheme(controlScheme, device);
        playerInput.ActivateInput();

        UIConexaoInGame.instancia.SetConectado(player);

        Debug.Log($"Dispositivo {device.displayName} cadastrado para o {player}. Device: {device}. Control Scheme: {controlScheme}");
    }

    string GetControlSchemeName(InputDevice device) {
        switch (device) {
            case Gamepad:
                return "Gamepad";
            case Keyboard:
                return "Keyboard&Mouse";
            case Touchscreen:
                return "Touch";
            case Joystick:
                return "Joystick";
            default:
                return "Unknown";
        }
    }

    public void AtualizarControleSingleplayer() {
        if (player1Input == null || player2Input == null) return;
        TrocarControleSingleplayer(playerAtual);
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

    #endregion


    #region Sistema de salas
    public List<Player> jogadores = new List<Player>();
    private AsyncOperation cenaProx;
    private sala sala = null;
    public sala salaAtual{ get{return sala;} }

    
    /// <summary>
    /// Descarrega a sala atual, finaliza o carregamento da proxima e posiciona o jogador no porximo ponto de spawn.
    /// </summary>
    public void PassaDeSala(){
        if (isOnline) RequestPassaDeSalaOnline();
        else PassaDeSalaOffline();
    }

    private void PassaDeSalaOffline() {
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
        if (this.sala != null){
            StartCoroutine(UnloadSala(this.sala.gameObject.scene));
        }

        // Determina a sala informada como a sala atual :
        this.sala = sala;

        // Evita de tentar carregar uma sala quando está voltando para o menu principal:
        if (voltandoParaMenu) return;

        // Inicia o precarregamento da próxima sala :
        string proximaSala = sala.NomeProximaSala();
        if(proximaSala == string.Empty){
            return;
        }
        StartCoroutine(PreloadProximaSala(proximaSala));

    }

    #region Corotinas de carregamento

    IEnumerator PreloadProximaSala(string salaPCarregar){

        if(SceneUtility.GetBuildIndexByScenePath($"Scenes/{salaPCarregar}") == 0){
            Debug.Log("Proxima cena não está contida na build ou, não está com o nome correto.");
            yield break;
        }

        cenaProx = SceneManager.LoadSceneAsync(salaPCarregar, LoadSceneMode.Additive);
        cenaProx.allowSceneActivation = false;
    }

    IEnumerator UnloadSala(Scene scene){
        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
        
        yield return new WaitUntil(() => op.isDone);
        Debug.Log("Terminou de descarregar : " + scene.name);
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
            PassaDeSalaOffline();
        }
    }

    #endregion


    public void VoltarParaMenu() {
        if (voltandoParaMenu) return;
        voltandoParaMenu = true;

        if (isOnline) {
            NetworkManager networkManager = NetworkManager.singleton;
            if (networkManager != null) {
                networkManager.StopHost();
                networkManager.StopClient();
                networkManager.StopServer();

                Destroy(networkManager.gameObject);
            }
        }

        gameObject.SetActive(true);

        if (cenaProx != null) {
            cenaProx.allowSceneActivation = true;
            cenaProx = null;
        }

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

}

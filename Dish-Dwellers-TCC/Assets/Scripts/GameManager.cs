using UnityEngine;
using UnityEngine.InputSystem;
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

    public void AAAa(string teste){
        Debug.Log("aaa: " + teste);
    }


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
        
        SetarInputs();


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

    public void Pause(){
        if(Time.timeScale == 1){
            OnPause?.Invoke(true);
            Time.timeScale = 0;
        }else{
            OnPause?.Invoke(false);
            Time.timeScale = 1;
        }
    }
    

    #region Input

    Actions input2_singleplayer;
    public QualPlayer playerAtual {get; private set;} = QualPlayer.Player1;

    public InputActionMap GetPlayerInput(QualPlayer player = QualPlayer.Player1){
        switch(modoDeJogo){
            case ModoDeJogo.SINGLEPLAYER:
                return player == QualPlayer.Player1 ? input.Player.Get() : input2_singleplayer.Player.Get();
            case ModoDeJogo.MULTIPLAYER_LOCAL:
                return player == QualPlayer.Player1 ? input.Player.Get() : input.Player2.Get();
            case ModoDeJogo.MULTIPLAYER_ONLINE:
                return input.Player.Get();
            default:
                return null;
        }
    }

    void SetarInputs() {
        switch(modoDeJogo){
            case ModoDeJogo.SINGLEPLAYER:
                if (input2_singleplayer == null) {
                    input2_singleplayer = new Actions();
                    input2_singleplayer.Enable();
                }

                input.Player2.Disable();
                input2_singleplayer.Player2.Disable();

                if (playerAtual == QualPlayer.Player1) {
                    input.Player.Enable();
                    input2_singleplayer.Player.Disable();
                } else {
                    input.Player.Disable();
                    input2_singleplayer.Player.Enable();
                }

                break;
            case ModoDeJogo.MULTIPLAYER_LOCAL:
                input.Player.Enable();
                input.Player2.Enable();
                break;
            case ModoDeJogo.MULTIPLAYER_ONLINE:
                input.Player.Enable();
                input.Player2.Disable();
                break;
            default:
                return;
        }
    }

    public void TrocarControleSingleplayer(){
        if (modoDeJogo != ModoDeJogo.SINGLEPLAYER) return;

        if (playerAtual == QualPlayer.Player1) {
            playerAtual = QualPlayer.Player2;
            input.Player.Disable();
            input2_singleplayer.Player.Enable();
        } else {
            playerAtual = QualPlayer.Player1;
            input2_singleplayer.Player.Disable();
            input.Player.Enable();
        }

        OnTrocarControle?.Invoke(playerAtual);
    }

    public void TrocarControleSingleplayer(QualPlayer player){
        if (modoDeJogo != ModoDeJogo.SINGLEPLAYER) return;

        if (player == QualPlayer.Player2) {
            playerAtual = QualPlayer.Player2;
            input.Player.Disable();
            input2_singleplayer.Player.Enable();
        } else {
            playerAtual = QualPlayer.Player1;
            input2_singleplayer.Player.Disable();
            input.Player.Enable();
        }

        OnTrocarControle?.Invoke(playerAtual);
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
        NetworkClient.Send(new DishNetworkManager.RequestPassaDeSalaMessage(true));
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
        if (input2_singleplayer != null) input2_singleplayer.Disable();

        instance = null;
        Destroy(gameObject);
    }

}

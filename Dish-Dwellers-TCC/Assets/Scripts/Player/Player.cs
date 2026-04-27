using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Collections;
using Mirror;
using System.Linq;

public enum QualPlayer { Player1, Player2, Desativado }
public enum QualPersonagem { Heater, Angler }

[RequireComponent(typeof(Carregador)), RequireComponent(typeof(Carregavel))]
public class Player : NetworkBehaviour, SincronizaMetodo, IGanchavelAntesPuxar {
    public QualPersonagem personagem = QualPersonagem.Heater;
    public QualPlayer qualPlayer = QualPlayer.Player1;
    [HideInInspector] public PlayerInput playerInput => GameManager.instance.inputController.GetPlayerInput(this);



    [Header("Atributos do Player")]
    public float velocidade = 14f;
    public float velocidadeRB = 14f; // Velocidade do Rigidbody
    bool sendoPuxado = false; // Se o jogador está sendo puxado por um gancho (definido no GanchavelAntesPuxar)

    [HideInInspector] public Vector3 direcao; // Direção que o jogador está olhando e movimentação atual (enquanto anda direcao = movimentacao)
    [HideInInspector] public Vector3 mira;
    [HideInInspector] public Vector3 movimentacao;

    private int _playerVidas = 3;
    public int playerVidas {
        get { return _playerVidas; }
        set {
            _playerVidas = Mathf.Clamp(value, 0, 3);
            OnVidaMudada?.Invoke(this, _playerVidas);

            // if (_playerVidas == 0){
            //     Morrer();
            // }
        }
    }
    public static event UnityAction<Player, int> OnVidaMudada; //Evento global para dano ou ganhar vida


    [Header("Configuração de Interação")]
    public int maxInteragiveisEmRaio = 8;
    public float raioInteracao = 1f;
    public LayerMask layerInteragivel;
    public float velocidadeCarregandoMult = 0.85f;
    public InteragivelBase ultimoInteragivel;
    Collider[] collidersInteragiveis;
    public List<Collider> collidersIgnoraveis = new List<Collider>(); // Lista de colisores que o jogador não pode interagir
    public float velocidadeEmpurrandoMult = 0.5f;
    [HideInInspector] public bool empurrando = false;
    [HideInInspector] public Barco barcoEmbarcado;
    public bool embarcado { get { return barcoEmbarcado != null; }}


    [Header("Referências")]
    public GameObject visualizarDirecao;
    public Transform pontoCentral; // pros bicho mirar certo e não atirarem no pé.
    bool podeMovimentar = true; // Solução TEMPORARIA enquanto não há estados implementados
    public GameObject dropShadow;
    


    // Titizim:
    [Header("Config do Escudo")] [Space(10)]
    public bool escudo;
    public bool escudoAtivo { get; set; }
    public float velocidadeComEscudoMult = 0.65f;

    [Header("Config de Mira")] [Space(10)]
    public float velocidadeGanchadoMult = 0.75f;
    public bool atirandoGancho = false;
    public bool estaMirando = false;
    private Vector2 inputMira;
    public float deadzoneMira = 0.1f; // Zona morta para evitar mira acidental

    [Header("Configurações de Knockback")] [Space(10)]
    public float forcaKnockback = 5f;
    public float duracaoKnockback = 0.3f;
    public float componenteVerticalKnockback = 0.2f;
    public bool estaSofrendoKnockback = false;

    public System.Action<bool> onEmoteWheel;




    // Referências internas
    public Ferramenta ferramenta;
    [HideInInspector] public Carregador carregador; // O que permite o jogador carregar coisas
    public Carregavel carregando => carregador.carregado; // O que o jogador está carregando
    Carregavel carregavel; // O que permite o jogador a ser carregado
    public bool sendoCarregado => carregavel.sendoCarregado; // Se o jogador está sendo carregado
    Ganchavel ganchavel; // O que permite o jogador ser puxado pelo gancho
    public bool ganchado => ganchavel != null && ganchavel.ganchado; // Se o jogador está ganchado
    AnimadorPlayer animacaoJogador;

    public bool estaNoChao = true;
    CharacterController characterController;
    Rigidbody rb; // Rigidbody do jogador (se houver)
    Collider col;
    Grudavel grudavel;
    AndadorSobChao andador;
    public UnityEvent onTomarDano;

    public Indicador indicador;
    public System.Action<InputDevice> OnDeviceChange;
    public InputDevice controleAtual { get; private set; }
    private bool morto = false;

    // Awake: trata de referências/configurações internas
    void Awake() {
        collidersInteragiveis = new Collider[maxInteragiveisEmRaio];

        direcao = new Vector3(0, 0, -1);

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        characterController = GetComponent<CharacterController>();
        carregador = GetComponent<Carregador>();
        carregavel = GetComponent<Carregavel>();
        ganchavel = GetComponent<Ganchavel>();
        ferramenta = GetComponentInChildren<Ferramenta>();
        ferramenta.Inicializar(this);
        
        grudavel = gameObject.GetComponent<Grudavel>();
        if (grudavel == null) grudavel = gameObject.AddComponent<Grudavel>();
        if (gameObject.GetComponent<GrudaEmChoes>() == null) gameObject.AddComponent<GrudaEmChoes>();

        andador = GetComponent<AndadorSobChao>();
        andador.modoDeCheck = AndadorSobChao.CheckDeChao.Script;
        
        animacaoJogador = GetComponentInChildren<AnimadorPlayer>();

        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders) {
            collidersIgnoraveis.Add(col);
        }

        // Se está carregando algo, ignora a interação com esta coisa
        carregador.OnCarregar += (carregavel) => { if (carregavel != null) collidersIgnoraveis.Add(carregavel.GetComponent<Collider>()); ResetarFerramenta(); };
        carregador.OnSoltar += (carregavel) => { if (carregavel != null) collidersIgnoraveis.Remove(carregavel.GetComponent<Collider>()); };

        // Se o jogador está sendo carregado, ignora a interação com o carregador
        carregavel.OnCarregado += (carregador) => { if (carregador != null) collidersIgnoraveis.Add(carregador.GetComponent<Collider>()); UsarRB(); };
        carregavel.OnSolto += (carregador) => { if (carregador != null) collidersIgnoraveis.Remove(carregador.GetComponent<Collider>()); };
        
    }

    // Start: trata de referências/configurações externas
    void Start() {
        if (GameManager.instance.isOnline) {
            if (isLocalPlayer){
                transform.SetParent(GameManager.instance.transform, true);
                GameManager.instance.SetarPlayerAtualOnline(qualPlayer);
            }

            qualPlayer = isLocalPlayer ? QualPlayer.Player1 : QualPlayer.Desativado;
        }

        GameManager.instance.inputController.OnInputTriggered += OnInputTriggered; // Registra o evento de input do GameManager

        estaNoChao = CheckEstaNoChao(); // Verifica se o jogador está no chão
        if (estaNoChao){ 
            UsarCC(true); // Se o jogador está no chão, habilita o CharacterController (desabilita o Rigidbody)
            UsarAtrito(true);
        }
        else{
            UsarRB(true); // Se o jogador não está no chão, desabilita o CharacterController (habilita o Rigidbody)
            UsarAtrito(false);
        }

        ultimaPosicao = transform.position;
    }

    public void OnInputTriggered(InputAction.CallbackContext ctx, QualPlayer qualPlayer) {
        if (qualPlayer != this.qualPlayer) return;
        if (!ehJogadorAtual) return; // Se não é o jogador atual, não faz nada
        if (GameManager.instance.isPaused) return; // Se o jogo está pausado, não faz nada
        if(GameManager.instance.jogadorMorto) return; // Se algum dos jogadores morreu, n faz mais nada.
        

        if (ctx.control.device != controleAtual) {
            controleAtual = ctx.control.device;
            OnDeviceChange?.Invoke(controleAtual);
        }

        switch (ctx.action.name) {
            case "Interact":
                if (ctx.performed) Interagir(ctx);
                break;
            case "Attack":
                if (ctx.performed) AcionarFerramenta(ctx);
                else SoltarFerramenta(ctx);
                break;
            case "Aim":
                Mira();
                break;
            case "EmoteWheel":
                onEmoteWheel?.Invoke(ctx.performed);
                break;
        }
    }

    void OnEnable() {
        if (GameManager.instance != null && GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER) {
            GameManager.instance.AtualizarControleSingleplayer();
        }
    }

    void OnDisable() {
        if (ultimoInteragivel != null) {
            ultimoInteragivel.MostrarIndicador(false, indicador);
            ultimoInteragivel = null;
        }

        Resetar();

        if (GameManager.instance != null && GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && playerInput != null && playerInput.enabled) {
            GameManager.instance.TrocarControleSingleplayer();
        }
    }

    public bool ehJogadorAtual { get {
            if (GameManager.instance == null) return true;
            
            switch (GameManager.instance.modoDeJogo) {
                case ModoDeJogo.SINGLEPLAYER:
                    return playerInput != null && playerInput.enabled;
                case ModoDeJogo.MULTIPLAYER_ONLINE:
                    return isLocalPlayer;
                default:
                    return true;
            }
        }
    }

    void FixedUpdate() {
        if (GameManager.instance.jogadorMorto) return;
        // No modo singleplayer, caso este jogador não seja o atual, não faz nada
        if (GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && (playerInput == null || !playerInput.enabled)) {
            if (ultimoInteragivel != null) {
                ultimoInteragivel.MostrarIndicador(false, indicador);
                ultimoInteragivel = null;
            }

            movimentacao = Vector3.zero;
            animacaoJogador.Mover(movimentacao);
        }

        if (ehJogadorAtual) ChecarInteragiveis();
        Movimentacao();

        DesenharTrajetoria();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if(hit.collider.CompareTag("Queimavel")){
            AplicarKnockback(hit.transform);
        }
    }

    void OnDestroy() {
        if (GameManager.instance?.isOnline == true && isLocalPlayer) {
            GameManager.instance?.VoltarParaMenu();
        }
    }


    /// <summary>
    /// Aumenta ou diminui a vida do jogador
    /// </summary>
    /// <param name="valor">Valor a ser adicionado ou subtraído da vida do jogador</param>
    [Sincronizar]
    public void MudarVida(int valor, AnimadorPlayer.fonteDeDano motivo) {
        if(GameManager.instance.jogadorMorto) return;
        gameObject.Sincronizar(valor, motivo);
        if (valor < 0) {
            motivoDeDano = nameof(motivo);
            if (playerVidas + valor <= 0) {
                if (!GameManager.instance.isOnline || isServer) {
                    AnalyticsManager.instance?.RegistrarMorte(motivoDeDano, transform.position);
                }
                Morrer(motivo);
            }
            onTomarDano?.Invoke();
        }

        playerVidas += valor;
    }

    /// <summary>
    /// Chamado quando o jogador morre
    /// </summary>
    [Sincronizar]
    public void Morrer(AnimadorPlayer.fonteDeDano fonte) {
        dustVisualEffect.SetFloat("Count", 0);
        gameObject.Sincronizar(fonte);
        StartCoroutine(TocarAnimacaoDeMorte(fonte));
        GameManager.instance.jogadorMorto = true;
        Debug.Log("morreu");
    }

    IEnumerator TocarAnimacaoDeMorte(AnimadorPlayer.fonteDeDano fonte) {
        float duracao = animacaoJogador.Morte(fonte);
        yield return new WaitForSeconds(duracao);
        animacaoJogador.ResetAnimador();
        GameManager.instance.ResetSala(fonte);
    }

    /// <summary>
    // Reseta o estado do jogador, não altera posição
    /// </summary>

    public void Resetar() {
        playerVidas = 3;

        if (!rb.isKinematic) {
            rb.linearVelocity = Vector3.zero;
        }

        if (sendoCarregado) carregavel.carregador.Soltar(); // Se o jogador está sendo carregado, se solta
        if (carregando != null) carregador.Soltar(); // Se o jogador está carregando algo, se solta
        if (ferramenta != null) ferramenta.Cancelar(); // Se o jogador está acionando uma ferramenta, cancela a ação
        grudavel.Desgrudar();
        andador.DesgrudarDoChao();
        barcoEmbarcado = null;
        HandleEmbarcado();
    }

    string motivoDeDano = "Desconhecido";
    public void SetCausaDoDano(string causa) {
        motivoDeDano = causa;
    }

    public void HandleEmbarcado(){
        if(embarcado){
            col.excludeLayers |= (1 << LayerMask.NameToLayer("Gancho"));
            characterController.excludeLayers |= (1 << LayerMask.NameToLayer("Gancho"));
            if(ganchavel != null) ganchavel.enabled = false;
            if (carregando != null) carregador.Soltar();
            carregador.podeCarregar = false;
            
            velocidade = 0f;
            velocidadeRB = 0f;
        }
        else{
            col.excludeLayers &= ~(1 << LayerMask.NameToLayer("Gancho"));
            characterController.excludeLayers &= ~(1 << LayerMask.NameToLayer("Gancho"));
            if(ganchavel != null) ganchavel.enabled = true;
            carregador.podeCarregar = true;
            
            velocidade = 12f;
            velocidadeRB = 6.5f;
        }
    }

    


    #region Online

    [HideInInspector, SyncVar(hook = nameof(AtualizarStatusConectado))] public bool conectado = false;
    void AtualizarStatusConectado(bool oldValue, bool newValue) {
        if (!oldValue && newValue) {
            GameManager.instance.SetarOnline(this);
        } else {
            if (isLocalPlayer) GameManager.instance.VoltarParaMenu();
            else GameManager.instance.SoftResetSala();
        }
    }

    public override void OnStopClient (){
        base.OnStopClient();
        if (isLocalPlayer)
            GameManager.instance?.VoltarParaMenu();
    }

    public void OnTrocouAutoridade() {
        if (GameManager.instance.isOnline) {
            if (isLocalPlayer){
                GameManager.instance.SetarPlayerAtualOnline(qualPlayer);
            }

            qualPlayer = isLocalPlayer ? QualPlayer.Player1 : QualPlayer.Desativado;
        }
    }


    [Command]
    void AtualizarDirecaoCmd(Vector3 valor, bool isMira, bool estaMirando) {
        AtualizarDirecaoClientRpc(valor, isMira, estaMirando);
    }

    [ClientRpc]
    void AtualizarDirecaoClientRpc(Vector3 valor, bool isMira, bool estaMirando) {
        if (isLocalPlayer) return; // Não atualiza a direção do jogador local

        this.estaMirando = estaMirando;

        if (estaMirando || valor.magnitude != 0) {
            direcao = valor;
            visualizarDirecao.transform.forward = direcao;
        } else if (movimentacao.magnitude != 0) {
            direcao = movimentacao;
        }

        if (!isMira)
            animacaoJogador.Mover(direcao);
    }

    [Command]
    void AtualizarMovimentoCmd(Vector3 movimento) {
        AtualizarMovimentoClientRpc(movimento);
    }

    [ClientRpc]
    void AtualizarMovimentoClientRpc(Vector3 movimento) {
        if (isLocalPlayer) return; // Não atualiza a direção do jogador local

        movimentacao = movimento;

        if (!estaMirando && movimentacao.magnitude > 0) {
            direcao = movimentacao;
            visualizarDirecao.transform.forward = direcao;
        }

        animacaoJogador.Mover(movimentacao);
    }

    #endregion



    #region Ferramenta

    /// <summary>
    /// Chamado quando o botão de "ataque" é pressionado
    /// </summary>
    [Sincronizar]
    public void AcionarFerramenta() {
        gameObject.Sincronizar();
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Acionar();
    }

    void AcionarFerramenta(InputAction.CallbackContext ctx) {
        AcionarFerramenta();
    }

    /// <summary>
    /// Chamado quando o botão de "ataque" é solto
    /// </summary>
    [Sincronizar]
    public void SoltarFerramenta() {
        gameObject.Sincronizar();
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Soltar();
    }

    void SoltarFerramenta(InputAction.CallbackContext ctx) {
        SoltarFerramenta();
    }

    /// <summary>
    /// Chamado quando o jogador não pode estar com a ferramenta acionada porém está
    /// </summary>
    [Sincronizar]
    public void ResetarFerramenta() {
        if (!ferramenta.acionada) return;

        gameObject.Sincronizar();
        ferramenta.Cancelar();
    }

    public void LevantarEscudo(bool value) {
        animacaoJogador.AtivarEscudo(value);
    }

    public void AtirarGancho() {
        StartCoroutine(AtirandoGancho());
    }

    IEnumerator AtirandoGancho() {
        atirandoGancho = true;
        float timer = animacaoJogador.AtirarGancho();
        while(timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
        atirandoGancho = false;
    }

    /// <summary>
    /// Mostra ou esconde o indicador de direção (seta)
    /// Se mostrar, o jogador não pode se mover.
    /// </summary>
    /// <param name="mostrar">Se irá mostrar ou não</param>
    public void MostrarDirecional(bool mostrar) {
        visualizarDirecao.SetActive(mostrar);
    }

    /// <summary>
    /// Garante que o jogador será um Rigidbody ao ser puxado pelo gancho
    /// </summary>
    /// <returns></returns>
    public IEnumerator GanchavelAntesPuxar() {
        UsarRB(true);
        StartCoroutine(SendoPuxadoCoroutine());
        yield return new WaitForEndOfFrame();
    }

    IEnumerator SendoPuxadoCoroutine() {
        sendoPuxado = true;
        yield return new WaitForSeconds(0.1f); // Espera um pouco para garantir que deu tempo de sair do chão
        sendoPuxado = false;
    }


    #endregion



    #region Movimentacao

    public float coyote = 0.5f;
    float coyoteTimer = 0f;

    Vector3 ultimaPosicao;
    

    public System.Action<Vector3> OnPositionChange;


    /// <summary>
    /// Trata da movimentação do jogador
    /// </summary>
    //Titi: Fiz algumas alterações aqui na movimentação pro escudo ok :3
    void Movimentacao() {
        if (!GameManager.instance.isOnline || isLocalPlayer)
            CalcularDirecao();

        if (!usandoRb) {
            if (CheckEstaNoChao()) coyoteTimer = coyote;
            else coyoteTimer -= coyoteTimer >= 0 ? Time.deltaTime : 0;

            estaNoChao = /*!sendoPuxado &&*/ coyoteTimer > 0f;
        } else {
            coyoteTimer = 0;
            if(CheckEstaNoChao() != estaNoChao) {
                hitFloorEffect.Play();
                estaNoChao = CheckEstaNoChao();
            }
        }

        if(!estaNoChao) MovimentacaoNoAr();
        else MovimentacaoNoChao();

        UsarAtrito(estaNoChao);

        if (!GameManager.instance.isOnline || isLocalPlayer)
            animacaoJogador.Mover(movimentacao);
        
        Vector3 variacao = transform.position - ultimaPosicao;
        ultimaPosicao = transform.position;

        if (variacao.magnitude != 0f) OnPositionChange?.Invoke(variacao);
    }

    void CalcularDirecao() {
        if (playerInput == null || !playerInput.enabled) return;

        Vector2 input = playerInput.currentActionMap["Move"].ReadValue<Vector2>();

        if (GameManager.instance.isPaused) {
            input = Vector2.zero;
        }

        float x = input.x;
        float z = input.y;

        if (input.magnitude > 1) input = input.normalized;

        Vector3 ultimaMovimentacao = movimentacao;
        movimentacao = transform.right * x + transform.forward * z;

        if (!estaMirando && movimentacao.magnitude > 0) {
            direcao = movimentacao;
            visualizarDirecao.transform.forward = direcao;
        }

        if (ultimaMovimentacao != movimentacao && GameManager.instance.isOnline && isLocalPlayer)
            AtualizarMovimentoCmd(movimentacao);
    }

    float GetVelocidade(bool isRb = false) {
        float v = isRb ? velocidadeRB : velocidade;

        if (escudoAtivo) v *= velocidadeComEscudoMult;
        else if (carregando) v *= velocidadeCarregandoMult;
        if (ganchado) v *= velocidadeGanchadoMult;
        if (empurrando) v *= velocidadeEmpurrandoMult;

        return v;
    }

    public float tempoAteMovBase = 1f;
    float movGradual = 0f;


    public float noChaoTempoMin = 0.25f;
    float noChaoTimer = 0f;
    bool naoCairCC = false;
    [SerializeField] private VisualEffect dustVisualEffect;
    [SerializeField] private VisualEffect hitFloorEffect;
    
    // Chamado automaticamente pelo método Movimentacao
    void MovimentacaoNoChao() {
        UsarCC();
        if(atirandoGancho) {
            dustVisualEffect.SetFloat("Count", 0);
            return;
        }

        Vector3 movimentacaoEfetiva = Vector3.zero; 
        if (ehJogadorAtual && !sendoCarregado && podeMovimentar && movimentacao.magnitude > 0) {
            Vector3 mov = movimentacao * GetVelocidade();
            movGradual += Time.deltaTime;

            movimentacaoEfetiva += mov * Mathf.Min(1, movGradual/tempoAteMovBase);
        } else {
            movGradual = 0f;
        }
        
        // Se marcou que o player está no chão, começa um contador para parar de calcular a gravidade
        if (estaNoChao) {
            if (!naoCairCC) {
                noChaoTimer += Time.deltaTime;
                if (noChaoTimer > noChaoTempoMin) {
                    naoCairCC = true;
                }
            }
        } else {
            noChaoTimer = 0;
            naoCairCC = false;
        }
        
        
        if (!naoCairCC && !sendoCarregado) {
            movimentacaoEfetiva +=  Vector3.down * 9.81f; //Physics.gravity;
        }
            
        float velocidadeRelativa = 0.0f;
        if (movimentacaoEfetiva != Vector3.zero) {
            characterController.Move(movimentacaoEfetiva * Time.fixedDeltaTime);
            velocidadeRelativa = movimentacaoEfetiva.magnitude / GetVelocidade();
        }
        dustVisualEffect.SetFloat("Count", Mathf.Lerp(0, 1, velocidadeRelativa));
    }

    // Chamado automaticamente pelo método Movimentacao
    void MovimentacaoNoAr() {
        dustVisualEffect.SetFloat("Count", 0);
        UsarRB();

        if (!ehJogadorAtual || sendoCarregado || !podeMovimentar || movimentacao.magnitude == 0)  return;

        rb.AddForce(movimentacao.normalized * GetVelocidade(true) , ForceMode.Force);
    }

    public float aimSpeed = 3f;
    /// <summary>
    /// Código para fazer o player mirar a direção do escudo e gancho de forma separada da movimentação 
    /// </summary>
    void Mira()
    {
        if (playerInput == null || !playerInput.enabled) return;

        inputMira = playerInput.currentActionMap["Aim"].ReadValue<Vector2>();
        
        bool estavaMirando = estaMirando;
        estaMirando = inputMira.magnitude > deadzoneMira;

        if (estaMirando) {
            Vector3 novaDirecao = new Vector3(inputMira.x, 0, inputMira.y).normalized;
            bool houveMudanca = (direcao != novaDirecao) || (estavaMirando != estaMirando);

            if (novaDirecao.magnitude == 0) return;

            direcao = novaDirecao;
            visualizarDirecao.transform.forward = direcao;

            if (houveMudanca && GameManager.instance.isOnline && isLocalPlayer)
                AtualizarDirecaoCmd(direcao, true, estaMirando);
        } else if (GameManager.instance.isOnline && isLocalPlayer) {
            AtualizarDirecaoCmd(direcao, true, estaMirando);
        }
    }

    /// <summary>
    /// Coisas de knockback do player que foram mudadas 
    /// devido as complicações com a física atualmente 
    /// </summary>
    /// <param name="executaEmpurrar"></param>
    public void AplicarKnockback(Transform origem)
    {
        if (estaSofrendoKnockback) return;
        StartCoroutine(ProcessarKnockback(origem));
    }

    private IEnumerator ProcessarKnockback(Transform origem)
    {
        estaSofrendoKnockback = true;
        bool podiaSeMover = podeMovimentar;
        podeMovimentar = false;

        Vector3 direcao = (transform.position - origem.position).normalized;
        direcao.y = componenteVerticalKnockback;
        direcao.Normalize();

        float tempo = 0f;
        Vector3 velocidadeInicial = direcao * forcaKnockback;
        Vector3 velocidadeFinal = Vector3.zero;

        while (tempo < duracaoKnockback)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracaoKnockback;
            Vector3 velocidadeAtual = Vector3.Lerp(velocidadeInicial, velocidadeFinal, progresso);


            if (characterController.enabled) {
                characterController.Move(velocidadeAtual * Time.deltaTime);
            }
            yield return null;
        }

        podeMovimentar = podiaSeMover;
        estaSofrendoKnockback = false;
    }

    bool usandoRb = true;
    public void UsarRB(bool ignorarChecagem = false) {
        if (!ignorarChecagem && usandoRb) return; // Se já está usando o Rigidbody, não faz nada

        col.enabled = true;
        characterController.enabled = false; // Desabilita o CharacterController para evitar colisões
        rb.isKinematic = false; // Habilita o Rigidbody para permitir a física

        CalcularDirecao();
        rb.linearVelocity = movimentacao * velocidadeRB;
        
        usandoRb = true;
    }

    public void UsarCC(bool ignorarChecagem = false) {
        if (!ignorarChecagem && !usandoRb) return; // Se não está usando o Rigidbody, não faz nada

        col.enabled = false;
        characterController.enabled = true; // Habilita o CharacterController novamente
        rb.linearVelocity = Vector3.zero; // Zera a velocidade do Rigidbody
        rb.isKinematic = true; // Desabilita o Rigidbody para evitar a física

        noChaoTimer = 0;
        naoCairCC = false;

        usandoRb = false;
    }

    public bool CheckEstaNoChao() {
        if (sendoPuxado) return false; // Se o jogador está sendo puxado, não está no chão
        return andador.ChecarChao();
    }
    
    public void Teletransportar(Vector3 posicao) {
        if (!usandoRb) characterController.enabled = false; // Desabilita o CharacterController para evitar colisões
        transform.position = posicao;
        if (!usandoRb) characterController.enabled = true; // Habilita o CharacterController novamente
        ultimaPosicao = transform.position;
    }

    public void Teletransportar(Transform posicao) {
        Teletransportar(posicao.position);
    }

    // CODIGO DO LIMA TA AQUI OH BRIGA COM ELE :

    [SerializeField] private PhysicsMaterial matCAtrito, matSAtrito;
    private bool atrito = true;
    
    private void UsarAtrito(bool val){
        if(val == atrito) return;

        if(val == true){
            col.material = matCAtrito;
        }
        else{
            col.material = matSAtrito;
        }

        atrito = val;
    }

    #endregion



    #region Interacao

    Dictionary<Collider, InteragivelBase> cache_interagiveisProximos = new Dictionary<Collider, InteragivelBase>();
    List<Collider> removerDoCacheDeInteragiveis = new List<Collider>(); // Lista de colisores que não estão mais na área de interação (para remover do cache)

    /// <summary>
    /// Remove colisores que não podem ser interagidos (ex: o próprio jogador) da lista de colisores interagíveis
    /// No lugar do colisor, coloca null
    /// </summary>
    /// <param name="colliders"></param>
    /// <returns>Retorna a quantidade de itens que não foram removidos</returns>
    int RemoveColisoresIgnoraveis(Collider[] colliders) {
        int quant = 0;

        for (int i = 0; i < colliders.Length; i++) {
            if (collidersIgnoraveis.Contains(colliders[i]) || colliders[i] == null) {
                colliders[i] = null;
            } else {
                quant++;
            }
        }

        return quant;
    }

    /// <summary>
    /// Utilizado únicamente dentro da função 'ChecarInteragiveis' para manter cache de Collider/Interagivel, evitando a utilização de GetComponent a cada frame
    /// </summary>
    /// <param name="collider"></param>
    /// <returns>O componente Interagivel guardado em cache</returns>
    InteragivelBase PegaInteragivelDoCache(Collider collider) {
        if(!cache_interagiveisProximos.ContainsKey(collider)){
            // Se não está no cache, coloca no cache
            InteragivelBase interagivelAtual = collider.GetComponent<InteragivelBase>();

            if (interagivelAtual == null) return null;
            cache_interagiveisProximos.Add(collider, interagivelAtual);
        } else {
            removerDoCacheDeInteragiveis.Remove(collider);
        }

        return cache_interagiveisProximos[collider];
    }

    /// <summary>
    ///  Utilizado únicamente dentro da função 'ChecarInteragiveis', em conjunto com 'PegaInteragivelDoCache' serve para limpar colisores não necessários no cache
    /// </summary>
    void AtualizarCacheDeInteragivel() {
        // Remove os colisores que não estão mais na área de interação do cache
        foreach (var colisor in removerDoCacheDeInteragiveis) {
            cache_interagiveisProximos.Remove(colisor);
        }

        removerDoCacheDeInteragiveis = cache_interagiveisProximos.Keys.ToList();
    }


    /// <summary>
    /// Checa por objetos interagíveis no raio de interação e define o interagível mais próximo em "ultimoInteragivel"
    /// </summary>
    /// <returns>Retorna verdadeiro caso tenha um interagivel próximo ao jogador</returns>
    public bool ChecarInteragiveis() {
        // Checa por objetos interagíveis no raio de interação
        int quant = Physics.OverlapSphereNonAlloc(transform.position, raioInteracao, collidersInteragiveis, layerInteragivel);
        int quantFiltrada = RemoveColisoresIgnoraveis(collidersInteragiveis);

        // Na maioria das vezes, não haverá interagíveis
        if (quantFiltrada == 0) {
            if (ultimoInteragivel != null) ultimoInteragivel.MostrarIndicador(false, indicador);
            ultimoInteragivel = null;
            cache_interagiveisProximos.Clear();
            return false;
        }

        // Procura o interagível mais próximo (não podemos confiar na ordem padrão dos colliders)
        float menorDistancia = Mathf.Infinity;
        InteragivelBase interagivelMaisProximo = null;
        for (int i = 0; i < quant; i++) { // Na maioria das vezes, só haverá um interagível, e se houver mais, não será muitos (menos de 8)
            Collider collider = collidersInteragiveis[i];
            if (collider == null) continue; // Ignora objetos removidos

            InteragivelBase interagivelAtual = PegaInteragivelDoCache(collider);
            if (interagivelAtual == null || !interagivelAtual.PodeInteragir(this) || !interagivelAtual.enabled || !PodeInteragir(interagivelAtual, collider)) {
                //if (interagivelAtual != null)
                //    Debug.Log("Barrado " + interagivelAtual.name + " pois: " + !interagivelAtual.PodeInteragir(this)  + " " +  !interagivelAtual.enabled  + " " +  !PodeInteragir(interagivelAtual, collider));
                
                continue; // Ignora objetos removidos ou sem o componente Interagivel
            }

            float distancia = Vector3.Distance(transform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                menorDistancia = distancia;
                interagivelMaisProximo = interagivelAtual;
            }
        }

        AtualizarCacheDeInteragivel();

        if (interagivelMaisProximo == null) {
            if (ultimoInteragivel != null) ultimoInteragivel.MostrarIndicador(false, indicador);
            ultimoInteragivel = null;
            return false;
        }

        // Trata do ultimo interagivel
        if (ultimoInteragivel != null) {
            if (ultimoInteragivel == interagivelMaisProximo) return true; // Se o interagível mais próximo for o mesmo que o último interagível, não faz nada
            ultimoInteragivel.MostrarIndicador(false, indicador);
        }

        MotivoNaoInteracao motivo = interagivelMaisProximo.NaoPodeInteragirPois(this);


        // GAMBIARRA DO LIMA:
        try { interagivelMaisProximo.MostrarIndicador(true, indicador, motivo); } catch { }


        ultimoInteragivel = interagivelMaisProximo;

        return true;
    }


    public virtual bool PodeInteragir(InteragivelBase interagivel, Collider collider) {
        return !embarcado || !interagivel.gameObject.CompareTag("Player");
    }


    /// <summary>
    /// Interage com o objeto mais próximo (definido em "ultimoInteragivel")
    /// </summary>
    void Interagir() {
        // Prioriza interações ao invés de soltar o que carrega (caso a interação necessite de um objeto carregado)
        if (ultimoInteragivel != null) InteragirCom(ultimoInteragivel.gameObject);
        else if (carregador.estaCarregando) SoltarCarregando();
    }

    void Interagir(InputAction.CallbackContext ctx) {
        Interagir();
    }

    [Sincronizar]
    public void InteragirCom(GameObject interagivelObj) {
        InteragivelBase interagivel = interagivelObj.GetComponent<InteragivelBase>();
        if (interagivel == null) return;
        gameObject.Sincronizar(interagivelObj);

        ultimoInteragivel = interagivel;
        interagivel.Interagir(this);
    }

    [Sincronizar]
    public void SoltarCarregando() {
        if (!carregador.estaCarregando) return;
        gameObject.Sincronizar();

        carregador.Soltar(direcao.normalized, velocidade, movimentacao.magnitude > 0);
    }

    #endregion



    #region Trajetoria

    [Header("Trajetória do Arremesso")]
    public LineRenderer linhaTrajetoria;
    public Transform pontoFinalTrajetoria;
    public LayerMask layerTrajetoria;
    public SpriteRenderer spriteTrajetoriaFinal;
    public float baseScaleTrajetoriaFinal, selectedScaleTrajetoriaFinal;
    public Color baseColorTrajetoriaFinal, selectedColorTrajetoriaFinal;

    public void DesenharTrajetoria() {
        if (carregador == null || !carregador.estaCarregando) {
            if (linhaTrajetoria.enabled) linhaTrajetoria.enabled = false; // Desabilita a linha se não estiver carregando
            pontoFinalTrajetoria.gameObject.SetActive(false);
            return;
        }

        Vector3 direcaoArremesso = direcao.normalized;
        direcaoArremesso.y = carregador.alturaArremesso;
        Vector3 velocidadeInicial = Vector3.zero;

        if (movimentacao.magnitude > 0)
            velocidadeInicial = carregador.influenciaDaInerciaNoArremesso * (direcao.normalized * velocidade);

        linhaTrajetoria.positionCount = 0; // Limpa a linha anterior

        Vector3[] pontos = carregador.PreverArremesso(carregador.carregadoRigidbody, direcaoArremesso, carregador.forcaArremesso, velocidadeInicial, layer: layerTrajetoria, comecarPor: carregador.carregarTransform.position);
        if (pontos == null) {
            linhaTrajetoria.enabled = false; // Desabilita a linha se não houver pontos
            pontoFinalTrajetoria.gameObject.SetActive(false);
            return;
        }

        linhaTrajetoria.enabled = true; // Habilita a linha
        linhaTrajetoria.positionCount = pontos.Length;
        linhaTrajetoria.SetPositions(pontos);

        Vector3 pontoFinal = pontos[pontos.Length - 1];
        SetPontoFinal(true, pontoFinal);
    }
    
    public void SetPontoFinal(bool ativo, Vector3 posicao = default, bool encontrou = false) {
        pontoFinalTrajetoria.gameObject.SetActive(ativo);
        if (ativo) {
            pontoFinalTrajetoria.position = posicao;
            if (encontrou) {
                spriteTrajetoriaFinal.color = selectedColorTrajetoriaFinal;
                spriteTrajetoriaFinal.transform.localScale = Vector3.one * selectedScaleTrajetoriaFinal;
            } else {
                spriteTrajetoriaFinal.color = baseColorTrajetoriaFinal;
                spriteTrajetoriaFinal.transform.localScale = Vector3.one * baseScaleTrajetoriaFinal;
            }
        } else {
            spriteTrajetoriaFinal.color = baseColorTrajetoriaFinal;
            spriteTrajetoriaFinal.transform.localScale = Vector3.one * baseScaleTrajetoriaFinal;
        }
    }

    #endregion



    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioInteracao);

        // Direção
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direcao.normalized * 3);
    }

}

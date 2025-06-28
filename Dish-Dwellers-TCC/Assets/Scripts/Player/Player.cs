using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using Mirror;

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
    public LayerMask layerChao;
    public float distanciaCheckChao = 0.5f;
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

            if (_playerVidas == 0){
                Morrer();
            }
        }
    }
    public static event UnityAction<Player, int> OnVidaMudada; //Evento global para dano ou ganhar vida



    [Header("Configuração de Interação")]
    public int maxInteragiveisEmRaio = 8;
    public float raioInteracao = 1f;
    public LayerMask layerInteragivel;
    public float velocidadeCarregandoMult = 0.85f;
    Interagivel ultimoInteragivel;
    Collider[] collidersInteragiveis;
    public List<Collider> collidersIgnoraveis = new List<Collider>(); // Lista de colisores que o jogador não pode interagir



    [Header("Referências")]
    public GameObject visualizarDirecao;
    public Transform pontoCentral; // pros bicho mirar certo e não atirarem no pé.
    bool podeMovimentar = true; // Solução TEMPORARIA enquanto não há estados implementados
    


    // Titizim:
    [Header("Config do Escudo")] [Space(10)]
    public bool escudo;
    public bool escudoAtivo { get; set; }
    public float velocidadeComEscudoMult = 0.65f;

    [Header("Config de Mira")] [Space(10)]
    public float velocidadeGanchadoMult = 0.75f;
    public bool estaMirando = false;
    private Vector2 inputMira;
    public float deadzoneMira = 0.1f; // Zona morta para evitar mira acidental

    [Header("Configurações de Knockback")] [Space(10)]
    public float forcaKnockback = 5f;
    public float duracaoKnockback = 0.3f;
    public float componenteVerticalKnockback = 0.2f;
    private bool estaSofrendoKnockback = false;

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


    // Awake: trata de referências/configurações internas
    void Awake() {
        collidersInteragiveis = new Collider[maxInteragiveisEmRaio];

        direcao = new Vector3(0, 0, -1);
        offsetCheckChao = new Vector3(0, distanciaCheckChao, 0); // Offset para o raycast de verificação do chão

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        characterController = GetComponent<CharacterController>();
        carregador = GetComponent<Carregador>();
        carregavel = GetComponent<Carregavel>();
        ganchavel = GetComponent<Ganchavel>();
        ferramenta = GetComponentInChildren<Ferramenta>();
        ferramenta.Inicializar(this);

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
    }

    public void OnInputTriggered(InputAction.CallbackContext ctx, QualPlayer qualPlayer) {
        if (qualPlayer != this.qualPlayer) return;
        if (!ehJogadorAtual) return; // Se não é o jogador atual, não faz nada

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
            ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
        }

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
        // No modo singleplayer, caso este jogador não seja o atual, não faz nada
        if (GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && (playerInput == null || !playerInput.enabled)) {
            if (ultimoInteragivel != null) {
                ultimoInteragivel.MostarIndicador(false);
                ultimoInteragivel = null;
            }

            movimentacao = Vector3.zero;
            animacaoJogador.Mover(movimentacao);
        }

        if (ehJogadorAtual) ChecarInteragiveis();
        Movimentacao();

        DesenharTrajetoria();
    }


    /// <summary>
    /// Aumenta ou diminui a vida do jogador
    /// </summary>
    /// <param name="valor">Valor a ser adicionado ou subtraído da vida do jogador</param>
    public void MudarVida(int valor){
        playerVidas += valor;
    }

    /// <summary>
    /// Chamado quando o jogador morre
    /// </summary>
    [Sincronizar]
    public void Morrer() {
        gameObject.Sincronizar();
        GameManager.instance.ResetSala();
        Debug.Log("morreu");
    }

    public void Resetar() {
        MudarVida(3);

        if (sendoCarregado) carregavel.carregador.Soltar(); // Se o jogador está sendo carregado, se solta
        if (carregando != null) carregador.Soltar(); // Se o jogador está carregando algo, se solta
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Cancelar(); // Se o jogador está acionando uma ferramenta, cancela a ação 
    }


    #region Online

    [HideInInspector, SyncVar(hook = nameof(AtualizarStatusConectado))] public bool conectado = false;
    void AtualizarStatusConectado(bool oldValue, bool newValue) {
        if (isLocalPlayer) {
            if (!oldValue && newValue) {
                GameManager.instance.ComecarOnline();
            } else {
                GameManager.instance.VoltarParaMenu();
            }
        }
    }

    public override void OnStopClient (){
        base.OnStopClient();
        GameManager.instance?.VoltarParaMenu();
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

    /// <summary>
    /// Mostra ou esconde o indicador de direção (seta)
    /// Se mostrar, o jogador não pode se mover.
    /// </summary>
    /// <param name="mostrar">Se irá mostrar ou não</param>
    public void MostrarDirecional(bool mostrar) {
        visualizarDirecao.SetActive(mostrar);
        if (personagem == QualPersonagem.Heater) return;
        podeMovimentar = !mostrar;
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
            estaNoChao = CheckEstaNoChao();
        }

        if(!estaNoChao) MovimentacaoNoAr();
        else MovimentacaoNoChao();

        UsarAtrito(estaNoChao);

        if (!GameManager.instance.isOnline || isLocalPlayer)
            animacaoJogador.Mover(movimentacao);
    }

    void CalcularDirecao() {
        if (playerInput == null || !playerInput.enabled) return;

        Vector2 input = playerInput.currentActionMap["Move"].ReadValue<Vector2>();
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

        return v;
    }

    public float tempoAteMovBase = 1f;
    float movGradual = 0f;


    public float noChaoTempoMin = 0.25f;
    float noChaoTimer = 0f;
    bool naoCairCC = false;
    // Chamado automaticamente pelo método Movimentacao
    void MovimentacaoNoChao() {
        UsarCC();

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
            
        
        if (movimentacaoEfetiva != Vector3.zero) {
            characterController.Move(movimentacaoEfetiva * Time.fixedDeltaTime);
        }
    }

    // Chamado automaticamente pelo método Movimentacao
    void MovimentacaoNoAr() {
        UsarRB();

        if (!ehJogadorAtual || sendoCarregado || !podeMovimentar || movimentacao.magnitude == 0)  return;

        rb.AddForce(movimentacao.normalized * GetVelocidade(true) , ForceMode.Force);
    }

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
    public void AplicarKnockback(Transform origem, AudioClip soundFx)
    {
        if (estaSofrendoKnockback) return;
        AudioSource audioSource = GetComponentInChildren<AudioSource>();
        audioSource.clip = soundFx;
        audioSource.Play();
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
            
            characterController.Move(velocidadeAtual * Time.deltaTime);
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

    Vector3 offsetCheckChao; // Definido no Awake, mas em suma, é o mesmo valor de distanciaCheckChao, mas com Y positivo (assim a checagem não começa no pé do jogador)
    public bool CheckEstaNoChao() {
        if (sendoPuxado) return false; // Se o jogador está sendo puxado, não está no chão
        return Physics.Raycast(transform.position + offsetCheckChao, Vector3.down, 2 * distanciaCheckChao, layerChao);
    }

    public void Teletransportar(Vector3 posicao) {
        if (!usandoRb) characterController.enabled = false; // Desabilita o CharacterController para evitar colisões
        transform.position = posicao;
        if (!usandoRb) characterController.enabled = true; // Habilita o CharacterController novamente
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

    [System.Serializable]
    public class CacheColliderInteragivel {
        public Interagivel interagivel;
        public bool checado = false;

        public CacheColliderInteragivel(Interagivel interagivel = null, bool checado = false) {
            this.interagivel = interagivel;
            this.checado = checado;
        }
    }

    Dictionary<Collider, CacheColliderInteragivel> cache_interagiveisProximos = new Dictionary<Collider, CacheColliderInteragivel>();
    List<Collider> removerDoCacheDeInteragiveis = new List<Collider>(); // Lista de colisores que não estão mais na área de interação (para remover do cache)

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
            if (ultimoInteragivel != null) ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
            cache_interagiveisProximos.Clear();
            return false;
        }

        foreach (var cache in cache_interagiveisProximos) {
            cache.Value.checado = false;
        }

        // Procura o interagível mais próximo (não podemos confiar na ordem padrão dos colliders)
        float menorDistancia = Mathf.Infinity;
        Interagivel interagivelMaisProximo = null;
        for (int i = 0; i < quant; i++) { // Na maioria das vezes, só haverá um interagível, e se houver mais, não será muitos (menos de 8)
            Collider collider = collidersInteragiveis[i];
            if (collider == null) continue; // Ignora objetos removidos


            // Salva o colisor em um cache referenciado-o ao seu Interagivel (evitar GetComponent toda vez)
            CacheColliderInteragivel cache;
            if(!cache_interagiveisProximos.ContainsKey(collider)){ // Se não está no cache, cria um cache
                Interagivel interagivel = collider.GetComponent<Interagivel>();
                if (interagivel == null) continue; // Ignora objetos sem o componente Interagivel

                cache = new CacheColliderInteragivel(interagivel);
                cache_interagiveisProximos.Add(collider, cache);
            } else { // Se já está no cache, pega o cache
                cache = cache_interagiveisProximos[collider];
            }

            cache.checado = true; // Marca o colisor como checado, ou seja, ela ainda se encontra na area
            Interagivel interagivelAtual = cache.interagivel; // Pega o interagível do cache

            if (interagivelAtual == null) continue; // Ignora objetos removidos ou sem o componente Interagivel

            float distancia = Vector3.Distance(transform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                menorDistancia = distancia;
                interagivelMaisProximo = interagivelAtual;
            }
        }

        // Remove os colisores que não estão mais na área de interação do cache
        foreach (var cache in cache_interagiveisProximos) {
            if (!cache.Value.checado) { // Se o colisor não foi checado, significa que ele não está mais na área de interação
                removerDoCacheDeInteragiveis.Add(cache.Key);
            }
        }

        // Remove os colisores que não estão mais na área de interação do cache
        foreach (var colisor in removerDoCacheDeInteragiveis) {
            cache_interagiveisProximos.Remove(colisor);
        }

        removerDoCacheDeInteragiveis.Clear(); // Limpa a lista de colisores que não estão mais na área de interação


        if (interagivelMaisProximo == null) {
            if (ultimoInteragivel != null) ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
            return false;
        }

        // Trata do ultimo interagivel
        if (ultimoInteragivel != null) {
            if (ultimoInteragivel == interagivelMaisProximo) return true; // Se o interagível mais próximo for o mesmo que o último interagível, não faz nada
            ultimoInteragivel.MostarIndicador(false);
        }


        // GAMBIARRA DO LIMA:
        try { interagivelMaisProximo.MostarIndicador(true); } catch { }


        ultimoInteragivel = interagivelMaisProximo;

        return true;
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
        Interagivel interagivel = interagivelObj.GetComponent<Interagivel>();
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

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioInteracao);

        // Direção
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direcao.normalized * 3);
    }

    [Header("Trajetória do Arremesso")]
    public LineRenderer linhaTrajetoria;
    public Transform pontoFinalTrajetoria;
    public LayerMask layerTrajetoria;

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
        pontoFinalTrajetoria.position = pontoFinal;
        pontoFinalTrajetoria.gameObject.SetActive(true);
    }
    
    public void SetPontoFinal(bool ativo, Vector3 posicao = default) {
        pontoFinalTrajetoria.gameObject.SetActive(ativo);
        if (ativo) pontoFinalTrajetoria.position = posicao;
    }
}

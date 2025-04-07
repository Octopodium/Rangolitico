using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // Titi: Adição temporaria pra reset de sala
using System.Collections.Generic;
using Mirror;

public enum QualPlayer { Player1, Player2, Desativado }
public enum QualPersonagem { Heater, Angler }

[RequireComponent(typeof(Carregador)), RequireComponent(typeof(Carregavel))]
public class Player : NetworkBehaviour {
    public QualPersonagem personagem = QualPersonagem.Heater;
    public QualPlayer qualPlayer = QualPlayer.Player1;
    public InputActionMap inputActionMap {get; protected set;}



    [Header("Atributos do Player")]
    public float velocidade = 6f;
    [HideInInspector] public Vector3 direcao, mira, movimentacao; // Direção que o jogador está olhando e movimentação atual (enquanto anda direcao = movimentacao)
    private int _playerVidas = 3;
    public int playerVidas {
        get { return _playerVidas; }
        set {
            _playerVidas = Mathf.Clamp(value, 0, 3);
            OnVidaMudada?.Invoke(this, _playerVidas);

            if (_playerVidas == 0){
                //SceneManager.LoadScene.GameManager.instance.sala.scene.name;
                GameManager.instance.ResetSala();
                Debug.Log("morreu");
            }
        }
    }
    public static event UnityAction<Player, int> OnVidaMudada; //Evento global para dano ou ganhar vida



    [Header("Configuração de Interação")]
    public int maxInteragiveisEmRaio = 8;
    public float raioInteracao = 1f;
    public LayerMask layerInteragivel;
    Interagivel ultimoInteragivel;
    Collider[] collidersInteragiveis;
    public List<Collider> collidersIgnoraveis = new List<Collider>(); // Lista de colisores que o jogador não pode interagir



    [Header("Referências")]
    public GameObject visualizarDirecao;
    bool podeMovimentar = true; // Solução TEMPORARIA enquanto não há estados implementados
    


    // Titizim:
    [Header("Config do Escudo")]
    public bool escudoAtivo {get; set;}
    public float velocidadeComEscudo = 4f;



    // Referências internas
    public Ferramenta ferramenta;
    [HideInInspector] public Carregador carregador; // O que permite o jogador carregar coisas
    public Carregavel carregando => carregador.carregado; // O que o jogador está carregando
    Carregavel carregavel; // O que permite o jogador a ser carregado
    public bool sendoCarregado => carregavel.sendoCarregado; // Se o jogador está sendo carregado
    Rigidbody playerRigidbody;
    AnimadorPlayer animacaoJogador;



    // Awake: trata de referências/configurações internas
    void Awake() {
        collidersInteragiveis = new Collider[maxInteragiveisEmRaio];

        playerRigidbody = GetComponent<Rigidbody>();
        carregador = GetComponent<Carregador>();
        carregavel = GetComponent<Carregavel>();
        ferramenta = GetComponentInChildren<Ferramenta>();
        ferramenta.Inicializar(this);

        animacaoJogador = GetComponentInChildren<AnimadorPlayer>();

        collidersIgnoraveis.Add(GetComponent<Collider>());

        // Se está carregando algo, ignora a interação com esta coisa
        carregador.OnCarregar += (carregavel) => { if (carregavel != null)  collidersIgnoraveis.Add(carregavel.GetComponent<Collider>()); };
        carregador.OnSoltar += (carregavel) => { if (carregavel != null)  collidersIgnoraveis.Remove(carregavel.GetComponent<Collider>()); };

        // Se o jogador está sendo carregado, ignora a interação com o carregador
        carregavel.OnCarregado += (carregador) => { if (carregador != null)  collidersIgnoraveis.Add(carregador.GetComponent<Collider>()); };
        carregavel.OnSolto += (carregador) => {  if (carregador != null)  collidersIgnoraveis.Remove(carregador.GetComponent<Collider>()); };
    }

    // Start: trata de referências/configurações externas
    void Start() {
        if (GameManager.instance.isOnline) {
            if (isLocalPlayer){
                GameManager.instance.SetarPlayerAtualOnline(qualPlayer);
            }

            qualPlayer = isLocalPlayer ? QualPlayer.Player1 : QualPlayer.Desativado;
        }
        
        inputActionMap = GameManager.instance.GetPlayerInput(qualPlayer);

        if (!GameManager.instance.isOnline) {
            inputActionMap["Interact"].performed += Interagir;
            inputActionMap["Attack"].performed += ctx => AcionarFerramenta();
            inputActionMap["Attack"].canceled += ctx => SoltarFerramenta();
        } else if (isLocalPlayer) {
            inputActionMap["Interact"].performed += ctx => InteragirOnlineCmd();
            inputActionMap["Attack"].performed += ctx => AcionarFerramentaOnlineCmd();
            inputActionMap["Attack"].canceled += ctx => SoltarFerramentaOnlineCmd();
        }
    }

    void OnEnable() {
        if (GameManager.instance != null && GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && inputActionMap != null && inputActionMap.enabled) {
            GameManager.instance.TrocarControleSingleplayer(qualPlayer);
        }

        if (inputActionMap != null) inputActionMap.Enable();
    }

    void OnDisable(){
        /*
        if (inputActionMap != null) {
            inputActionMap["Interact"].performed -= Interagir;
            inputActionMap["Attack"].performed -= AcionarFerramenta;
            inputActionMap["Attack"].canceled -= SoltarFerramenta;
        }
        */

        if (ultimoInteragivel != null) {
            ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
        }

        if (GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && inputActionMap.enabled) {
            GameManager.instance.TrocarControleSingleplayer();
        }
    }

    void FixedUpdate() {
        if (GameManager.instance.isOnline && !isLocalPlayer) return;

        // No modo singleplayer, caso este jogador não seja o atual, não faz nada
        if (GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER && !inputActionMap.enabled) {
            if (ultimoInteragivel != null) {
                ultimoInteragivel.MostarIndicador(false);
                ultimoInteragivel = null;
            }

            return;
        }

        ChecarInteragiveis();
        Movimentacao();
    }


    /// <summary>
    /// Aumenta ou diminui a vida do jogador
    /// </summary>
    /// <param name="valor">Valor a ser adicionado ou subtraído da vida do jogador</param>
    public void MudarVida(int valor){
        playerVidas += valor;
    }



    #region Online

    [HideInInspector, SyncVar(hook=nameof(AtualizarStatusConectado))] public bool conectado = false;
    void AtualizarStatusConectado(bool oldValue, bool newValue) {
        if (isLocalPlayer) {
            if (!oldValue && newValue) {
                GameManager.instance.ComecarOnline();
            } else {
                GameManager.instance.VoltarParaMenu();
            }
        }
    }


    [Command]
    void AtualizarDirecaoCmd(Vector3 valor) {
        AtualizarDirecaoClientRpc(valor);
    }

    [ClientRpc]
    void AtualizarDirecaoClientRpc(Vector3 valor) {
        if (isLocalPlayer) return; // Não atualiza a direção do jogador local

        direcao = valor;
        visualizarDirecao.transform.forward = direcao;
    }


    // Respota para inputs na versão Online (é uma gambiarra que funciona)

    [Command]
    void InteragirOnlineCmd() {
        InteragirOnlineClientRpc();
    }

    [ClientRpc]
    void InteragirOnlineClientRpc() {
        ChecarInteragiveis();
        Interagir();
    }

    [Command]
    void AcionarFerramentaOnlineCmd() {
        AcionarFerramentaOnlineClientRpc();
    }

    [ClientRpc]
    void AcionarFerramentaOnlineClientRpc() {
        AcionarFerramenta();
    }

    [Command]
    void SoltarFerramentaOnlineCmd() {
        SoltarFerramentaOnlineClientRpc();
    }

    [ClientRpc]
    void SoltarFerramentaOnlineClientRpc() {
        SoltarFerramenta();
    }

    #endregion



    #region Ferramenta

    /// <summary>
    /// Chamado quando o botão de "ataque" é pressionado
    /// </summary>
    void AcionarFerramenta() {
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Acionar();
    }

    void AcionarFerramenta(InputAction.CallbackContext ctx) {
        AcionarFerramenta();
    }

    /// <summary>
    /// Chamado quando o botão de "ataque" é solto
    /// </summary>
    void SoltarFerramenta() {
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Soltar();
    }

    void SoltarFerramenta(InputAction.CallbackContext ctx) {
        SoltarFerramenta();
    }

    #endregion



    #region Movimentacao
    /// <summary>
    /// Trata da movimentação do jogador
    /// </summary>
    
    //Titi: Fiz algumas alterações aqui na movimentação pro escudo ok :3
    void Movimentacao() {
        Vector2 input = inputActionMap["Move"].ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;

        movimentacao = (transform.right * x + transform.forward * z).normalized;
        CalculaDirecao(movimentacao);

        if(sendoCarregado) return;

        // Descomente esse trecho para o player se mover enquanto levanta o escudo.
        /*
        if (escudoAtivo) 
        {
            if (movimentacao.magnitude > 0) 
            {   
                playerRigidbody.MovePosition(transform.position + movimentacao * (velocidade * 0.3f) * Time.fixedDeltaTime);
            }
            animacaoJogador.Mover(movimentacao * 0.3f);
        }
        */
        else if (podeMovimentar)  
        {
            playerRigidbody.MovePosition(transform.position + movimentacao * velocidade * Time.fixedDeltaTime);
            animacaoJogador.Mover(movimentacao);            
        }
    }

    private void CalculaDirecao(Vector3 movimentacao){
        if (movimentacao.magnitude > 0) {
            direcao = movimentacao;
            visualizarDirecao.transform.forward = direcao;

            if (GameManager.instance.isOnline && isLocalPlayer) AtualizarDirecaoCmd(direcao);
        }
    }

    public bool estaNoChao {
        get { return Physics.Raycast(transform.position, Vector3.down, 1.1f); }
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
            if (collidersIgnoraveis.Contains(colliders[i])) {
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
        try{interagivelMaisProximo.MostarIndicador(true);}
        catch{}


        ultimoInteragivel = interagivelMaisProximo;

        return true;
    }

    /// <summary>
    /// Interage com o objeto mais próximo (definido em "ultimoInteragivel")
    /// </summary>
    void Interagir() {
        // Prioriza interações ao invés de soltar o que carrega (caso a interação necessite de um objeto carregado)
        if (ultimoInteragivel != null) ultimoInteragivel.Interagir(this);
        else if (carregador.estaCarregando) carregador.Soltar(direcao, velocidade, movimentacao.magnitude > 0);
    }

    void Interagir(InputAction.CallbackContext ctx){
        Interagir();
    }

    #endregion



    /// <summary>
    /// Mostra ou esconde o indicador de direção (seta)
    /// Se mostrar, o jogador não pode se mover.
    /// </summary>
    /// <param name="mostrar">Se irá mostrar ou não</param>
    public void MostrarDirecional(bool mostrar) {
        visualizarDirecao.SetActive(mostrar);
        podeMovimentar = !mostrar;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioInteracao);
        
        // Direção
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direcao * 3);


        // Previsão arremeresso (Carregador)
        if (carregador != null && carregador.estaCarregando) {
            Gizmos.color = Color.blue;
            Vector3 direcaoArremesso = direcao;
            direcaoArremesso.y = carregador.alturaArremesso;
            Vector3 velocidadeInicial = Vector3.zero;

            if (movimentacao.magnitude > 0)
                velocidadeInicial = carregador.influenciaDaInerciaNoArremesso * (direcao * velocidade);

            Vector3[] pontos = carregador.PreverArremesso(carregador.carregado.GetComponent<Rigidbody>(), direcaoArremesso, carregador.forcaArremesso, velocidadeInicial);
            for (int i = 0; i < pontos.Length - 1; i++) {
                Gizmos.DrawLine(pontos[i], pontos[i + 1]);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public enum QualPlayer { Player1, Player2 }

[RequireComponent(typeof(Carregador)), RequireComponent(typeof(Carregavel))]
public class Player : MonoBehaviour {

    public QualPlayer qualPlayer = QualPlayer.Player1;
    InputActionMap inputActionMap;


    [Header("Atributos do Player")]
    public float velocidade = 6f;
    public Vector3 direcao, movimentacao; // Direção que o jogador está olhando e movimentação atual (enquanto anda direcao = movimentacao)
    private int _playerVidas = 3;
    public int playerVidas {
        get { return _playerVidas; }
        set {
            _playerVidas = Mathf.Clamp(value, 0, 3);
            OnVidaMudada?.Invoke(this, _playerVidas);
        }
    }

    public event UnityAction<Player, int> OnVidaMudada;

    [Header("Configuração de Interação")]
    public int maxInteragiveisEmRaio = 8;
    public float raioInteracao = 1f;
    public LayerMask layerInteragivel;
    Interagivel ultimoInteragivel;
    Collider[] collidersInteragiveis;

    [Header("Referências")]
    public GameObject visualizarDirecao;
    bool podeMovimentar = true; // Solução TEMPORARIA enquanto não há estados implementados


    // Referências internas
    public Ferramenta ferramenta;
    [HideInInspector] public Carregador carregador; // O que permite o jogador carregar coisas
    public Carregavel carregando => carregador.carregado; // O que o jogador está carregando
    Carregavel carregavel; // O que permite o jogador a ser carregado
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

        OnVidaMudada += NotificarUI;

    }

    // Start: trata de referências/configurações externas
    void Start() {
        inputActionMap = qualPlayer == QualPlayer.Player1 ? GameManager.instance.input.Player.Get() : GameManager.instance.input.Player2.Get();
        inputActionMap["Interact"].performed += ctx => Interagir();
        inputActionMap["Attack"].performed += ctx => AcionarFerramenta();
    }

    //OnDestroy apenas desinscreve para nao quebrar tudo
    void OnDestroy(){
        OnVidaMudada -= NotificarUI;
    }

    void FixedUpdate() {
        ChecarInteragiveis();
        if (!carregavel.sendoCarregado) Movimentacao();
    }

    //Mesmo que "Tomar dano" e "Ganhar vida"
    void MudarVida(int valor){
        //Se o valor for -1, ele tira vida
        //se for 1 ele ganha vida
        if(playerVidas + valor <= 3 && playerVidas > 0){
            playerVidas += valor;
        }else{
            //Morrer
        }
    }

    //Notificador para a UI
    private void NotificarUI(Player player, int valor){
        //paramentro eh o proprio player e o valor atual de vida
        UIManager.instance.AtualizarDisplayVida(this, valor);
    }

    #region Ferramenta

    /// <summary>
    /// Chamado quando o botão de "ataque" é pressionado
    /// </summary>
    void AcionarFerramenta() {
        Debug.Log("Acionar ferramenta");
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Acionar();
    }

    /// <summary>
    /// Chamado quando o botão de "ataque" é solto
    /// </summary>
    void SoltarFerramenta() {
        Debug.Log("Soltar ferramenta");
        if (!carregador.estaCarregando && ferramenta != null) ferramenta.Soltar();
    }

    #endregion

    /// <summary>
    /// Trata da movimentação do jogador
    /// </summary>
    void Movimentacao() {
        Vector2 input = inputActionMap["Move"].ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;

        movimentacao = (transform.right * x + transform.forward * z).normalized;

        if (movimentacao.magnitude > 0) {
            direcao = movimentacao;

            visualizarDirecao.transform.forward = direcao;
        }

        if (!podeMovimentar) return;
        
        playerRigidbody.MovePosition(transform.position + movimentacao * velocidade * Time.fixedDeltaTime);
        animacaoJogador.Mover(movimentacao);
    }

    public bool estaNoChao {
        get { return Physics.Raycast(transform.position, Vector3.down, 1.1f); }
    }

    #region Interacao

    /// <summary>
    /// Checa por objetos interagíveis no raio de interação e define o interagível mais próximo em "ultimoInteragivel"
    /// </summary>
    /// <returns>Retorna verdadeiro caso tenha um interagivel próximo ao jogador</returns>
    public bool ChecarInteragiveis() {
        // Checa por objetos interagíveis no raio de interação
        int quant = Physics.OverlapSphereNonAlloc(transform.position, raioInteracao, collidersInteragiveis, layerInteragivel);

        // Na maioria das vezes, não haverá interagíveis (se houver apenas 1 e este for o próprio player, também considera que não há)
        if (quant == 0 || (quant == 1 && collidersInteragiveis[0].gameObject == gameObject)) { 
            if (ultimoInteragivel != null) ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
            return false;
        }

        // Procura o interagível mais próximo (não podemos confiar na ordem padrão dos colliders)
        float menorDistancia = Mathf.Infinity;
        Collider interagivelMaisProximo = null;
        for (int i = 0; i < quant; i++) { // Na maioria das vezes, só haverá um interagível, e se houver mais, não será muitos (menos de 8)
            Collider collider = collidersInteragiveis[i];
            if (collider == null || collider.gameObject == gameObject) continue; // Ignora o próprio jogador

            float distancia = Vector3.Distance(transform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                menorDistancia = distancia;
                interagivelMaisProximo = collider;
            }
        }
        

        // Trata do ultimo interagivel
        if (ultimoInteragivel != null) {
            if (ultimoInteragivel.gameObject == interagivelMaisProximo.gameObject) return true; // Se o interagível mais próximo for o mesmo que o último interagível, não faz nada
            ultimoInteragivel.MostarIndicador(false);
        }

        // Trata do novo interagivel mais proximo
        Interagivel interagivel = interagivelMaisProximo.GetComponent<Interagivel>();
        interagivel.MostarIndicador(true);
        ultimoInteragivel = interagivel;

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

    #endregion

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

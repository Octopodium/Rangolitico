using UnityEngine;
using System.Collections;
using Mirror;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(Interagivel))]
public class Empurravel : MonoBehaviour, InteracaoCondicional, IRecebeTemplate, Pesavel {
    [System.Serializable]
    public class DirecaoEmpurrar {
        public bool cima = true;
        public bool baixo = true;
        public bool esquerda = true;
        public bool direita = true;
    }

    NetworkIdentity netID;
    public Transform triggerHolder;
    public GameObject encostouPorUltimoEm;

    public float distBordaInteracao = 0.5f;
    public float paddingTrigger = 0.25f;
    public DirecaoEmpurrar direcoes;

    Rigidbody rb;
    [HideInInspector] public BoxCollider col;
    Interagivel interagivel;


    bool sendoEmpurrado = false;
    Player jogadorEmpurrando = null;
    Vector3 eixo, eixoInvertido;

    Vector3 topoOffset;

    bool algoNoCaminho = false;



    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();
        interagivel = GetComponent<Interagivel>();
        netID = GetComponent<NetworkIdentity>();

        Setup();
    }

    public float GetPeso() {
        return rb != null ? rb.mass : 2f;
    }

    public void RecebeTemplate(GameObject template) {
        if (template == null) return;
        Empurravel empurravelTemplate = template.GetComponent<Empurravel>();
        if (empurravelTemplate == null) return;

        col.size = empurravelTemplate.col.size;
        col.center = empurravelTemplate.col.center;

        distBordaInteracao = empurravelTemplate.distBordaInteracao;
        paddingTrigger = empurravelTemplate.paddingTrigger;
        direcoes = empurravelTemplate.direcoes;

        Setup();
    }

    public void Setup() {
        AndadorSobChao andador = GetComponent<AndadorSobChao>();
        if (andador != null) {
            andador.SetOffsetBase(new Vector3(0, -(andador.distanciaCheckChao + col.center.y + col.size.y / 2f), 0));
            andador.tipoDeCheck = AndadorSobChao.TipoDeCheck.Box;
            andador.SetBoxRect(new Vector3(col.size.x / 2f, 0, col.size.z / 2f));
        }

        topoOffset = col.center;
        topoOffset.y += col.size.y /2f;

        CriarTriggersDeInteracao();
    }

    public void CriarTriggersDeInteracao() {
        foreach (Transform child in triggerHolder) {
            Destroy(child.gameObject);
        }

        if (direcoes.direita) CriarTriggerDeInteracao(0, 1);
        if (direcoes.esquerda) CriarTriggerDeInteracao(0, -1);
        if (direcoes.cima) CriarTriggerDeInteracao(1, 0);
        if (direcoes.baixo) CriarTriggerDeInteracao(-1, 0);
    }

    GameObject CriarTriggerDeInteracao(int xDir, int yDir) {
        GameObject trigger = new GameObject("TriggerDeInteracao_" + gameObject.name);
        trigger.layer = LayerMask.NameToLayer("Interagivel");

        trigger.transform.SetParent(triggerHolder, false);


        Vector3 direcao = transform.forward * xDir + transform.right * yDir;

        trigger.transform.localPosition = col.center + direcao;
        trigger.transform.localRotation = Quaternion.identity;
        trigger.transform.localScale = Vector3.one;


        BoxCollider boxCol = trigger.AddComponent<BoxCollider>();
        boxCol.isTrigger = true;

        Vector3 colSize = Vector3.one;
        colSize.z = Mathf.Abs(xDir) == 1 ? distBordaInteracao : (col.size.x - paddingTrigger*2f);
        colSize.y = col.size.y - paddingTrigger * 2f;
        colSize.x = Mathf.Abs(yDir) == 1 ? distBordaInteracao : (col.size.z - paddingTrigger*2f);
        boxCol.size = colSize;

        boxCol.center = new Vector3(yDir, 0, xDir) * distBordaInteracao / 2f;


        PontoInteragivel ponto = trigger.AddComponent<PontoInteragivel>();
        ponto.SetInteragivelParaRedirecionar(interagivel);
        ponto.offsetIndicador = Vector3.zero;

        OnTrigger onTrigger = trigger.AddComponent<OnTrigger>();
        onTrigger.onTriggerStayAction += col => OnTriggerAoRedor(col, yDir, xDir);
        onTrigger.onTriggerExitAction += OnSaiuDoTrigger;


        return trigger;
    }

    public bool PodeInteragir(Player jogador) {
        return !jogador.carregador.estaCarregando && jogador.transform.position.y + paddingTrigger < (transform.position.y + topoOffset.y) && !sendoEmpurrado;
    }

    public MotivoNaoInteracao NaoPodeInteragirPois(Player jogador) {
        if (jogador.carregador.aguentaCarregar < Peso.Pesado) return MotivoNaoInteracao.Fraco;
        return MotivoNaoInteracao.Nenhum;
    }

    Vector3 GetDirecaoPlayer(Player jogador) {
        Vector3 posRelativa = (jogador.transform.position - transform.position).normalized;
        bool eixoX = Mathf.Abs(posRelativa.x) > Mathf.Abs(posRelativa.z);

        return eixoX ? new Vector3(Mathf.Sign(posRelativa.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(posRelativa.z));
    }

    bool IsDirecaoPermitida(Vector3 direcao) {
        if (direcao == Vector3.left) return direcoes.esquerda;
        if (direcao == Vector3.right) return direcoes.direita;
        if (direcao == Vector3.forward) return direcoes.baixo;
        if (direcao == -Vector3.forward) return direcoes.cima;
        return false;
    }


    public void Interagir(Player jogador) {
        if (sendoEmpurrado) {
            SoltarEmpurro();
            return;
        }

        Vector3 direcao = GetDirecaoPlayer(jogador);
        if (!IsDirecaoPermitida(direcao)) return;

        if (GameManager.instance.isOnline)
            Sincronizador.instance.SetarAutoridade(netID, jogador);

        // Posiciona o jogador bem no meio da caixa
        Vector3 novaPosicaoPlayer = transform.right * direcao.x + transform.forward * direcao.z;
        novaPosicaoPlayer += novaPosicaoPlayer * distBordaInteracao;
        novaPosicaoPlayer += transform.position;
        novaPosicaoPlayer.y = jogador.transform.position.y;

        jogador.Teletransportar(novaPosicaoPlayer);

        jogadorEmpurrando = jogador;
        sendoEmpurrado = true;
        eixo = direcao;
        eixoInvertido = eixo * -1;

        jogadorEmpurrando.empurrando = true;
        StartCoroutine(EsperaAntesDeSetar());
    }

    IEnumerator EsperaAntesDeSetar() {
        // Skipa 4 frames pq assim funciona....
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        if (jogadorEmpurrando != null) {
            jogadorEmpurrando.OnPositionChange += OnMovimento;
        }
    }

    void SoltarEmpurro() {
        if (jogadorEmpurrando != null) {
            jogadorEmpurrando.empurrando = false;
            jogadorEmpurrando.OnPositionChange -= OnMovimento;
            jogadorEmpurrando = null;
        }
        
        sendoEmpurrado = false;
        eixo = Vector3.zero;
        eixoInvertido = Vector3.zero;
    }


    void OnTriggerAoRedor(Collider col, int x, int z) {
        if (!sendoEmpurrado) return;
        if (col.isTrigger) return;
        if (col.tag == "Subida") return;

        if (eixoInvertido.x == x && eixoInvertido.z == z) {
            algoNoCaminho = true;
            encostouPorUltimoEm = col.gameObject;
        }
    }

    void OnSaiuDoTrigger(Collider col) {
        if (!sendoEmpurrado) return;
        if (col.gameObject == jogadorEmpurrando.gameObject)
            SoltarEmpurro();
    }

    bool EstaIndoNoEixo(Vector3 direcaoIndo) {
        return (direcaoIndo.z == 0 && ((eixoInvertido.x > 0 && direcaoIndo.x > 0) || (eixoInvertido.x < 0 && direcaoIndo.x < 0))) || 
        (direcaoIndo.x == 0 && ((eixoInvertido.z > 0 && direcaoIndo.z > 0) || (eixoInvertido.z < 0 && direcaoIndo.z < 0)));
    }


    void OnMovimento(Vector3 variacaoPos) {
        if (!sendoEmpurrado) return;

        if (algoNoCaminho) {
            encostouPorUltimoEm = null;
            algoNoCaminho = false;
            return;
        }

        if (!EstaIndoNoEixo(variacaoPos)) {
            SoltarEmpurro();
            return;
        }

        if (Vector3.Dot(variacaoPos, jogadorEmpurrando.direcao) < 0f) return;

        
        Vector3 movimento = transform.position;
        movimento.x += Mathf.Abs(eixo.x) * variacaoPos.x;
        movimento.z += Mathf.Abs(eixo.z) * variacaoPos.z;
        transform.position = movimento;
    }

    void OnDrawGizmosSelected() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();

        Vector3 size = col.size;
        size.x += distBordaInteracao * 2;
        size.z += distBordaInteracao * 2;

        Vector3 centro = transform.position + col.center;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centro, size);

        Gizmos.color = Color.magenta;

        float raio = 0.25f;
        if (direcoes.esquerda) Gizmos.DrawSphere(centro + Vector3.left * col.size.x, raio);
        if (direcoes.direita) Gizmos.DrawSphere(centro + Vector3.right * col.size.x, raio);
        if (direcoes.baixo) Gizmos.DrawSphere(centro + Vector3.forward * col.size.z, raio);
        if (direcoes.cima) Gizmos.DrawSphere(centro - Vector3.forward * col.size.z, raio);
    }
}

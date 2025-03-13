using UnityEngine;

public class Carregador: MonoBehaviour {
    public Transform carregarTransform;
    public float forcaArremesso = 5f, alturaArremesso = 1.5f;
    [Range(0, 1)] public float influenciaDaInerciaNoArremesso = 0.33f;

    [Header("Pegar automaticamente")]
    public bool carregarSeCairNaArea = false;
    Collider[] collidersNaArea = new Collider[10];
    [Tooltip("Necessário apenas para pegar automaticamente")]
    public LayerMask layerCarregavel;

    [HideInInspector] public Carregavel carregado;
    Carregavel ultimoCarregado;
    float tempoLimpaUltimoCarregado = 0.25f; // Impede jogador de soltar e pegar um carregavel no mesmo momento
    float timerLimparUltimoCarregado = 0;

    public bool estaCarregando => carregado != null;

    void FixedUpdate() {
        if (timerLimparUltimoCarregado > 0) {
            timerLimparUltimoCarregado -= Time.fixedDeltaTime;
            if (timerLimparUltimoCarregado <= 0) {
                ultimoCarregado = null;
            }
        }

        ProcuraCarregavelNaArea();
    }

    /// <summary>
    /// Função chamada apenas se está habilitado para o carregador pegar automaticamente um objeto que caia na área (desativado por padrão)
    /// </summary>
    void ProcuraCarregavelNaArea() {
        if (!carregarSeCairNaArea || carregado != null) return;

        int quant = Physics.OverlapSphereNonAlloc(carregarTransform.position, 0.1f, collidersNaArea, layerCarregavel);
        if (quant == 0 || (quant == 1 && collidersNaArea[0].gameObject == gameObject)) return; // Na maioria dos casos, não haverá carregáveis

        float menorDistancia = Mathf.Infinity;
        Carregavel carregavelProximo = null;

        for (int i = 0; i < quant; i++) {
            Collider collider = collidersNaArea[i];
            if (collider == null || collider.gameObject == gameObject) continue; // Ignora o próprio jogador

            float distancia = Vector3.Distance(carregarTransform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                Carregavel carregavel = collider.GetComponent<Carregavel>();
                if (carregavel != null) {
                    menorDistancia = distancia;
                    carregavelProximo = carregavel;
                }
            }
        }

        if (carregavelProximo == null) return;

        Carregar(carregavelProximo);
    }

    public void Carregar(Carregavel carregavel) {
        if (carregado != null || carregavel == ultimoCarregado) return;
        
        carregado = carregavel;
        ultimoCarregado = carregavel;

        carregavel.transform.SetParent(carregarTransform);
        carregavel.transform.localPosition = Vector3.zero;

        Rigidbody cargaRigidbody = carregavel.GetComponent<Rigidbody>();
        if (cargaRigidbody != null) {
            carregavel.HandleSendoCarregado();
        }
    }

    public void Soltar(Vector3 direcao, float velocidade, bool movendo = false) {
        carregado.transform.SetParent(null);

        Rigidbody cargaRigidbody = carregado.GetComponent<Rigidbody>();

        if (cargaRigidbody != null) {
            carregado.HandleSolto();

            if (movendo)
                cargaRigidbody.linearVelocity = influenciaDaInerciaNoArremesso * (direcao * velocidade);

            Vector3 arremeco = direcao;
            arremeco.y = alturaArremesso;
            cargaRigidbody.AddForce(arremeco * forcaArremesso, ForceMode.Impulse);
        }

        carregado = null;
        timerLimparUltimoCarregado = tempoLimpaUltimoCarregado;
    }

    public Vector3[] PreverArremesso(Rigidbody rigidbody, Vector3 direcao, float forca, Vector3 velocidadeInicial) {
        int quantidadeMaxPontos = 20;
        float tempo = 10 * Time.fixedDeltaTime; // Intervalos de tempo

        Vector3[] pontos = new Vector3[quantidadeMaxPontos];
        Vector3 posicao = rigidbody.position;
        Vector3 velocidade = direcao * forca + velocidadeInicial;
        Vector3 gravidade = 0.5f * Physics.gravity * tempo * tempo;

        for (int i = 0; i < quantidadeMaxPontos; i++) {
            posicao += velocidade * tempo + gravidade;
            velocidade += (Physics.gravity * tempo);
            pontos[i] = posicao;
        }

        return pontos;
    }
}

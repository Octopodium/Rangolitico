using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : IResetavel, SincronizaMetodo
{
    [Space(10)][Header("<color=green> Info </color>")][Space(10)]
    [SerializeField] private Collider[] emCimaDaPlaca = new Collider[5];
    [SerializeField] private float pesoAtual = 0;
    [SerializeField] private bool ativado = false;

    [Space(10)][Header("<color=green> Configurações </color>")][Space(10)]

    [Tooltip("Peso minimo para que o evento de ativação ocorra")]
    [Range(1, 10)][SerializeField] private int pesoDesejado;

    [Tooltip("Meias proporções da hitbox do botão")]
    [SerializeField] private Vector3 boxHalfExtents;
    [SerializeField] private Vector3 offset;
    public UnityEvent OnAtivado, OnDesativado;

    [Header("Decalque")]
    [SerializeField] private Renderer decalRender;
    [SerializeField] private Renderer[] decalsLinkados;
    [SerializeField][ColorUsage(true, true)] private Color corAtivado, corDesativado;
    private MaterialPropertyBlock decalMPB;

    private Coroutine corotinaQuandoAtivo = null;

    // Animação:
    [SerializeField] private Animator animator;
    private AudioSource audioSource;
    public AudioClip botaoApertadoSom;
    public static readonly int pressureID = Animator.StringToHash("Pressure");


    private void Awake() {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
        decalMPB = new MaterialPropertyBlock();
    }

    public override void OnReset(){
        // Faz a checagem pra desativar o botão depois que a sala reseta.
        ChecarAtivacao();
    }

    private void OnTriggerEnter(Collider other) {
        ChecarAtivacao();
        audioSource.clip = botaoApertadoSom;
        audioSource.Play();
    }

    private void OnTriggerStay(Collider other) {
        ChecarAtivacao();
    }

    private void OnTriggerExit(Collider other) {
        ChecarAtivacao();
    }

    // Faz a checagem do peso total em cima da placa e chama os eventos de ativação ou desativação conforme necessario.
    private void ChecarAtivacao(){
        pesoAtual = CalcularPeso();

        if(pesoAtual >= pesoDesejado){
            Ativar();
        }
        else if(ativado){
            Desativar();
        }

        animator.SetFloat(pressureID, pesoAtual/pesoDesejado);
    }

    [Sincronizar]
    public void Ativar() {
        if (ativado) return;

        gameObject.Sincronizar();

        ativado = true;

        OnAtivado?.Invoke();

        corotinaQuandoAtivo = StartCoroutine(CheckSeAindaEmCima());
        TrocarCorDoDecalque(corAtivado);
    }

    private void TrocarCorDoDecalque(Color col) {
        decalMPB.SetColor("_EmissionColor", col);
        decalRender.SetPropertyBlock(decalMPB);

        if (decalsLinkados == null) return;

        foreach (Renderer render in decalsLinkados) {
            render.SetPropertyBlock(decalMPB);
        }
    }

    IEnumerator CheckSeAindaEmCima() {
        do {
            yield return new WaitForEndOfFrame();
            ChecarAtivacao();
        } while (corotinaQuandoAtivo != null);
    }

    [Sincronizar]
    public void Desativar() {
        if (!ativado) return;

        gameObject.Sincronizar();
        ativado = false;

        OnDesativado?.Invoke();

        if (corotinaQuandoAtivo != null) {
            StopCoroutine(corotinaQuandoAtivo);
        }

        TrocarCorDoDecalque(corDesativado);
    }

    // Para cada objeto com um rigidbody em cima do botão, adiciona o peso dele ao peso total.
    // obs: tem um limite de quantos objetos consegue calcular em cima do botão, o limite é ditado pelo tamanho do array emCimaDaPlaca[].
    private float CalcularPeso(){
        float peso = 0;

        int num = Physics.OverlapBoxNonAlloc(transform.position + offset * transform.localScale.y/2, boxHalfExtents, emCimaDaPlaca);

        for(int i = 0; i < num ; i++){
            Rigidbody rb = emCimaDaPlaca[i].GetComponent<Rigidbody>(); 

            if(rb != null){
                peso += rb.mass;
            }
        }

        return peso;
    }

    // Só desenha a caixa responsavel por testar objetos com peso do CalcularPeso().
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position + offset * transform.localScale.y/2, boxHalfExtents * 2);
    }

}

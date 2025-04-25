using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
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
    public UnityEvent OnAtivado, OnDesativado;

    //Gambiarra placeholder:
    MaterialPropertyBlock mpb;

    private void Start(){
        MudarCor(Color.black);
    }
    
    private void OnTriggerEnter(Collider other){
        ChecarAtivacao();
    }

    private void OnTriggerExit(Collider other){
        ChecarAtivacao();
    }

    // Faz a checagem do peso total em cima da placa e chama os eventos de ativação ou desativação conforme necessario.
    private void ChecarAtivacao(){
        pesoAtual = CalcularPeso();

        if(pesoAtual >= pesoDesejado){
            if(ativado) return;

            Debug.Log("<color=green>Botão ativado.</color>");
            ativado = true;
            MudarCor(Color.green);

            OnAtivado?.Invoke();
        }
        else if(ativado){
            Debug.Log("<color=red>Botão desativado.</color>");
            ativado = false;
            MudarCor(Color.black);

            OnDesativado?.Invoke();
        }
    }

    // Para cada objeto com um rigidbody em cima do botão, adiciona o peso dele ao peso total.
    // obs: tem um limite de quantos objetos consegue calcular em cima do botão, o limite é ditado pelo tamanho do array emCimaDaPlaca[].
    private float CalcularPeso(){
        float peso = 0;

        int num = Physics.OverlapBoxNonAlloc(transform.position + Vector3.up * transform.localScale.y/2, boxHalfExtents, emCimaDaPlaca);

        for(int i = 0; i < num ; i++){
            Rigidbody rb = emCimaDaPlaca[i].GetComponent<Rigidbody>(); 

            if(rb != null){
                peso += rb.mass;
            }
        }

        return peso;
    }

    // Metodo que eu fiz so pra ter feedback visual por enquanto.
    // deve ser substituido ou melhorado com animações ou coisas mais finalizadas depois.
    private void MudarCor(Color cor){
        Renderer renderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        mpb.SetColor("_Color", cor);
        renderer.SetPropertyBlock(mpb);
    }

    // Só desenha a caixa responsavel por testar objetos com peso do CalcularPeso().
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position + Vector3.up * transform.localScale.y/2, boxHalfExtents * 2);
    }

}

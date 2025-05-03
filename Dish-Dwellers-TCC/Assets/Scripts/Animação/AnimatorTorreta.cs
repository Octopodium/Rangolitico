using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorTorreta : MonoBehaviour
{
    private Animator animator;

    #region Parâmetros do animator
    
    public static readonly int cospe = Animator.StringToHash("Cospe");
    public static readonly int morre = Animator.StringToHash("Morre");
    public static readonly int atordoado = Animator.StringToHash("Atordoado");
    public static readonly int agarrado = Animator.StringToHash("Agarrado");

    #endregion

    float orientacao = 1;


    private void Awake(){
        animator = GetComponent<Animator>();
    }


    public void Cospe(){
        animator.SetTrigger(cospe);
    }

    public void Morre(){
        animator.SetTrigger(morre);
        animator.SetBool(atordoado, false);
        animator.SetBool(agarrado, false);
        Destroy(gameObject, 4f);
    }

    public void Atordoado(bool val){
        animator.SetBool(atordoado, val);
    }

    /// <summary>
    /// Inicia a animação de agarrar e encerra o estado de atordoado.
    /// </summary>
    /// <param name="val"></param>
    public void Agarrado(bool val){
        animator.SetBool(atordoado, false);
        animator.SetBool(agarrado, val);
    }
    
    /// <summary>
    /// Determina se a torreta deve estar virada para a direita ou esquerda baseado na direção do alvo
    /// </summary>
    /// <param name="dirAlvo"></param>
    public void Olhar(Vector3 dirAlvo){
        Vector3 escala = transform.localScale;

        if(dirAlvo.x != 0){     
            orientacao = dirAlvo.x > 0 ? -1 : 1;
            escala.x = orientacao;
        }

        transform.localScale = escala;
    }

    // Metodo inutil que eu fiz pra pensar no trem de aggro da torreta. Não deve ser usado em lugar algum :

    //Comentei pra testar outra coisa, Lima me desculpe ;-; 

    // private void SphereDetection(){
    //     // Dados arbitrarios :
    //     float detectionRay = 5;
    //     LayerMask playerLayer = 0;
    //     Transform target = null;

    //     // pega os colliders na area
    //     Collider[] playersColliders = Physics.OverlapSphere(transform.position, detectionRay, playerLayer);

    //     // Se tem ao menos um collider na area, ele se torna o novo alvo
    //     if(playersColliders.Length > 0 && target == null){
    //         target = playersColliders[0].transform;
    //     }
    // }

}

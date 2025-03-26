using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorTorreta : MonoBehaviour
{
    private Animator animator;

    #region Parâmetros do animator
    
    public static readonly int cospe = Animator.StringToHash(nameof(cospe));
    public static readonly int morre = Animator.StringToHash(nameof(morre));
    public static readonly int atordoado = Animator.StringToHash(nameof(atordoado));
    public static readonly int agarrado = Animator.StringToHash(nameof(agarrado));

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
    /// 
    /// </summary>
    /// <param name="dir"></param>
    public void Olhar(Vector3 dir){
        Vector3 escala = transform.localScale;

        if(dir.x != 0) orientacao = dir.x > 0 ? -1 : 1;
        escala.x = orientacao;

        transform.localScale = escala;
    }

}

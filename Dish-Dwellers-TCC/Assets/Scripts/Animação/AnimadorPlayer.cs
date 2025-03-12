using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimadorPLayer : MonoBehaviour
{
    private Animator animator;

    #region Parâmetros do animator
    public static readonly int Anda = Animator.StringToHash(nameof(Anda));
    public static readonly int Caindo = Animator.StringToHash(nameof(Caindo));
    public static readonly int Carrega = Animator.StringToHash(nameof(Carrega));
    public static readonly int Arremesso = Animator.StringToHash(nameof(Arremesso));
    public static readonly int Morre = Animator.StringToHash(nameof(Morre));
    public static readonly int Dano = Animator.StringToHash(nameof(Dano));
    #endregion


    private void Start(){
        animator = GetComponent<Animator>();
    }

    #region Métodos de animação

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade.
    /// </summary>
    /// <param name="velocidade"></param>
    public void Mover(Vector3 velocidade){
        Vector3 escala = transform.localScale;

        if(velocidade.x > 0){ // Vira para a esquerda
            escala.x = 1;
        }
        else if(velocidade.x < 0){// Vira para a direita
            escala.x = -1;
        }
        if(velocidade.z > 0){// Vira de costas
            Debug.Log("Vira de costas");
        }
        else if(velocidade.z < 0){ // Vira para a frente
            Debug.Log("Vira de frente");
        }

        animator.SetBool(Anda, (velocidade.magnitude > 0));
        transform.localScale = escala;
    }

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade.
    /// </summary>
    /// <param name="velocidade"></param>
    public void Mover(Vector2 velocidade){
        Mover(new Vector3(velocidade.x, velocidade.y));
    }

    /// <summary>
    /// Toca a animação de carregar objeto do personagem. Permanece na pose de carregar enquanto AtirarObjeto não for chamado.
    /// </summary>
    public void Carregar(){
        animator.SetTrigger(Carrega);
    }

    /// <summary>
    /// Toca a animação de arremesso do personagem.
    /// </summary>
    public void AtirarObjeto(){
        animator.SetTrigger(Arremesso);
    }

    /// <summary>
    /// Toca a animação de morte do personagem.
    /// </summary>
    public void Morte(){
        animator.SetTrigger(Morre);
    }

    /// <summary>
    /// Toca a animação de receber dano do personagem.
    /// </summary>
    public void TomarDano(){
        animator.SetTrigger(Dano);
    }

    #endregion
}

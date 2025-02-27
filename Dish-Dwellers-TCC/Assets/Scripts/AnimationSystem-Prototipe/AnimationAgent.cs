using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationAgent : MonoBehaviour
{
    #region Propriedades

    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Game Information"), Space(10)]
    public static readonly int Walk = Animator.StringToHash(nameof(Walk));
    public static readonly int Falling = Animator.StringToHash(nameof(Falling));
    public static readonly int Carry = Animator.StringToHash(nameof(Carry));
    public static readonly int Throw = Animator.StringToHash(nameof(Throw));
    public static readonly int Dead = Animator.StringToHash(nameof(Dead));
    public static readonly int Damage = Animator.StringToHash(nameof(Damage));
    public bool _inAciotn;
    public bool inAciotn{
        get{
            return _inAciotn;
        }
        private set{
            _inAciotn = value;
        }
    }
    public bool busy{
        get{
            // Pega informação do estado atual do animator
            int stateID = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            // Checa  se o estado atual é algum dos listados
            if(stateID == Falling ||  stateID == Dead || stateID == Damage)
                return true;
            return false;
        }
    }

    #endregion

    private void Start(){
        animator = GetComponent<Animator>();
        Move(new Vector3(0,0,0));
    }

    #region Métodos de animação

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade speed.
    /// </summary>
    /// <param name="speed"></param>
    public void Move(Vector3 speed){

        if(speed.x > 0){ // Vira para a esquerda
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.right);
        }
        else if(speed.x < 0){// Vira para a direita
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.left);
        }
        if(speed.z > 0){// Vira de costas
            Debug.Log("Vira de costas");
        }
        else if(speed.z < 0){ // Vira para a frente
            Debug.Log("Vira de frente");
        }
    }

    /// <summary>
    /// Atribui a animação de andar correta ao jogador, baseada no Vetor de velocidade speed.
    /// </summary>
    /// <param name="speed"></param>
    public void Move(Vector2 speed){
        if(speed.x > 0){ // Vira para a direita
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.right);
        }
        else if(speed.x < 0){// Vira para a esquerda
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.left);
        }
        if(speed.y > 0){// Vira de costas      
            Debug.Log("Vira de costas");
        }
        else if(speed.y < 0){ // Vira para a frente
            Debug.Log("Vira de frente");
        }

        animator.SetBool(Walk, true);
    }

    /// <summary>
    /// Toca a animação de carregar objeto do personagem. Permanece na pose de carregar enquanto ThrowObject não for chamado.
    /// </summary>
    public void PickUp(){
        animator.SetTrigger(Carry);
    }

    /// <summary>
    /// Toca a animação de arremesso do personagem.
    /// </summary>
    public void ThrowObject(){
        animator.SetTrigger(Throw);
    }

    /// <summary>
    /// Toca a animação de morte do personagem.
    /// </summary>
    public void Die(){
        animator.SetTrigger(Dead);
    }

    /// <summary>
    /// Toca a animação de receber dano do personagem.
    /// </summary>
    public void TakeDamage(){
        animator.SetTrigger(Damage);
    }

    #endregion
}

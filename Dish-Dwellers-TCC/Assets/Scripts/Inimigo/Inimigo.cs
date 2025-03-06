using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//<summary>
//Classe genérica usada para a construção de inimigos, eles herdaram seus valores e metodos base daqui.
//Herdam caso necessário, a implementação de todos os metodos não é obrigatoria.
//</summary>
public class Inimigo : MonoBehaviour
{
    #region Declarações

    [Header("Valores genéricos dos inimigos")]
    [Space(10)]
    public int vidas;
    public int dano;
    public float velocidade;

    [Header("Referências do player")]
    [Space(10)]
    public Transform target;
    public LayerMask playerLayer;
    
    protected CharacterController cc;

    #endregion

    //Metodos estão protegidos para serem usados apenas das classes ques os herdarem
    protected virtual void TomaDano(int valor)
    {
        vidas -= valor;

        if(vidas <= 0)
        {
            Morte();
        }
    }

    protected virtual void Movimento()
    {
        Vector3 mov = new Vector3(transform.position.x, 0, transform.position.z);
        cc.Move(mov * velocidade * Time.deltaTime);

        Debug.Log("inimigo se moveu");
    }

    protected virtual void Atacar()
    {
        //Debug.Log("Inimigo Atacou");
    }

    protected void Morte()
    {
        Destroy(gameObject);
    }

}

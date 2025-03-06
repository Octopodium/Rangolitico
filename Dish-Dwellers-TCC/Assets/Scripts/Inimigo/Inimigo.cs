using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inimigo : MonoBehaviour
{
    #region Declarações

    [Header("Valores genéricos dos inimigos")]
    [Space(10)]
    public int vidas;
    public int dano;
    public float velocidade;

    [Header("Referências do player")]
    public Transform target;
    public LayerMask playerLayer;
    
    protected CharacterController cc;

    #endregion

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

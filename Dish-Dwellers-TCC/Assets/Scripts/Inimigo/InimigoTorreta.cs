using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tenho que passar esse código para português depois... :p
public class InimigoTorreta : Inimigo
{
    #region Declaration

    [Header("Referências de ação do inimigo")]
    [Space(10)]
    public GameObject projectile;
    public Transform fireAction;

    [Header("Valores de ação do inimigo torreta")]
    [Space(10)]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;

    #endregion

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        cc = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        ChecagemDeZonas();
        Movimento();
        Atacar();
    }

    #region Métodos Genéricos que foram herdados da Classe Inimigo
    protected override void ChecagemDeZonas()
    {
        base.ChecagemDeZonas();
    }

    protected override void Movimento()
    {        
        if(_playerNoCampoDeVisao)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero) // Garante que o inimigo rotacione o apenas no eixo Y
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * velocidade);
            }
        }
    }

    protected override void Atacar()
    {
        if(_playerNaZonaDeAtaque && Time.time > nextFire)
        {
            base.Atacar();
            nextFire = Time.time + fireRate;
            Instantiate(projectile, fireAction.transform.position, transform.rotation); 
        }
    }

    protected override void TomaDano(int valor)
    {
        base.TomaDano(valor);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, campoDeVisao);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, zonaDeAtaque);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InimigoTorreta : Inimigo
{
    #region Declaration

    [Header("References")]
    [Space(10)]
    public GameObject projectile;
    public Transform fireAction;

    [Header("Range zone values")]
    [Space(10)]
    [SerializeField] private float atkZone;
    [SerializeField] private float sightZone;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;

    private bool _canFollow;
    private bool _playerInSightZone;
    private bool _playerInAtkZone;

    #endregion

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        _playerInSightZone = Physics.CheckSphere(transform.position, sightZone, playerLayer);
        _playerInAtkZone = Physics.CheckSphere(transform.position, atkZone, playerLayer);
        
        Movimento();
        Atacar();
    }

    protected override void Movimento()
    {        
        if(_playerInSightZone)
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
        if(_playerInAtkZone && Time.time > nextFire)
        {
            base.Atacar();
            nextFire = Time.time + fireRate;
            Instantiate(projectile, fireAction.transform.position, transform.rotation); 
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightZone);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, atkZone);
    }
}

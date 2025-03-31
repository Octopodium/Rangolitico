using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Classe base de todos os inimigos, com interfaces implementadas para tratar, dano, movimento e percepção de ambiente.
/// Também trata de inicializar a máquina de estados dos inimigos, e contém as instâncias bases para todos os estados.
/// </summary>

[RequireComponent(typeof(CharacterController))] [RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour, IDamageable, IEnemyMovement, ICheckers
{
    [Header("Valores do Inimigo")]
    [field: SerializeField] public int maxHealth { get; set; } = 3;
    public int currentHealth { get; set; }
    public float speedMov { get; set; }

    [Header("Valores das Zonas de Percepção")]
    [field: SerializeField] public float sightZone { get; set;}
    [field: SerializeField] public float attackZone { get; set;}

    [Header("Referências do Player")]
    public LayerMask playerLayer;
    Vector3 enemyMovement;

    public NavMeshAgent agent;
    [HideInInspector] public CharacterController cc { get; set; }
    [HideInInspector] public bool _canMove { get; set; }
    [HideInInspector] public bool _playerInSightZone { get; set;}
    [HideInInspector] public bool _playerInAttackZone { get; set;}


    #region StateMachineVariables
    public EnemyStateMachine enemyStateMachine {get; set;}
    public EnemyIdleState enemyIdleState {get; set;}
    public EnemyChaseState enemyChaseState {get; set;}
    public EnemyAttackState enemyAttackState {get; set;}

    #endregion

    #region ScriptableObjectVariables
    [SerializeField] private IdleSOBase enemyIdleBase;
    [SerializeField] private ChaseSOBase enemyChaseBase;
    [SerializeField] private AttackSOBase enemyAttackBase;

    public IdleSOBase enemyIdleBaseInstance {get; set;}
    public ChaseSOBase enemyChaseBaseInstance {get; set;}
    public AttackSOBase enemyAttackBaseInstance {get; set;}

    #endregion
    
    protected virtual void Awake()
    {   
        enemyIdleBaseInstance = Instantiate(enemyIdleBase);
        enemyChaseBaseInstance = Instantiate(enemyChaseBase);
        enemyAttackBaseInstance = Instantiate(enemyAttackBase);

        enemyStateMachine = new EnemyStateMachine();

        enemyIdleState = new EnemyIdleState(this, enemyStateMachine);
        enemyChaseState = new EnemyChaseState(this, enemyStateMachine);
        enemyAttackState = new EnemyAttackState(this, enemyStateMachine);

        cc = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        enemyIdleBaseInstance.Initialize(gameObject, this);
        enemyChaseBaseInstance.Initialize(gameObject, this);
        enemyAttackBaseInstance.Initialize(gameObject, this);

        enemyStateMachine.Initialize(enemyIdleState); //passando um estado padrão para Iniciar a máquina de estados
    }

    protected virtual void Update()
    {
        enemyStateMachine.currentEnemyState.FrameUpdate();
    }  

    protected virtual void FixedUpdate()
    {
        enemyStateMachine.currentEnemyState.PhysicsUpdate();
        CheckZones();
    }
    
    /// <summary>
    /// Método base para checagem de zonas, que vai definir qual ação o inimigo ir tomar
    /// ou em qual estado ele se encontra.
    /// </summary>
    public void CheckZones()
    {
        _playerInSightZone = Physics.CheckSphere(transform.position, sightZone, playerLayer);
        _playerInAttackZone = Physics.CheckSphere(transform.position, attackZone, playerLayer);
    }

    public void TakeDamage(int value)
    {
        currentHealth -= value;

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public void Movement()
    {
        enemyMovement = new Vector3(enemyMovement.x, 0f,enemyMovement.z);
        cc.Move(speedMov * enemyMovement * Time.deltaTime);
    }

    public void Die()
    {
        //Lógica de morte...
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightZone);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackZone);
    }
}

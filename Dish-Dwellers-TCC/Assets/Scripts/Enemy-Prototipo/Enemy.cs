using UnityEngine;

public class Enemy : MonoBehaviour
{
   #region Declaration

    [Header("References")]
    [Space(10)]
    public Transform target;
    public Transform rotatePoint;
    public LayerMask playerLayer;
    public GameObject projectile;
    public Transform fireAction;

    [Header("Range zone values")]
    [Space(10)]
    [SerializeField] private float atkZone;
    [SerializeField] private float sightZone;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;

    private Rigidbody rb;

    private bool _canFollow;
    private bool _playerInSightZone;
    private bool _playerInAtkZone;

    #endregion

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _playerInSightZone = Physics.CheckSphere(transform.position, sightZone, playerLayer);
        _playerInAtkZone = Physics.CheckSphere(transform.position, atkZone, playerLayer);
        FollowPlayer();
        AttackPlayer();
    }

    private void FollowPlayer()
    {
        if(_playerInSightZone) transform.LookAt(target.position);
    }

    private void AttackPlayer()
    {
        if(_playerInAtkZone && Time.time > nextFire)
        {
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

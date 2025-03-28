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
    [SerializeField] Vector3 direction;

    //Lima:
    [SerializeField] private AnimatorTorreta animator;

    #endregion

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<AnimatorTorreta>();
    }

    void FixedUpdate()
    {
        base.ChecagemDeZonas();
        Movimento();
        Atacar();
    }

    #region Métodos Genéricos que foram herdados da Classe Inimigo
    protected override void Movimento()
    {        
        if(_playerNoCampoDeVisao)
        {
            direction = target.position - transform.position;
            animator.Olhar(direction);
            direction.y = 0;
        }
    }

    public override void Atacar()
    {
        if(_playerNaZonaDeAtaque && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            GameObject newProjectile = Instantiate(projectile, fireAction.transform.position, target.rotation);
            newProjectile.GetComponent<Projectile>().owner = this.gameObject; // Atribui o inimigo como "dono" do projétil
            base.Atacar();
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

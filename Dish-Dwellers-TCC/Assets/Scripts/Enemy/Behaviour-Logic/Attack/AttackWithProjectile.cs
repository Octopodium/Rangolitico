using UnityEngine;

[CreateAssetMenu(fileName = "AttackWithProjectile", menuName = "AttackWithProjectile")]
public class AttackWithProjectile : AttackSOBase
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;
    [SerializeField] private Transform fireAction;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }
    public override void DoUpdateLogic()
    {
        base.DoUpdateLogic();

        if(Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(projectile, fireAction.transform.position, transform.rotation); 
        }
    }
    public override void DoResetValues(){}
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}

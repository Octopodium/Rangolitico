using UnityEngine;

[CreateAssetMenu(fileName = "ChaseDirectToPlayer", menuName = "ChaseDirectToPlayer")]
public class ChaseDirectToPlayer : ChaseSOBase
{   
    [SerializeField] private float chaseingTime = 3f;

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
        
        enemy.agent.SetDestination(playerTransform.position);

        if(!enemy._playerInSightZone && Time.time >= chaseingTime)
        {
            enemy.enemyStateMachine.ChangeState(enemy.enemyIdleState);
        }
    }
    public override void DoResetValues(){}
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}

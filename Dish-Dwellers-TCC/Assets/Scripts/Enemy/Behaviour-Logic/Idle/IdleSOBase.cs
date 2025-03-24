using UnityEngine;

public class IdleSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;
    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public virtual void DoEnterLogic(){}
    public virtual void DoExitLogic(){DoResetValues();}
    public virtual void DoUpdateLogic()
    {
        if(enemy._playerInSightZone)
        {
            enemy.enemyStateMachine.ChangeState(enemy.enemyChaseState);
        }
    }
    public virtual void DoResetValues(){}
}

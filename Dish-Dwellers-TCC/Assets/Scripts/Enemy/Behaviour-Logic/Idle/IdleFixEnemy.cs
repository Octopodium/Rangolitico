using UnityEngine;

[CreateAssetMenu(fileName = "EnemyFixIdle", menuName = "EnemyFixIdle")]
public class IdleFixEnemy : IdleSOBase
{   
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
    }
    public override void DoResetValues(){}
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}

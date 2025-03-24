using UnityEngine;

[CreateAssetMenu(fileName = "AttackExplode", menuName = "AttackExplode")]
public class AttackExplode : AttackSOBase
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
        Destroy(gameObject);
    }
    public override void DoResetValues(){}
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}

using UnityEngine;

public class AttackSOBase : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;
    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.enemy = enemy;
        transform = gameObject.transform;
        this.gameObject = gameObject;

        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public virtual void DoEnterLogic(){}
    public virtual void DoExitLogic(){DoResetValues();}
    public virtual void DoUpdateLogic(){}
    public virtual void DoResetValues(){}
}

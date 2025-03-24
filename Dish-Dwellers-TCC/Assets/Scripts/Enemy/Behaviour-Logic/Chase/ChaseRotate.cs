using UnityEngine;

[CreateAssetMenu(fileName = "ChaseRotate", menuName = "ChaseRotate")]
public class ChaseRotate : ChaseSOBase
{
    public float rotateSpeed;
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
    
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero) // Garante que o inimigo rotacione o apenas no eixo Y
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed);
        }
    }
    public override void DoResetValues(){}
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}

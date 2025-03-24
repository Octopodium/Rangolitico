using UnityEngine;

/// <summary>
/// classe de estados base que os inimigos vão herdar, e seram alterados na maquina da estados do inimigo
/// </summary>
public class EnemyState 
{
    protected Enemy enemy;
    protected EnemyStateMachine enemyStateMachine;

    //Construtor da classe de Estados base
    public EnemyState(Enemy enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    //Responsavel pelos estados do inimigo
    public virtual void EnterState(){}
    public virtual void ExitState(){}
    public virtual void FrameUpdate(){}
    public virtual void PhysicsUpdate(){}
}

using UnityEngine;

/// <summary>
/// Maquina de estados do inimigo responsavel por lidar com a troca de estados
/// </summary>
public class EnemyStateMachine
{
    public EnemyState currentEnemyState {get; set;}

    public void Initialize(EnemyState startingState)
    {
        currentEnemyState = startingState;
        currentEnemyState.EnterState();
    }

    public void ChangeState(EnemyState newEnemyState)
    {
        currentEnemyState.ExitState();
        currentEnemyState = newEnemyState;
        currentEnemyState.EnterState();
    }
    
}

using UnityEngine;

public interface IEnemyMovement
{
    CharacterController cc {get; set;}
    float speedMov {get; set;}
    bool _canMove {get; set;}

    void Movement();
}

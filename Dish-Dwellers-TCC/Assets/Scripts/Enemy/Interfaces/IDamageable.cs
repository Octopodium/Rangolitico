using UnityEngine;

public interface IDamageable
{
    int maxHealth {get; set;}
    int currentHealth {get; set;}
    
    void TakeDamage(int value); 
    void Die();
}

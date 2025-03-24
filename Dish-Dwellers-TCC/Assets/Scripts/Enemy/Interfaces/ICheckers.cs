using UnityEngine;

public interface ICheckers
{
   public float sightZone {get; set;}
   public float attackZone {get; set;}

   public bool _playerInSightZone { get; set;}
   public bool _playerInAttackZone { get; set;}

   void CheckZones();
   void OnDrawGizmos();
}

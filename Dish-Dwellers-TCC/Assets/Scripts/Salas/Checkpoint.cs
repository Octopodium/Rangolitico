using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour {
    public Transform[] spawnPoints = new Transform[2];
    [HideInInspector]public BoxCollider col;
    bool habilitado = false;


    private void Start() {
        Setup();
    }

    private void OnValidate() {
        Setup();
    }

    private void Setup() {
        col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        col.center = new Vector3(0, col.size.y / 2, 0);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            HabilitarCheckPoint();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
        
        
    }

    private void HabilitarCheckPoint(){
        if (habilitado) return;

        sala sala = GameManager.instance.salaAtual;
        sala.spawnPoints = spawnPoints;

        habilitado = true;
    }
}

using UnityEngine;

public class ProjetilDoGancho : MonoBehaviour {
    Gancho gancho;
    float velocidade;
    Vector3 direcao;

    

    public void Inicializar(Gancho gancho, Vector3 direcao, float velocidade) {
        this.gancho = gancho;
        this.direcao = direcao;
        this.velocidade = velocidade;

        Collider collider = GetComponent<Collider>();
        collider.includeLayers = gancho.layerGanchavel;
        collider.excludeLayers = ~(gancho.layerGanchavel);
    }

    void FixedUpdate() {
        transform.position += direcao * velocidade * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider collider) {
        Destroy(gameObject);

        Ganchavel ganchavel = collider.gameObject.GetComponent<Ganchavel>();
        if (ganchavel != null && ganchavel.PodeSerGanchado()) {
            gancho.SetarGanchado(ganchavel);
        }
    }
}

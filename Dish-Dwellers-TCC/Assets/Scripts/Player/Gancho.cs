using UnityEngine;

public class Gancho : MonoBehaviour, Ferramenta {
    public LayerMask layerGanchavel, layerNaoGanchavel;
    public LineRenderer lineRenderer;
    public Transform ganchoSpawn;
    public GameObject ganchoPrefab;
    public float distanciaMaxima = 10f;
    public float velocidadeGancho = 20f;

    GameObject gancho;
    Ganchavel ganchado;

    public void Acionar() {
        if (gancho == null) {
            gancho = Instantiate(ganchoPrefab, ganchoSpawn.position, Quaternion.identity);
            ProjetilDoGancho projetil = gancho.GetComponent<ProjetilDoGancho>();
            projetil.Inicializar(this, ganchoSpawn.forward, velocidadeGancho);
        } else if (gancho != null) {
            Destroy(gancho);
            UpdateCorda(null);
        } else if (ganchado != null) {
            ganchado.onDesganchado.Invoke();
            ganchado = null;
            UpdateCorda(null);
        }
    }

    public void Soltar() {
        
    }

    public void DestruirGancho() {
        Destroy(gancho);
        UpdateCorda(null);
    }

    public void SetarGanchado(Ganchavel ganchavel) {
        ganchado = ganchavel;
        UpdateCorda(ganchavel.transform);
    }

    public void UpdateCorda(Transform target) {
        if (target == null) {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, ganchoSpawn.position);
        lineRenderer.SetPosition(1, target.position);
    }

    void FixedUpdate() {
        if (gancho != null) {
            UpdateCorda(gancho.transform);

            if (Vector3.Distance(ganchoSpawn.position, gancho.transform.position) > distanciaMaxima) {
                DestruirGancho();
            }
        } else if (ganchado != null) {
            UpdateCorda(ganchado.transform);

            if (Vector3.Distance(ganchoSpawn.position, ganchado.transform.position) > distanciaMaxima) {
                ganchado.onDesganchado.Invoke();
                ganchado = null;
                UpdateCorda(null);
            }
        }
    }

}

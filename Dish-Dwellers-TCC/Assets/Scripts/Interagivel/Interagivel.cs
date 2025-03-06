using UnityEngine;

public class Interagivel : MonoBehaviour {

    [Header("Indicador de interação")]
    public GameObject indicadorPrefab;
    GameObject indicador;
    public Vector3 offsetIndicador = Vector3.up;


    // Awake: coisas que são da própria classe
    void Awake() {
        if (indicadorPrefab) {
            indicador = Instantiate(indicadorPrefab, transform.position + offsetIndicador, Quaternion.identity);
            indicador.transform.SetParent(transform);
            indicador.SetActive(false);
        } else {
            Debug.LogWarning("Indicador de interação não configurado");
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position + offsetIndicador, Vector3.one * 0.1f);
    }

    public void MostarIndicador(bool mostrar) {
        if (indicador) indicador.SetActive(mostrar);
    }

    public void Interagir(Player jogador) {
        Interacao interacao = GetComponent<Interacao>();
        if (interacao != null) interacao.Interagir(jogador);
    }
}

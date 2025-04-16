using UnityEngine;

public class Interagivel : MonoBehaviour {

    [Header("Indicador de interação")]
    public GameObject indicadorPrefab;
    //GameObject indicador;
    public Vector3 offsetIndicador = Vector3.up;


    void Start() {
        GameManager.instance.controle.OnIndicadorChange += OnIndicadorChange;
        indicadorPrefab = GameManager.instance.controle.indicadorAtual;
    }

    void OnDestroy() {
        if (GameManager.instance == null || GameManager.instance.controle == null) return;
        GameManager.instance.controle.OnIndicadorChange -= OnIndicadorChange;
    }

    
    public void MostarIndicador(bool mostrar) {
        if (indicadorPrefab) {
            indicadorPrefab.transform.position = transform.position + offsetIndicador;
            indicadorPrefab.SetActive(mostrar);
        }
    }

    public void Interagir(Player jogador) {
        Interacao interacao = GetComponent<Interacao>();
        if (interacao != null) interacao.Interagir(jogador);
    }


    public void OnIndicadorChange(GameObject novoIndicador) {
        if (indicadorPrefab != null && indicadorPrefab.activeSelf) {
            indicadorPrefab.SetActive(false);
            indicadorPrefab = novoIndicador;
            indicadorPrefab.SetActive(true);
        } else {
            indicadorPrefab = novoIndicador;
        }
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position + offsetIndicador, Vector3.one * 0.1f);
    }
}

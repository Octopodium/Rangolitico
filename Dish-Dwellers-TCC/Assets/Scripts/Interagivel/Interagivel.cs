using UnityEngine;

public class Interagivel : MonoBehaviour {

    [Header("Indicador de interação")]
    public Indicador indicador;
    //GameObject indicador;
    public Vector3 offsetIndicador = Vector3.up;


    void Start() {
        GameManager.instance.controle.OnIndicadorChange += OnIndicadorChange;
        indicador = GameManager.instance.controle.indicadorAtual;

        Sincronizavel sinc = gameObject.GetComponent<Sincronizavel>();
        if (sinc == null) {
            sinc = gameObject.AddComponent<Sincronizavel>();
        }
    }

    void OnDestroy() {
        if (GameManager.instance == null || GameManager.instance.controle == null) return;
        GameManager.instance.controle.OnIndicadorChange -= OnIndicadorChange;
    }

    
    public void MostarIndicador(bool mostrar) {
        if (indicador) {
            if (mostrar) indicador.Mostrar(this);
            else indicador.Esconder(this);
        }
    }

    public void Interagir(Player jogador) {
        Interacao interacao = GetComponent<Interacao>();
        if (interacao != null) interacao.Interagir(jogador);
    }


    public void OnIndicadorChange(Indicador novoIndicador) {
        if (novoIndicador == indicador) return;

        if (indicador != null && indicador.interagivel == this) {
            indicador.Esconder(this);
            indicador = novoIndicador;
            indicador.Mostrar(this);
        } else {
            indicador = novoIndicador;
        }
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position + offsetIndicador, Vector3.one * 0.1f);
    }
}

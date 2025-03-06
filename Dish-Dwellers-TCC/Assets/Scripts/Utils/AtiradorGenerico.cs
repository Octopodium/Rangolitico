using UnityEngine;

// Script de teste apenas para exemplificar a utilização de interfaces
[RequireComponent(typeof(Interagivel))]
public class AtiradorGenerico : MonoBehaviour, Interacao {
    public Transform saidaDoTiro;
    public GameObject tiroPrefab;
    public float tiroTimer = 1f;
    float tiroTimerCounter = 0f;

    public bool ativo = true;

    void FixedUpdate() {
        if (!ativo) return;

        tiroTimerCounter += Time.fixedDeltaTime;
        if (tiroTimerCounter >= tiroTimer) {
            Atirar();
            tiroTimerCounter = 0f;
        }
    }

    public void Atirar() {
        Instantiate(tiroPrefab, saidaDoTiro.position, saidaDoTiro.rotation);
    }

    public void Interagir(Player jogador) {
        ativo = !ativo;
    }
}

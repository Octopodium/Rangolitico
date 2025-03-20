using UnityEngine;

public class Escudo : MonoBehaviour, Ferramenta {
    public GameObject protecao;
    
    float dirHorizontal = 1, dirVertical = -1;
    Vector3 direcaoProtecao = Vector3.zero;

    Player jogador;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar() {
        protecao.SetActive(true);
    }

    public void Soltar() {
        protecao.SetActive(false);
    }
    
    void FixedUpdate() {
        if (jogador.direcao.x != 0) dirHorizontal = Mathf.Sign(jogador.direcao.x);
        if (jogador.direcao.z != 0) dirVertical = Mathf.Sign(jogador.direcao.z);

        direcaoProtecao.x = dirHorizontal;
        direcaoProtecao.z = dirVertical;
        protecao.transform.forward = direcaoProtecao;
    }
}

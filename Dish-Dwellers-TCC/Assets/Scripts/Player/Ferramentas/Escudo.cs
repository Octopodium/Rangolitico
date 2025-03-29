using UnityEngine;

public class Escudo : MonoBehaviour, Ferramenta 
{
    public GameObject protecao;
    
    Vector3 direcaoProtecao = Vector3.zero;

    Player jogador;

    public void Inicializar(Player jogador) 
    {
        this.jogador = jogador;
    }

    public void Acionar() 
    {
        protecao.SetActive(true);
        jogador.escudoAtivo = true; 
        jogador.MostrarDirecional(true);
    }

    public void Soltar() 
    {
        protecao.SetActive(false);
        jogador.escudoAtivo = false; 
        jogador.MostrarDirecional(false);
    }
    
    void FixedUpdate() 
    {
        direcaoProtecao.x = jogador.direcao.x;
        direcaoProtecao.z = jogador.direcao.z;

        if (direcaoProtecao.magnitude > 0)
            protecao.transform.forward = direcaoProtecao;
    }
}
using UnityEngine;

public class Escudo : MonoBehaviour, Ferramenta 
{
    public GameObject protecao;

    Vector3 direcaoProtecao = Vector3.zero;
    public bool acionada { get; protected set; } = false;
    
    Player jogador;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar()
    {
        if (acionada) return;

        protecao.SetActive(true);
        jogador.escudoAtivo = true;
        jogador.MostrarDirecional(true);
        
        acionada = true;
    }

    public void Soltar()
    {
        if (!acionada) return;

        protecao.SetActive(false);
        jogador.escudoAtivo = false;
        jogador.MostrarDirecional(false);
        
        acionada = false;
    }

    public void Cancelar()
    {
        Soltar();
    }
    
    void FixedUpdate() 
    {
        direcaoProtecao.x = jogador.direcao.x;
        direcaoProtecao.z = jogador.direcao.z;

        if (direcaoProtecao.magnitude > 0)
            protecao.transform.forward = direcaoProtecao;
    }
}
using UnityEngine;
using UnityEngine.Events;

public class Porta : MonoBehaviour, Interacao{
    [Header("Estado da porta")]

    public UnityEvent destranca;

    public void Interagir(Player jogador){
        // if(jogador.carregando == chave) destranca.Invoke();
        //else animaçãoDeTrancada.Play()
    }
}

using UnityEngine;

public class Porta : MonoBehaviour, Interacao{

    public void Interagir(Player jogador){
        GameManager.instance.PassaDeSala();
    }
}

using UnityEngine;

public class Porta : MonoBehaviour, Interacao{

    public void Interagir(Player jogador){
        Debug.Log("oioioi");
        GameManager.instance.PassaDeSala();
    }
}

using UnityEngine;

public class matadorDeGameManager : MonoBehaviour
{
    public bool setPartidaConcluida = false;
    private void Awake() {
        if (setPartidaConcluida) GameManager.instance.SetPartidaConcluida();
        Destroy(GameManager.instance.gameObject);
        GameManager.instance = null;
    }
}

using UnityEngine;

public class matadorDeGameManager : MonoBehaviour
{
    private void Awake() {
        Destroy(GameManager.instance.gameObject);
        GameManager.instance = null;
    }
}

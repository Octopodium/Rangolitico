using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Actions input;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        input = new Actions();
        input.Enable();
    }
}

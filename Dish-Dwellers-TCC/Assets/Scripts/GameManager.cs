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


    #region Protótipo sistema de salas do Lima
    [Space(10)][Header("<color=green>Informações da sala :</color>")]
    [SerializeField] private bool loadingFinished; // Informa se o carregamento da proxima sala ja foi finalizado.
    [HideInInspector] public Sala sala; // Sala atual

    #endregion
}

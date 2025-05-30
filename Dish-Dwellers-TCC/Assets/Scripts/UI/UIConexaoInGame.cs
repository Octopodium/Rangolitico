using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIConexaoInGame : MonoBehaviour {
    public static UIConexaoInGame instancia;
    public Text conectandoLocalP1, conectandoLocalP2;

    void Awake() {
        if (instancia == null) {
            instancia = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void SetConectando(QualPlayer qualPlayer) {
        if (qualPlayer == QualPlayer.Player1) {
            conectandoLocalP1.gameObject.SetActive(true);
        } else if (qualPlayer == QualPlayer.Player2) {
            conectandoLocalP2.gameObject.SetActive(true);
        }
    }

    public void SetConectado(QualPlayer qualPlayer) {
        if (qualPlayer == QualPlayer.Player1) {
            conectandoLocalP1.gameObject.SetActive(false);
        } else if (qualPlayer == QualPlayer.Player2) {
            conectandoLocalP2.gameObject.SetActive(false);
        }
    }
}

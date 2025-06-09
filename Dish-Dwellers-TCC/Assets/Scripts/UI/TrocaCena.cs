using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocaCena : MonoBehaviour {
    public void TrocarDeCena(string nomeDaCena) {
        SceneManager.LoadScene(nomeDaCena);
    }
}

using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MenuPrincipal : MonoBehaviour {
    public TMPro.TMP_Text buildVersionTxt;


    void Awake() {
        buildVersionTxt.text = "Build v" + Application.version;
    }

    void Start() {
        PrevencaoDeBugs();
    }

    public void PrevencaoDeBugs() {
        Time.timeScale = 1f;

        if (GameManager.instance != null) {
            Destroy(GameManager.instance.gameObject);
            Debug.LogWarning("Instancia de GameManager encontrada no Menu Principal e destruída para evitar bugs. Isso normalmente acontece.");
        }

        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null) {
            Destroy(networkManager.gameObject);
            Debug.LogWarning("Instancia de NetworkManager encontrada no Menu Principal e destruída para evitar bugs. Isso não deveria acontecer.");
        }
    }

    public void QuitJogo(){
        Application.Quit();
    }

    public void IrParaLink(string link) {
        // Abre o link no navegador padrão do dispositivo
        Application.OpenURL(link);
    }
}

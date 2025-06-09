using UnityEngine;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour {
    public TMPro.TMP_Text buildVersionTxt;


    void Awake() {
        buildVersionTxt.text = "Build v" + Application.version;
    }

    public void QuitJogo(){
        Application.Quit();
    }

    public void IrParaLink(string link) {
        // Abre o link no navegador padrão do dispositivo
        Application.OpenURL(link);
    }
}

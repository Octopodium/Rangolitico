using UnityEngine;

public class IrParaLink : MonoBehaviour {
    public void IrPara(string link) {
        // Abre o link no navegador padrão do dispositivo
        Application.OpenURL(link);
    }
}
